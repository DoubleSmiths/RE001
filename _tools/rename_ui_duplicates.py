"""
Rename duplicate child GameObject names in Unity UI prefabs.

Strategy:
- Parse each prefab YAML to collect GameObjects, Transforms, Components.
- Detect component types via known UGUI/TMP script GUIDs.
- Identify duplicate GameObject names within each prefab.
- For each duplicate group:
  - If the name is referenced by an enum in the corresponding C# script,
    keep the first occurrence; rename the others.
  - Otherwise, rename all occurrences with Unreal-style names.
- "Careless" names (GameObject, _, Image, Text, etc.) are always replaced when
  they form duplicates.
- New name format: <Prefix>_<ParentContext>_<NN>
  - Prefix: Img_/Txt_/Btn_/Tgl_/Scr_/Sld_/Sb_/Mask_/Anim_/Panel_/Obj_
  - ParentContext: parent GameObject name (sanitised, max 24 chars)
  - NN: 2-digit index, 01-based per group

Outputs:
- Modifies *.prefab in place (text replacement of m_Name lines targeted by GO id).
- Writes a Markdown report to Documents/UI_Prefab_Rename_Report.md.
"""
from __future__ import annotations
import re
import json
from pathlib import Path
from collections import defaultdict, OrderedDict
from typing import Dict, List, Tuple, Optional

# ─────────────────────────────────────────────────────────────
# Paths
# ─────────────────────────────────────────────────────────────
PROJECT_ROOT = Path(r"C:\stopresent\Unity\RE001")
PREFAB_ROOT = PROJECT_ROOT / "Assets" / "@Resources" / "Prefab" / "UI"
SCRIPT_ROOT = PROJECT_ROOT / "Assets" / "@Scripts" / "UI"
DOCS_ROOT = PROJECT_ROOT / "Documents"
REPORT_PATH = DOCS_ROOT / "UI_Prefab_Rename_Report.md"

# ─────────────────────────────────────────────────────────────
# Component script GUIDs (UGUI + TMP)
# ─────────────────────────────────────────────────────────────
GUID_TO_TYPE = {
    "fe87c0e1cc204ed48ad3b37840f39efc": "Image",
    "1344c3c82d62a2a41a3576d8abb8e3ea": "RawImage",
    "5f7201a12d95ffc409449d95f23cf332": "Text",
    "f4688fdb7df04437aeb418b961361dc5": "TMP_Text",
    "4e29b1a8efbd4b44bb3f3716e73f07ff": "Button",
    "9085046f02f69544eb97fd06b6048fe2": "Toggle",
    "1aa08ab6e0800fa44ae55d278d1423e3": "ScrollRect",
    "31a19414c41e5ae4aae2af33fee712f6": "Mask",
    "3312d7739989d2b4e91e6319e9a96d76": "RectMask2D",
    "2a4db7a114972834c8e4117be1d82ba3": "Scrollbar",
    "67db9e8f0e2ae9c40bc1e2b64352a6b4": "Slider",
}

# Unreal-style prefixes
TYPE_TO_PREFIX = {
    "Image": "Img",
    "RawImage": "Img",
    "Text": "Txt",
    "TMP_Text": "Txt",
    "Button": "Btn",
    "Toggle": "Tgl",
    "ScrollRect": "Scr",
    "Mask": "Mask",
    "RectMask2D": "Mask",
    "Scrollbar": "Sb",
    "Slider": "Sld",
    "Animator": "Anim",
    "ParticleSystem": "VFX",
    "Canvas": "Cv",
}

# Class IDs we care about (Unity built-in)
CLASS_GAMEOBJECT = "1"
CLASS_TRANSFORM = "4"
CLASS_RECTTRANSFORM = "224"
CLASS_MONOBEHAVIOUR = "114"
CLASS_CANVAS = "223"
CLASS_CANVAS_RENDERER = "222"
CLASS_ANIMATOR = "95"
CLASS_PARTICLE_SYSTEM = "198"
CLASS_PARTICLE_RENDERER = "199"

# Names considered "careless" — always replaced when duplicated.
CARELESS_PATTERNS = [
    re.compile(r"^GameObject(_\d+| \(\d+\))?$"),
    re.compile(r"^Image(_\d+)?$"),
    re.compile(r"^Img(_\d+)?$"),
    re.compile(r"^Text(_\d+)?$"),
    re.compile(r"^text(_\d+)?$"),
    re.compile(r"^Mask(_\d+)?$"),
    re.compile(r"^mask(_\d+)?$"),
    re.compile(r"^Frame(_\d+)?$"),
    re.compile(r"^frame(_\d+)?$"),
    re.compile(r"^Background(_\d+)?$"),
    re.compile(r"^_+(\d+)?$"),
    re.compile(r"^Icon(_\d+)?$"),
    re.compile(r"^Glow(_\d+)?$"),
    re.compile(r"^Txt(_\d+)?$"),
    re.compile(r"^Btn(_\d+)?$"),
]


def is_careless(name: str) -> bool:
    return any(p.match(name) for p in CARELESS_PATTERNS)


# ─────────────────────────────────────────────────────────────
# YAML doc splitter
# ─────────────────────────────────────────────────────────────
DOC_SEP_RE = re.compile(
    r"^--- !u!(\d+) &(\d+)(?:\s+stripped)?\s*$",
    re.MULTILINE,
)
NAME_LINE_RE = re.compile(r"^(\s*m_Name:\s*)(.+?)(\s*)$", re.MULTILINE)
M_GO_RE = re.compile(r"m_GameObject:\s*\{fileID:\s*(-?\d+)")
M_FATHER_RE = re.compile(r"m_Father:\s*\{fileID:\s*(-?\d+)")
M_COMP_RE = re.compile(r"-\s*component:\s*\{fileID:\s*(-?\d+)\}")
M_SCRIPT_GUID_RE = re.compile(r"m_Script:.*guid:\s*([a-f0-9]{32})")


def split_docs(text: str) -> List[Tuple[str, str, str]]:
    """Yield (class_id, file_id, body) for each YAML doc."""
    matches = list(DOC_SEP_RE.finditer(text))
    docs = []
    for i, m in enumerate(matches):
        body_start = m.end()
        body_end = matches[i + 1].start() if i + 1 < len(matches) else len(text)
        body = text[body_start:body_end]
        docs.append((m.group(1), m.group(2), body))
    return docs


# ─────────────────────────────────────────────────────────────
# Prefab parse
# ─────────────────────────────────────────────────────────────
class GameObjectInfo:
    __slots__ = ("file_id", "name", "components", "transform_id", "parent_go_id",
                 "children_go_ids", "comp_types", "depth", "order")

    def __init__(self, file_id: str, name: str):
        self.file_id = file_id
        self.name = name
        self.components: List[str] = []
        self.transform_id: Optional[str] = None
        self.parent_go_id: Optional[str] = None
        self.children_go_ids: List[str] = []
        self.comp_types: List[str] = []
        self.depth = 0
        self.order = 0  # Document order


class PrefabModel:
    def __init__(self, path: Path):
        self.path = path
        self.text = path.read_bytes().decode("utf-8")
        self.gameobjects: Dict[str, GameObjectInfo] = OrderedDict()
        # transform fileID -> GameObject fileID
        self.transform_to_go: Dict[str, str] = {}
        # component fileID -> GameObject fileID
        self.comp_to_go: Dict[str, str] = {}
        # MonoBehaviour fileID -> script guid
        self.mb_to_guid: Dict[str, str] = {}
        # transform fileID -> parent transform fileID
        self.transform_to_parent: Dict[str, str] = {}
        # component fileID -> class id
        self.comp_class: Dict[str, str] = {}
        self._parse()
        self._build_hierarchy()

    def _parse(self):
        order = 0
        for class_id, file_id, body in split_docs(self.text):
            if class_id == CLASS_GAMEOBJECT:
                m = NAME_LINE_RE.search(body)
                raw = m.group(2).strip() if m else ""
                # Strip YAML quoting if present
                if (raw.startswith("'") and raw.endswith("'")) or \
                   (raw.startswith('"') and raw.endswith('"')):
                    raw = raw[1:-1]
                name = raw
                gi = GameObjectInfo(file_id, name)
                gi.order = order
                order += 1
                # Components
                for cm in M_COMP_RE.finditer(body):
                    cid = cm.group(1)
                    gi.components.append(cid)
                    self.comp_to_go[cid] = file_id
                self.gameobjects[file_id] = gi
            elif class_id in (CLASS_TRANSFORM, CLASS_RECTTRANSFORM):
                go_m = M_GO_RE.search(body)
                if go_m:
                    go_id = go_m.group(1)
                    self.transform_to_go[file_id] = go_id
                fa_m = M_FATHER_RE.search(body)
                if fa_m:
                    self.transform_to_parent[file_id] = fa_m.group(1)
                self.comp_class[file_id] = class_id
            elif class_id == CLASS_MONOBEHAVIOUR:
                gm = M_SCRIPT_GUID_RE.search(body)
                if gm:
                    self.mb_to_guid[file_id] = gm.group(1)
                self.comp_class[file_id] = class_id
            else:
                self.comp_class[file_id] = class_id

    def _build_hierarchy(self):
        # Link transform_id and parent for each GameObject
        for tx_id, go_id in self.transform_to_go.items():
            if go_id in self.gameobjects:
                self.gameobjects[go_id].transform_id = tx_id
        # Resolve parent GO for each child GO
        for tx_id, parent_tx_id in self.transform_to_parent.items():
            child_go = self.transform_to_go.get(tx_id)
            parent_go = self.transform_to_go.get(parent_tx_id)
            if child_go and parent_go and child_go in self.gameobjects:
                self.gameobjects[child_go].parent_go_id = parent_go
                if parent_go in self.gameobjects:
                    self.gameobjects[parent_go].children_go_ids.append(child_go)
        # Component types
        for go_id, gi in self.gameobjects.items():
            for cid in gi.components:
                cls = self.comp_class.get(cid)
                if cls == CLASS_MONOBEHAVIOUR:
                    guid = self.mb_to_guid.get(cid)
                    t = GUID_TO_TYPE.get(guid)
                    if t:
                        gi.comp_types.append(t)
                elif cls == CLASS_CANVAS:
                    gi.comp_types.append("Canvas")
                elif cls == CLASS_ANIMATOR:
                    gi.comp_types.append("Animator")
                elif cls == CLASS_PARTICLE_SYSTEM:
                    gi.comp_types.append("ParticleSystem")
                elif cls in (CLASS_TRANSFORM, CLASS_RECTTRANSFORM):
                    pass  # transform always present, ignore
                elif cls == CLASS_CANVAS_RENDERER:
                    pass
        # Depth
        # Use BFS from root. Root = GameObject(s) with no parent_go_id.
        roots = [go_id for go_id, gi in self.gameobjects.items() if gi.parent_go_id is None]
        for r in roots:
            self._set_depth(r, 0)

    def _set_depth(self, go_id: str, depth: int):
        if go_id not in self.gameobjects:
            return
        gi = self.gameobjects[go_id]
        gi.depth = depth
        for c in gi.children_go_ids:
            self._set_depth(c, depth + 1)


# ─────────────────────────────────────────────────────────────
# Naming
# ─────────────────────────────────────────────────────────────
SANITISE_RE = re.compile(r"[^A-Za-z0-9]+")


def clean_token(s: str, max_len: int = 24) -> str:
    s = SANITISE_RE.sub("_", s).strip("_")
    if not s:
        return "Item"
    # Keep first char alpha
    if not s[0].isalpha():
        s = "X_" + s
    return s[:max_len]


def primary_prefix(comp_types: List[str], has_children: bool) -> str:
    # Priority: Button > Toggle > ScrollRect > Slider > Scrollbar > TMP_Text > Text >
    #          Mask/RectMask2D > Image/RawImage > Animator > ParticleSystem > Panel/Obj
    priority = ["Button", "Toggle", "ScrollRect", "Slider", "Scrollbar",
                "TMP_Text", "Text", "Mask", "RectMask2D",
                "Image", "RawImage", "Animator", "ParticleSystem", "Canvas"]
    for t in priority:
        if t in comp_types:
            return TYPE_TO_PREFIX[t]
    return "Panel" if has_children else "Obj"


def make_new_name(prefab_stem: str, parent_name: str, prefix: str, idx: int,
                  used: set) -> str:
    parent_token = clean_token(parent_name) if parent_name else "Root"
    base = f"{prefix}_{parent_token}_{idx:02d}"
    candidate = base
    bump = idx + 1
    while candidate in used:
        bump += 1
        candidate = f"{prefix}_{parent_token}_{bump:02d}"
    used.add(candidate)
    return candidate


# ─────────────────────────────────────────────────────────────
# Enum extraction (per-prefab protected names)
# ─────────────────────────────────────────────────────────────
ENUM_RE = re.compile(r"\benum\s+(\w+)\s*\{([^}]*)\}", re.DOTALL)


def extract_enum_names(cs_path: Path) -> set:
    if not cs_path.exists():
        return set()
    text = cs_path.read_text(encoding="utf-8", errors="ignore")
    names = set()
    for m in ENUM_RE.finditer(text):
        body = m.group(2)
        for line in body.splitlines():
            line = line.split("//", 1)[0].strip().rstrip(",").strip()
            mm = re.match(r"([A-Za-z_]\w*)", line)
            if mm:
                names.add(mm.group(1))
    return names


def find_script_for_prefab(prefab_stem: str) -> Optional[Path]:
    # Naming convention: UI_Foo.prefab ↔ UI_Foo.cs
    for cs in SCRIPT_ROOT.rglob(f"{prefab_stem}.cs"):
        return cs
    return None


# ─────────────────────────────────────────────────────────────
# Rename plan + apply
# ─────────────────────────────────────────────────────────────
def plan_renames(model: PrefabModel, protected: set) -> Dict[str, str]:
    """Return {go_file_id: new_name} for objects that should be renamed."""
    plan: Dict[str, str] = {}
    used_names = set(gi.name for gi in model.gameobjects.values())

    # Group by name
    by_name: Dict[str, List[GameObjectInfo]] = defaultdict(list)
    for gi in model.gameobjects.values():
        by_name[gi.name].append(gi)

    for name, items in by_name.items():
        if len(items) < 2:
            continue
        # Sort: keep the shallowest, then earliest in document order
        items.sort(key=lambda g: (g.depth, g.order))

        keep_first = name in protected
        # Always rename when name is "careless" — including the first occurrence,
        # because the bind is broken anyway and a meaningful name helps.
        if is_careless(name):
            keep_first = False

        # Reserve the kept name
        if keep_first:
            used_names.add(name)
            to_rename = items[1:]
        else:
            # Free up the original name from used_names so the new generator can re-use
            used_names.discard(name)
            to_rename = items

        for idx, gi in enumerate(to_rename, start=1):
            parent_gi = model.gameobjects.get(gi.parent_go_id) if gi.parent_go_id else None
            parent_name = parent_gi.name if parent_gi else model.path.stem
            has_children = bool(gi.children_go_ids)
            prefix = primary_prefix(gi.comp_types, has_children)
            new_name = make_new_name(model.path.stem, parent_name, prefix, idx, used_names)
            plan[gi.file_id] = new_name

    return plan


def apply_renames(model: PrefabModel, plan: Dict[str, str]) -> str:
    """Rebuild the YAML text with renamed m_Name lines."""
    if not plan:
        return model.text

    text = model.text
    # Walk doc-by-doc, replace m_Name in matching GameObject docs.
    parts = []
    last_end = 0
    matches = list(DOC_SEP_RE.finditer(text))
    for i, m in enumerate(matches):
        body_start = m.end()
        body_end = matches[i + 1].start() if i + 1 < len(matches) else len(text)
        # Append unchanged region before doc header (header is included in body)
        # Actually we need the header itself + body, both unchanged in structure.
        # We'll write everything between last_end and body_end, but with body
        # potentially altered.
        parts.append(text[last_end:body_start])  # header line
        body = text[body_start:body_end]
        if m.group(1) == CLASS_GAMEOBJECT and m.group(2) in plan:
            new_name = plan[m.group(2)]
            body = NAME_LINE_RE.sub(
                lambda nm: f"{nm.group(1)}{new_name}{nm.group(3)}",
                body,
                count=1,
            )
        parts.append(body)
        last_end = body_end
    parts.append(text[last_end:])
    return "".join(parts)


# ─────────────────────────────────────────────────────────────
# Main
# ─────────────────────────────────────────────────────────────
def main(dry_run: bool = False):
    prefabs = sorted(PREFAB_ROOT.rglob("*.prefab"))
    report_lines = [
        "# UI Prefab 중복 이름 리네이밍 보고서",
        "",
        f"- 작업일: 2026-04-20",
        f"- 대상 폴더: `Assets/@Resources/Prefab/UI/`",
        f"- 도구: `_tools/rename_ui_duplicates.py`",
        "",
        "## 변경 규칙",
        "",
        "1. **중복 제거**: 같은 프리펩 안에서 GameObject 이름이 중복되면 두 번째부터 새 이름으로 변경 "
        "(UI 자동 바인드 enum이 첫 번째 오브젝트만 찾아오는 문제 해결).",
        "2. **enum 보호**: 프리펩과 같은 이름의 C# 스크립트가 있고 그 스크립트의 enum 멤버에 해당 이름이 있을 경우, "
        "첫 번째 오브젝트의 이름은 유지하고 나머지만 새 이름으로 변경.",
        "3. **성의 없는 이름 교체**: `GameObject`, `_`, `Image`, `Text`, `Frame`, `Mask`, `Background`, "
        "`Icon`, `Glow`, `Img` 류의 일반명사는 enum 보호 대상이 아닐 경우 첫 번째도 새 이름으로 교체.",
        "4. **언리얼 명명 규칙**: `Img_<부모>_NN`, `Btn_<부모>_NN`, `Txt_<부모>_NN`, `Tgl_<부모>_NN`, "
        "`Scr_<부모>_NN`, `Sld_<부모>_NN`, `Sb_<부모>_NN`, `Mask_<부모>_NN`, `Anim_<부모>_NN`, "
        "`VFX_<부모>_NN`, `Panel_<부모>_NN`, `Obj_<부모>_NN`.",
        "5. 컴포넌트가 여러 개 붙어있으면 우선순위(Button > Toggle > ScrollRect > Slider > Scrollbar > "
        "Text/TMP_Text > Mask > Image > Animator > ParticleSystem > Canvas)에 따라 prefix 결정.",
        "",
        "---",
        "",
    ]

    total_renames = 0
    total_prefabs_changed = 0

    for prefab_path in prefabs:
        try:
            model = PrefabModel(prefab_path)
        except Exception as e:
            print(f"[ERROR] {prefab_path}: {e}")
            continue

        cs_path = find_script_for_prefab(prefab_path.stem)
        protected = extract_enum_names(cs_path) if cs_path else set()

        plan = plan_renames(model, protected)
        if not plan:
            continue

        total_prefabs_changed += 1
        total_renames += len(plan)

        rel = prefab_path.relative_to(PROJECT_ROOT).as_posix()
        report_lines.append(f"## `{rel}`")
        report_lines.append("")
        if cs_path:
            cs_rel = cs_path.relative_to(PROJECT_ROOT).as_posix()
            report_lines.append(f"- 바인드 스크립트: `{cs_rel}` (enum 멤버 {len(protected)}개 보호)")
        else:
            report_lines.append("- 바인드 스크립트: 없음 (enum 보호 대상 없음)")
        report_lines.append(f"- 변경된 오브젝트: {len(plan)}개")
        report_lines.append("")
        report_lines.append("| Old Name | New Name | Parent | Components |")
        report_lines.append("| --- | --- | --- | --- |")
        # Sort entries by old name then by depth/order for stable output
        entries = []
        for go_id, new_name in plan.items():
            gi = model.gameobjects[go_id]
            parent_name = (model.gameobjects[gi.parent_go_id].name
                           if gi.parent_go_id and gi.parent_go_id in model.gameobjects
                           else "(root)")
            comps = ",".join(gi.comp_types) if gi.comp_types else "—"
            entries.append((gi.name, new_name, parent_name, comps, gi.depth, gi.order))
        entries.sort(key=lambda x: (x[0], x[4], x[5]))
        for old, new, parent, comps, _d, _o in entries:
            # escape pipes in markdown
            esc = lambda s: s.replace("|", "\\|") if s else s
            report_lines.append(f"| `{esc(old)}` | `{esc(new)}` | `{esc(parent)}` | {esc(comps)} |")
        report_lines.append("")

        if not dry_run:
            new_text = apply_renames(model, plan)
            prefab_path.write_bytes(new_text.encode("utf-8"))

    report_lines.append("---")
    report_lines.append("")
    report_lines.append("## 요약")
    report_lines.append("")
    report_lines.append(f"- 전체 UI 프리펩: {len(prefabs)}개")
    report_lines.append(f"- 변경된 프리펩: {total_prefabs_changed}개")
    report_lines.append(f"- 변경된 오브젝트: {total_renames}개")
    report_lines.append("")

    DOCS_ROOT.mkdir(parents=True, exist_ok=True)
    REPORT_PATH.write_text("\n".join(report_lines), encoding="utf-8")
    print(f"Report: {REPORT_PATH}")
    print(f"Prefabs changed: {total_prefabs_changed}")
    print(f"Total renames:   {total_renames}")


if __name__ == "__main__":
    import sys
    dry = "--dry-run" in sys.argv
    main(dry_run=dry)
