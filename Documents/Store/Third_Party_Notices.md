# Third-Party Components — Inventory and Notices

Working title: **Minimon Quest** (provisional). Snapshot: 2026-09-29, branch `feat/release-readiness` working tree.
The credits-screen text is in `Third_Party_Notices.txt`. It contains only the entries that need or deserve on-screen attribution, plus the full license texts.
The in-game copy is `Assets/@Resources/Data/Credits.txt`. It has the same content, wrapped at 48 columns for the portrait panel, and adds a title, a `v{version}` line and a "Developed by DoubleSmiths" block. When one file changes, update the other.

## How "Ships?" was decided

A dependency scan followed every `guid:` reference outward from four root sets:

- the build scenes (`TitleScene` and `GameScene`)
- every Addressables group entry
- every non-Editor `Resources` folder
- `ProjectSettings/*.asset`

TMP's editor-only `m_SourceFontFile_EditorRef` was skipped. Runtime C# in non-Editor folders is counted as shipped even when nothing references it, because it compiles into the player.

The scan works on text-serialized assets only. It is not a build report, so confirm the result against the final build's BuildReport or the Addressables build layout.

Status key:

- **OK**: no action beyond keeping the license.
- **ATTRIBUTION NEEDED**: the notice must appear in the credits or documentation. It is already included in the `.txt`.
- **UNKNOWN — VERIFY**: no license evidence in the repo, and the owner must confirm the rights before release.
- **ATTRIBUTION/LICENSE VERIFY**: the source is identified. The owner must confirm the purchase or license and whether it requires a credit.
- **REMOVED**: not shippable, and taken out of the game.
- **BLOCKER**: must not ship.

## 1. Code and libraries

| Component | Source / author | License (evidence) | Attribution | Ships? | Status |
|---|---|---|---|---|---|
| Unity 6000.3.10f1 engine and Unity packages (URP 17.3, Addressables 2.9.1, Input System 1.18, uGUI/TMP 2.0, 2D, Timeline, Visual Scripting, Burst, Collections, Mathematics) | Unity Technologies | Unity Terms of Service / Unity Companion License (each package's `LICENSE.md`) | No | Yes | OK |
| Third-party code embedded in Unity packages: Burst (LLVM, SLEEF, musl, mimalloc…), URP (NVIDIA FXAA 3.11), RP Core (RadeonRays, Bullet, Sobol), 2D (Clipper, Eigen, libigl, LibTessDotNet, cdt2d), Visual Scripting (Full Serializer, YamlDotNet, SQLite…); FreeType in the engine | various | each package's `Third Party Notices.md` under `Library/PackageCache/` | Possibly | Mostly editor or build-time; not verified per component | UNKNOWN — VERIFY. The `.txt` already carries the FreeType line as a conservative default. If in doubt, append those notice files to the credits |
| Json.NET (`com.unity.nuget.newtonsoft-json` 3.2.2) | James Newton-King; SaladLab; Kalle Jillheden; Mike Wuetherick | MIT (`Third Party Notices.md` in the package) | Yes | Yes (save/data loading) | ATTRIBUTION NEEDED |
| DOTween | Daniele Giardini, Demigiant | DOTween license, commercial use allowed and credit not required (`Assets/Plugins/Demigiant/DOTween/readme.txt`, dotween.demigiant.com/license.php) | No | Yes (`DOTween.dll`) | OK |
| DOTween Pro | Demigiant | Asset Store, per seat: "every person working on the Unity project has a valid DOTween Pro license" | No | Yes (`DOTweenPro.dll`, DOTweenAnimation) | OK. Confirm every contributor holds a seat |
| UI Particle (`com.coffee.ui-particle` 4.13.2, git `87dca448ce`) | mob-sakai | MIT (`LICENSE.md` in the package) | Yes | Yes (`UIFx.PlayParticle`) | ATTRIBUTION NEEDED |
| Unity UI Extensions 2.3.2 (embedded `Packages/com.unity.uiextensions`) | SimonDarksideJ and contributors | BSD-3-Clause (`Packages/com.unity.uiextensions/LICENSE.md`). Bundled sub-licenses: LibLZF/CLZF2 BSD-style (`Runtime/Scripts/Utilities/CLZF2.cs`); MIT for `RaycastMask.cs`, `EasingCore.cs` and `RadialLayout.cs`; BSD-style for `ToolTip.cs` | Yes | Yes (17 shaders plus runtime code) | ATTRIBUTION NEEDED |
| Particle Image | AssetKits | Unity Asset Store EULA (no file) | No | Yes (runtime plus demo sprites Fire, Glow, GlowRays, Slide*, Star) | OK |
| Damage Numbers Pro | Ekincan Tas | Asset Store EULA | No | Runtime assembly compiles in; no shipped asset uses it | OK. Remove if unused |
| Sprite Shaders Ultimate | Ekincan Tas | Asset Store EULA | No | Yes (2 shaders, 3 textures) | OK |
| Shine Shader (`Assets/IMPORTS/Sharklib/ShineShader`) | "Sharklib" (store source not recorded) | `license.txt`: MIT © 2016 Unity Technologies (base shaders), plus © 2015 Gregg Tavares hsva-unity, which the author lists as "Inspiration for the Hue-Range masking feature". That is credited, but its license text is not reproduced. Example art under CC BY 3.0 and CC0 does not ship | Yes | Yes (3 shaders, 3 materials, 3 ramps) | ATTRIBUTION NEEDED. Also confirm where the asset came from |
| Sprite Outline shader (`Assets/SpriteOutline`, `Sprites/Outline`) | unknown | none in repo | ? | Yes (`OutlineMaterial`, then `CriticalDamageFont.prefab`) | UNKNOWN — VERIFY |
| Good UI Icon Trail (`Assets/IMPORTS/GoodUIIconTrail`) | unknown | none in repo | ? | Yes (shader and textures via `EquipedMat`, `CriticalOutlineMat`, `ScrollMat`, `SelectedMat`, `EpicMat`) | UNKNOWN — VERIFY |
| TextMesh Pro essential resources (two copies: `Assets/TextMesh Pro`, `Assets/IMPORTS/TextMesh Pro`) | Unity | Unity Companion License | No | Yes (`Resources`) | OK. Liberation Sans and EmojiOne are listed separately below. The duplicate `TMP Settings` in two `Resources` folders is a load-order hazard, so keep one |
| Febucci Text Animator (`Packages/com.febucci.text-animator-unity`) | Febucci | paid asset | — | **No** (no `package.json`, so it never compiles) | OK. Delete the folder |
| unity-cli connector; ExcelDataReader and System.Text.Encoding.CodePages (`Assets/Plugins/Editor`) | — | — | — | No (Editor-only) | OK |
| FastScriptReload (named in AGENTS.md) | — | — | — | No (not in the repo) | OK |

## 2. Fonts

None of these fonts has a license file in the repo except Liberation Sans. The license column gives the terms the author publishes; mark each one verified once you have saved that page.

| Font | Author (from font name table) | Commonly published license | Ships? | Status |
|---|---|---|---|---|
| DOSSaemmul | © 2016 Damheo Lee (leedheo) | MIT. For commercial works the author asks for a credit line ("DOSSaemmul font was used") | Yes (TTF; dynamic TMP asset in the `Fonts` group) | ATTRIBUTION NEEDED · verify |
| PF Stardust 3.0 | © Pinnata (campanula913@naver.com) | Free for commercial use; app embedding allowed; modifying, redistributing or selling the font file without permission is prohibited | Yes (TTF inside the Addressables bundle) | OK · verify that embedding a dynamic font counts as allowed "embedding" |
| Cafe24 Dangdanghae (`Cafe24.ttf`) | © 2018 Cafe24 Corp. | SIL OFL 1.1 (per Cafe24 and noonnu) | Yes (TTF) | ATTRIBUTION NEEDED (OFL text) · verify |
| NEXON Lv.2 Gothic | © 2019 NEXON, designed by FONTRIX | NEXON free font license: commercial use, bundling and embedding allowed if the copyright notice is kept; no modification or sale | Yes (TTF, `UI_DialoguePopup`) | ATTRIBUTION NEEDED · verify |
| Ria Sans (Regular, Bold, ExtraBold) | © 2025 LOTTE GRS (ttseoul) | Free including commercial use and embedding; **no BI/CI use** (don't build the game logo or title from it); no file redistribution | SDF atlases only (`DamageFont`) | OK · verify |
| Liberation Sans | © 2010 Google, © 2012 Red Hat | SIL OFL 1.1 (`Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt`) | Yes (TMP fallback, TTF) | ATTRIBUTION NEEDED |
| Ycomputer (KT corp.), NanumSquare_ac (NAVER, OFL), DNP fonts (Kufic Studio), OptimusPrinceps (Manfred Klein) | — | — | No | OK while unreferenced |
| **Calibri Light** (`Assets/IMPORTS/EpicVictoryEffects/Fonts/calibril.ttf`) | © 2014 Microsoft | Proprietary; its name table only permits use under the EULA of the product it came with | No | Not shipped, but **delete it** so nothing can pick it up |

## 3. Art and visual effects

| Component | Source | License | Ships? | Status |
|---|---|---|---|---|
| AI-generated art: `Monster/Anim` (750), `Monster/Card` (503), `Monster/Generated` (30), equipment and skill icons, 25 region backgrounds and parallax layers, village icons, Forge and Shop backgrounds and NPC | AetherForge API (`_tools/aether_*.py`, ADR 0005). Aether Forge Games Inc., Korea | ToS effective 2026-04-01: "a Member may freely use Generated Outputs for personal or commercial purposes … selling, or providing games". The member is responsible for third-party rights | Yes | OK · VERIFY. Keep a copy of the ToS in force when the art was generated. **Steam AI disclosure required** (Release_Checklist 2-6) |
| Epic Victory Effects | Mixture Art | Asset Store EULA (`Readme.txt`) | Yes (9 textures, 8 materials) | OK |
| Stylized Element Splash vol.2 / Stylized Explosion / Stylized Hit & Slash | VFX_Klaus | Asset Store EULA (ReadMe files) | Yes | OK |
| Sword slash VFX | Hovl Studio (Vladislav Horobets) | Asset Store EULA (`Readme.txt`) | Yes | OK |
| Classic Pixel RPG UI (`Assets/IMPORTS/ClassicPixelRPGUI`) | unknown | none | Yes (65 PNGs plus grayscale shader) | UNKNOWN — VERIFY |
| Dark Fantasy GUI (`gui_atlas.psd`) | unknown | none | Yes (via `UI_BattleBtnGroup_Legecy.prefab`; the spec also uses `gui_atlas` for the [!] marker) | UNKNOWN — VERIFY |
| 25 Pixel Sprite effects | unknown | none | Yes (Ef_23 sheet, HP potion effect) | UNKNOWN — VERIFY |
| Pixel Emotion (`Emoji.png`) | unknown | none | Yes (`UI_TamePopup`) | UNKNOWN — VERIFY |
| Platformer Set 1 (`main_lev_build.png`) | unknown | none | Yes (`Village1_Monastery_BG`) | UNKNOWN — VERIFY |
| EmojiOne sample sprites (TMP) | EmojiOne | `Assets/TextMesh Pro/Sprites/EmojiOne Attribution.txt` (2.x art is commonly CC BY 4.0) | Yes (default TMP sprite asset in `Resources`) | ATTRIBUTION NEEDED. Alternatively, remove the sprite asset if the game never shows emoji |
| Pack-style sprites in `Assets/@Resources/Sprite` with no license: `Background/CH1/PixelForestSummer_FullHD_layer01-05`, `Background/CH2/{bg1_far,bg2_near,Foreground,JungleTileset}`, `UI/Button/Mythic_Button_*`, `UI/Decoration/{General_Border_*,General_Container_*,ActionBar_Background,FlatIcon_37_NoBG}`, `UI/Image/{BeastLord,Knight,LoneWolf}_128x128`, `Icon_CrimsonKnights`, `Icon_WhiteNightVagabond`, `Skill/Water * - SpriteSheet`, `Effect/Fireball`, `Player/*_Sheet`, `NPC/*`, `Monster/CH1/Slime_*`, `HP_Bar/*` | unknown; some probably came with the original VS project (`Documents/RE001_Share-main`) | none | Yes | UNKNOWN — VERIFY. Record the origin (store, author or in-house) for each group |

## 4. Audio (every file ships via the Addressables `Sound` group)

All 65 original audio files were added in one batch in the initial commit `e191a5ba` "[SETTING] 프로젝트 세팅" (2026-03-06). Git history holds no provenance for any of them. The ID3 tags below are the only evidence.

| Files | Evidence (embedded ID3 tags) | Status |
|---|---|---|
| `BGM/BGM_Minion_battle.mp3` (track 24/51), `SFX/UI/Minimon/SFX_MinimonCatch.mp3` (track 27/51). At 2026-09-29 10:40 both were still referenced by `UI_TamePopup.cs:110` and `:497` and the `Sound` group; the swap is in progress | Artist "Junichi Masuda", album "Pokemon Red", "Game Boy". This is ripped Nintendo, Creatures and GAME FREAK audio | **REMOVED**. Not shippable. Before building, confirm three things: both files are gone, their `Sound` group entries are gone, and no build output still contains the old addresses |
| `BGM_MinimonCapture.wav`, `SFX_MinimonCaught.mp3` (the replacements) | Original chiptunes synthesized by `_tools/gen_chiptune.py`. In-house work, no third-party samples | OK (original work). Registered in the `Sound` group and played by `UI_TamePopup` (verified 2026-09-29). The looping BGM is WAV because Unity ignores the MP3 gapless header |
| `SFX/Monster/Slime_Die.mp3`, `Slime_OnHit.mp3` (the hit and death sound for all 503 monsters) | Were "SmartSound FX (MONSTERS) © 2019" | **REPLACED** 2026-09-30 by original synthesized sounds (`_tools/gen_sfx.py`, ADR0048). Credit line removed |
| `SFX/Player/Sound_Miss.mp3`, `Sound_PlayerAttack.mp3`, `PlayerSkill/Sound_Smash.mp3` | Were "Medieval Fantasy Audio Bundle" (Evil Mind Entertainment, 2013) | **REPLACED** 2026-09-30 by original synthesized sounds (`_tools/gen_sfx.py`, ADR0048). Credit line removed |
| `BGM/BGM_1`, `BGM_2`, `BGM_BWF.mp3`, `BGM_Village1.mp3`, `BGM_BreezeForest.mp3`; `ENV/ENV_OnFire`, `ENV_Windbreeze` | no metadata | **REMOVED** 2026-09-29 (branch `game/remake-v2`) together with their `Sound` group entries. Nothing referenced `BGM_1`, `BGM_2` or the two ENV files |
| `BGM_BreezeForest.wav` (title), `BGM_Village1.wav` (village), `BGM_BWF.wav` (field), `BGM_Boss.wav` (stage boss) — the replacements | Original chiptunes written as score data in `_tools/gen_bgm_suite.py` (synth and mastering parts reused from `gen_chiptune.py`). In-house work: no samples, no borrowed melodies | OK (original work). WAV so loops stay gapless; Unity compresses to Vorbis on import. Verified 2026-09-29: each context plays its clip at the exact sample count with loop on |
| The other 51 SFX files | no metadata | **REPLACED** 2026-09-30: every SFX under `Sound/SFX` (56 files, names and GUIDs kept) is now an original chiptune-style sound synthesized by `_tools/gen_sfx.py` (ADR0048); `SFX_MinimonCaught.mp3` was already original. In-house work, no third-party samples |

The SmartSound FX and Evil Mind Entertainment credit lines were removed from `Third_Party_Notices.txt` and the in-game credits when their files were replaced (2026-09-30).

## 5. Notes for the owner

- **Asset Store licenses belong to a seat and a purchaser.** The publishing entity must own every paid asset used here, including anything inherited from the original project. The repo is private (`DoubleSmiths/RE001`), so paid source is not being redistributed. Keep it private.
- **Steam requires an AI disclosure** in the Content Survey, under "Pre-Generated". It covers AetherForge art. It also covers any player-facing text written with AI tools (dialogue, item descriptions, English translation). Coding assistants are exempt.
- **Before the final build**, re-run the dependency check. At minimum, check the Addressables build layout, so that a new asset or a legacy prefab does not quietly pull in something unlicensed.
