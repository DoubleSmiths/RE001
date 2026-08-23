# -*- coding: utf-8 -*-
"""RE001_Share의 SampleScene.unity(81MB)에서 원하는 오브젝트 가지만 잘라낸다.

원작은 비주얼 스크립팅 프로젝트라 UI가 프리팹이 아니라 **씬에 통째로 저작**돼 있다.
그래서 "대장간 연출이 어떻게 생겼나"를 보려면 이 한 파일을 뒤져야 하는데,
통째로 열면 어떤 도구도 감당하지 못한다. 이름으로 가지를 찾아 그 아래만 뽑는다.

사용:
  python _tools/share_scene_dump.py --tree "Enhace Popup"          # 계층만
  python _tools/share_scene_dump.py --dump "Enhace Popup" --depth 3 # 컴포넌트까지
  python _tools/share_scene_dump.py --find Flame                    # 이름 검색
  python _tools/share_scene_dump.py --obj 1234567890                # fileID 하나
"""
import argparse
import io
import os
import pickle
import re
import sys

SCENE = r'C:\stopresent\Unity\RE001_Share\Assets\Scenes\SampleScene.unity'
# CACHE는 이 스크립트가 직접 만든 로컬 파싱 캐시다(외부 입력 아님).
CACHE = os.path.join(os.path.dirname(os.path.abspath(__file__)), '.share_scene.pkl')

DOC_RE = re.compile(r'^--- !u!(\d+) &(\d+)')

# 우리가 실제로 읽는 컴포넌트만. 나머지는 클래스 번호만 남긴다.
CLASS_NAME = {
    1: 'GameObject', 4: 'Transform', 224: 'RectTransform', 114: 'MonoBehaviour',
    222: 'CanvasRenderer', 223: 'Canvas', 225: 'CanvasGroup', 95: 'Animator',
    198: 'ParticleSystem', 199: 'ParticleSystemRenderer', 212: 'SpriteRenderer',
    82: 'AudioSource', 33: 'MeshFilter', 23: 'MeshRenderer', 108: 'Light',
}


def parse():
    """씬을 문서 단위로 쪼갠다. {fileID: (classId, [본문 줄])}"""
    docs = {}
    cur_id = None
    cur_cls = None
    buf = []
    with io.open(SCENE, encoding='utf-8', errors='replace') as f:
        for line in f:
            m = DOC_RE.match(line)
            if m:
                if cur_id is not None:
                    docs[cur_id] = (cur_cls, buf)
                cur_cls = int(m.group(1))
                cur_id = int(m.group(2))
                buf = []
            elif cur_id is not None:
                buf.append(line.rstrip('\n'))
        if cur_id is not None:
            docs[cur_id] = (cur_cls, buf)
    return docs


def load():
    if os.path.exists(CACHE) and os.path.getmtime(CACHE) > os.path.getmtime(SCENE):
        with open(CACHE, 'rb') as f:
            return pickle.load(f)
    docs = parse()
    with open(CACHE, 'wb') as f:
        pickle.dump(docs, f, protocol=4)
    return docs


def field(lines, key):
    pat = re.compile(r'^\s*' + re.escape(key) + r':\s*(.*)$')
    for l in lines:
        m = pat.match(l)
        if m:
            return m.group(1).strip()
    return None


def ref(val):
    """'{fileID: 123}' -> 123"""
    if not val:
        return None
    m = re.search(r'fileID:\s*(-?\d+)', val)
    return int(m.group(1)) if m else None


class Scene(object):
    def __init__(self, docs):
        self.docs = docs
        self.go_name = {}        # goId -> name
        self.go_comps = {}       # goId -> [compId]
        self.tr_of_go = {}       # goId -> transformId
        self.go_of_tr = {}       # transformId -> goId
        self.children = {}       # transformId -> [transformId]
        self.parent = {}         # transformId -> transformId
        self._index()

    def _index(self):
        for fid, (cls, lines) in self.docs.items():
            if cls == 1:
                self.go_name[fid] = field(lines, 'm_Name') or ''
                comps = [int(m.group(1)) for m in
                         re.finditer(r'component:\s*\{fileID:\s*(-?\d+)\}', '\n'.join(lines))]
                self.go_comps[fid] = comps
            elif cls in (4, 224):
                go = ref(field(lines, 'm_GameObject'))
                if go is None:
                    continue
                self.tr_of_go[go] = fid
                self.go_of_tr[fid] = go
                self.parent[fid] = ref(field(lines, 'm_Father'))
                body = '\n'.join(lines)
                kids = re.search(r'm_Children:\s*\n((?:\s*-\s*\{fileID:\s*-?\d+\}\n?)*)', body)
                self.children[fid] = ([int(x) for x in re.findall(r'fileID:\s*(-?\d+)', kids.group(1))]
                                      if kids else [])

    def find(self, needle, exact=False):
        n = needle.lower()
        out = []
        for gid, name in self.go_name.items():
            if (name.lower() == n) if exact else (n in name.lower()):
                out.append(gid)
        return out

    def path(self, gid):
        parts = []
        tr = self.tr_of_go.get(gid)
        while tr is not None:
            parts.append(self.go_name.get(self.go_of_tr.get(tr), '?'))
            tr = self.parent.get(tr)
        return '/'.join(reversed(parts))

    def comp_kinds(self, gid):
        out = []
        for cid in self.go_comps.get(gid, []):
            cls = self.docs.get(cid, (0, []))[0]
            out.append(CLASS_NAME.get(cls, 'cls%d' % cls))
        return out

    def tree(self, gid, depth=99, _d=0, out=None):
        if out is None:
            out = []
        tr = self.tr_of_go.get(gid)
        kinds = [k for k in self.comp_kinds(gid) if k not in ('Transform', 'RectTransform', 'CanvasRenderer')]
        active = field(self.docs[gid][1], 'm_IsActive')
        out.append('%s%s%s%s' % ('  ' * _d, self.go_name.get(gid, '?'),
                                 ('  [' + ', '.join(kinds) + ']') if kinds else '',
                                 '' if active == '1' else '  (off)'))
        if _d < depth and tr is not None:
            for c in self.children.get(tr, []):
                cg = self.go_of_tr.get(c)
                if cg is not None:
                    self.tree(cg, depth, _d + 1, out)
        return out

    def dump(self, gid, depth=99, _d=0, out=None):
        if out is None:
            out = []
        tr = self.tr_of_go.get(gid)
        out.append('%s=== %s  (GameObject %d)  active=%s' %
                   ('  ' * _d, self.go_name.get(gid, '?'), gid, field(self.docs[gid][1], 'm_IsActive')))
        for cid in self.go_comps.get(gid, []):
            cls, lines = self.docs.get(cid, (0, []))
            out.append('%s  --- %s &%d' % ('  ' * _d, CLASS_NAME.get(cls, 'cls%d' % cls), cid))
            for l in lines:
                out.append('%s  %s' % ('  ' * _d, l))
        if _d < depth and tr is not None:
            for c in self.children.get(tr, []):
                cg = self.go_of_tr.get(c)
                if cg is not None:
                    self.dump(cg, depth, _d + 1, out)
        return out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--find')
    ap.add_argument('--tree')
    ap.add_argument('--dump')
    ap.add_argument('--obj', type=int)
    ap.add_argument('--depth', type=int, default=99)
    ap.add_argument('--exact', action='store_true')
    ap.add_argument('--out')
    a = ap.parse_args()

    sc = Scene(load())
    lines = []

    if a.find:
        for gid in sc.find(a.find, a.exact):
            lines.append('%d  %s  [%s]' % (gid, sc.path(gid), ', '.join(sc.comp_kinds(gid))))
    elif a.tree:
        hits = sc.find(a.tree, a.exact)
        if not hits:
            lines.append('없음: ' + a.tree)
        for gid in hits:
            lines.append('# ' + sc.path(gid))
            lines += sc.tree(gid, a.depth)
            lines.append('')
    elif a.dump:
        hits = sc.find(a.dump, a.exact)
        for gid in hits:
            lines.append('# ' + sc.path(gid))
            lines += sc.dump(gid, a.depth)
            lines.append('')
    elif a.obj:
        cls, body = sc.docs[a.obj]
        lines.append('--- %s &%d' % (CLASS_NAME.get(cls, 'cls%d' % cls), a.obj))
        lines += body
    else:
        ap.print_help()
        return

    txt = '\n'.join(lines)
    if a.out:
        io.open(a.out, 'w', encoding='utf-8').write(txt)
        print('%s (%d줄)' % (a.out, len(lines)))
    else:
        sys.stdout.buffer.write(txt.encode('utf-8'))
        sys.stdout.buffer.write(b'\n')


if __name__ == '__main__':
    main()
