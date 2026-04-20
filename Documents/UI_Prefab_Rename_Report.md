# UI Prefab 중복 이름 리네이밍 보고서

- 작업일: 2026-04-20
- 대상 폴더: `Assets/@Resources/Prefab/UI/`
- 도구: `_tools/rename_ui_duplicates.py`

## 변경 규칙

1. **중복 제거**: 같은 프리펩 안에서 GameObject 이름이 중복되면 두 번째부터 새 이름으로 변경 (UI 자동 바인드 enum이 첫 번째 오브젝트만 찾아오는 문제 해결).
2. **enum 보호**: 프리펩과 같은 이름의 C# 스크립트가 있고 그 스크립트의 enum 멤버에 해당 이름이 있을 경우, 첫 번째 오브젝트의 이름은 유지하고 나머지만 새 이름으로 변경.
3. **성의 없는 이름 교체**: `GameObject`, `_`, `Image`, `Text`, `Frame`, `Mask`, `Background`, `Icon`, `Glow`, `Img` 류의 일반명사는 enum 보호 대상이 아닐 경우 첫 번째도 새 이름으로 교체.
4. **언리얼 명명 규칙**: `Img_<부모>_NN`, `Btn_<부모>_NN`, `Txt_<부모>_NN`, `Tgl_<부모>_NN`, `Scr_<부모>_NN`, `Sld_<부모>_NN`, `Sb_<부모>_NN`, `Mask_<부모>_NN`, `Anim_<부모>_NN`, `VFX_<부모>_NN`, `Panel_<부모>_NN`, `Obj_<부모>_NN`.
5. 컴포넌트가 여러 개 붙어있으면 우선순위(Button > Toggle > ScrollRect > Slider > Scrollbar > Text/TMP_Text > Mask > Image > Animator > ParticleSystem > Canvas)에 따라 prefix 결정.

---

## `Assets/@Resources/Prefab/UI/Legecy/UI_CharacterPopup_Legecy.prefab`

- 바인드 스크립트: 없음 (enum 보호 대상 없음)
- 변경된 오브젝트: 133개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `CharacterPopupTitle` | `Panel_StatDetailFrame_1_01` | `StatDetailFrame_1` | — |
| `CharacterPopupTitle` | `Panel_StatDetailFrame_02` | `StatDetailFrame` | — |
| `CharacterPopupTitle` | `Panel_CharacterPopupFrame_03` | `CharacterPopupFrame` | — |
| `DEXDetailGroup` | `Img_Layout_3_04` | `Layout_3` | Image |
| `DEXDetailGroup` | `Img_Layout_05` | `Layout` | Image |
| `DEXDetailInfoGroup` | `Panel_DEXDetailGroup_01` | `DEXDetailGroup` | — |
| `DEXDetailInfoGroup` | `Panel_DEXDetailGroup_1_02` | `DEXDetailGroup_1` | — |
| `DEXDetailInfoGroup` | `Panel_DEXDetailGroup_03` | `DEXDetailGroup` | — |
| `DEXDetailInfoMainTxt` | `Txt_DEXDetailInfoGroup_07` | `DEXDetailInfoGroup` | TMP_Text |
| `DEXDetailInfoMainTxt` | `Txt_DEXDetailInfoGroup_08` | `DEXDetailInfoGroup` | TMP_Text |
| `DEXDetailInfoMainTxt` | `Txt_DEXDetailInfoGroup_09` | `DEXDetailInfoGroup` | TMP_Text |
| `DEXDetailInfoMainTxt_1` | `Txt_DEXDetailInfoGroup_10` | `DEXDetailInfoGroup` | TMP_Text |
| `DEXDetailInfoMainTxt_1` | `Txt_DEXDetailInfoGroup_11` | `DEXDetailInfoGroup` | TMP_Text |
| `DEXDetailInfoMainTxt_1` | `Txt_DEXDetailInfoGroup_12` | `DEXDetailInfoGroup` | TMP_Text |
| `DEXDetailTitleGroup` | `Panel_DEXDetailGroup_1_01` | `DEXDetailGroup_1` | — |
| `DEXDetailTitleGroup` | `Panel_DEXDetailGroup_02` | `DEXDetailGroup` | — |
| `DEXDetailTitleGroup` | `Panel_DEXDetailGroup_05` | `DEXDetailGroup` | — |
| `DEXDetailTitleTxt` | `Txt_DEXDetailTitleGroup_01` | `DEXDetailTitleGroup` | TMP_Text |
| `DEXDetailTitleTxt` | `Txt_DEXDetailTitleGroup_02` | `DEXDetailTitleGroup` | TMP_Text |
| `DEXDetailTitleTxt` | `Txt_DEXDetailTitleGroup_03` | `DEXDetailTitleGroup` | TMP_Text |
| `Frame` | `Img_STRDetailGroup_1_01` | `STRDetailGroup_1` | Image |
| `Frame` | `Img_DEXDetailTitleGroup_02` | `DEXDetailTitleGroup` | Image |
| `Frame` | `Img_STRDetailTitleGroup_03` | `STRDetailTitleGroup` | Image |
| `Frame` | `Img_DEXDetailTitleGroup_04` | `DEXDetailTitleGroup` | Image |
| `Frame` | `Img_VITDetailTitleGroup_05` | `VITDetailTitleGroup` | Image |
| `Frame` | `Img_ASDetailGroup_06` | `ASDetailGroup` | Image |
| `Frame` | `Img_HPDetailGroup_07` | `HPDetailGroup` | Image |
| `Frame` | `Img_DROP_RATEDetailGroup_08` | `DROP_RATEDetailGroup` | Image |
| `Frame` | `Img_DEFDetailGroup_09` | `DEFDetailGroup` | Image |
| `Frame` | `Img_CDMGDetailGroup_10` | `CDMGDetailGroup` | Image |
| `Frame` | `Img_EVADetailGroup_11` | `EVADetailGroup` | Image |
| `Frame` | `Img_VITDetailTitleGroup_12` | `VITDetailTitleGroup` | Image |
| `Frame` | `Img_M_RESDetailGroup_13` | `M_RESDetailGroup` | Image |
| `Frame` | `Img_P_RESDetailGroup_14` | `P_RESDetailGroup` | Image |
| `Frame` | `Img_STRDetailTitleGroup_15` | `STRDetailTitleGroup` | Image |
| `Frame` | `Img_CCDetailGroup_16` | `CCDetailGroup` | Image |
| `Frame` | `Img_RECOVDetailGroup_17` | `RECOVDetailGroup` | Image |
| `Frame` | `Img_ACCDetailGroup_18` | `ACCDetailGroup` | Image |
| `Frame` | `Img_DEXDetailTitleGroup_19` | `DEXDetailTitleGroup` | Image |
| `Frame` | `Img_EXP_RATEDetailGroup_20` | `EXP_RATEDetailGroup` | Image |
| `Frame` | `Img_MATKDetailGroup_21` | `MATKDetailGroup` | Image |
| `Frame` | `Img_PATKDetailGroup_22` | `PATKDetailGroup` | Image |
| `GameObject` | `Txt_Xbutton_01` | `Xbutton` | TMP_Text |
| `GameObject` | `Mask_StatResetBtn_02` | `StatResetBtn` | Image,Mask |
| `GameObject` | `Img_VITInPointBtn_03` | `VITInPointBtn` | Image |
| `GameObject` | `Img_VITDePointBtn_04` | `VITDePointBtn` | Image |
| `GameObject` | `Img_STRInPointBtn_05` | `STRInPointBtn` | Image |
| `GameObject` | `Img_STRDePointBtn_06` | `STRDePointBtn` | Image |
| `GameObject` | `Img_MAGDePointBtn_07` | `MAGDePointBtn` | Image |
| `GameObject` | `Img_DEXDePointBtn_08` | `DEXDePointBtn` | Image |
| `GameObject_1` | `Mask_StatConfirmBtn_01` | `StatConfirmBtn` | Image,Mask |
| `GameObject_1` | `Img_MAGInPointBtn_02` | `MAGInPointBtn` | Image |
| `GameObject_1` | `Img_DEXInPointBtn_03` | `DEXInPointBtn` | Image |
| `Image` | `Img_PointsandElixirBar_01` | `PointsandElixirBar` | Image |
| `Image` | `Img_SubTitle_ATK_02` | `SubTitle_ATK` | Image |
| `Image` | `Img_SubTitle_UTIL_03` | `SubTitle_UTIL` | Image |
| `Image` | `Img_SubTitle_DEF_04` | `SubTitle_DEF` | Image |
| `Image_1` | `Img_SubTitle_ATK_01` | `SubTitle_ATK` | Image |
| `Image_1` | `Img_SubTitle_DEF_02` | `SubTitle_DEF` | Image |
| `Image_1` | `Img_SubTitle_UTIL_05` | `SubTitle_UTIL` | Image |
| `InfoACCTxt` | `Txt_STRDetailInfoGroup_06` | `STRDetailInfoGroup` | TMP_Text |
| `InfoACCTxt` | `Txt_STRDetailInfoGroup_07` | `STRDetailInfoGroup` | TMP_Text |
| `InfoACCTxt` | `Txt_STRDetailInfoGroup_08` | `STRDetailInfoGroup` | TMP_Text |
| `InfoASTxt` | `Txt_DEXDetailInfoGroup_13` | `DEXDetailInfoGroup` | TMP_Text |
| `InfoASTxt` | `Txt_DEXDetailInfoGroup_14` | `DEXDetailInfoGroup` | TMP_Text |
| `InfoASTxt` | `Txt_DEXDetailInfoGroup_15` | `DEXDetailInfoGroup` | TMP_Text |
| `InfoCRITTxt` | `Txt_STRDetailInfoGroup_01` | `STRDetailInfoGroup` | TMP_Text |
| `InfoCRITTxt` | `Txt_STRDetailInfoGroup_04` | `STRDetailInfoGroup` | TMP_Text |
| `InfoCRITTxt` | `Txt_STRDetailInfoGroup_05` | `STRDetailInfoGroup` | TMP_Text |
| `InfoCTRTxt` | `Txt_DEXDetailInfoGroup_04` | `DEXDetailInfoGroup` | TMP_Text |
| `InfoCTRTxt` | `Txt_DEXDetailInfoGroup_05` | `DEXDetailInfoGroup` | TMP_Text |
| `InfoCTRTxt` | `Txt_DEXDetailInfoGroup_06` | `DEXDetailInfoGroup` | TMP_Text |
| `InfoDEFTxt` | `Txt_VITDetailInfoGroup_07` | `VITDetailInfoGroup` | TMP_Text |
| `InfoDEFTxt` | `Txt_VITDetailInfoGroup_08` | `VITDetailInfoGroup` | TMP_Text |
| `InfoDMGTxt` | `Txt_STRDetailGroup_1_03` | `STRDetailGroup_1` | TMP_Text |
| `InfoDMGTxt` | `Txt_STRDetailInfoGroup_12` | `STRDetailInfoGroup` | TMP_Text |
| `InfoDMGTxt` | `Txt_STRDetailInfoGroup_13` | `STRDetailInfoGroup` | TMP_Text |
| `InfoEVATxt` | `Txt_DEXDetailInfoGroup_01` | `DEXDetailInfoGroup` | TMP_Text |
| `InfoEVATxt` | `Txt_DEXDetailInfoGroup_02` | `DEXDetailInfoGroup` | TMP_Text |
| `InfoEVATxt` | `Txt_DEXDetailInfoGroup_03` | `DEXDetailInfoGroup` | TMP_Text |
| `InfoHPTxt` | `Txt_VITDetailInfoGroup_05` | `VITDetailInfoGroup` | TMP_Text |
| `InfoHPTxt` | `Txt_VITDetailInfoGroup_06` | `VITDetailInfoGroup` | TMP_Text |
| `InfoRecovTxt` | `Txt_VITDetailInfoGroup_03` | `VITDetailInfoGroup` | TMP_Text |
| `InfoRecovTxt` | `Txt_VITDetailInfoGroup_04` | `VITDetailInfoGroup` | TMP_Text |
| `Layout` | `Panel_CharacterPopup_01` | `CharacterPopup` | — |
| `Layout` | `Panel_CharacterPopupTitle_02` | `CharacterPopupTitle` | — |
| `Layout` | `Panel_CharacterPopupTitle_03` | `CharacterPopupTitle` | — |
| `STRDetailGroup` | `Img_Layout_3_01` | `Layout_3` | Image |
| `STRDetailGroup` | `Img_Layout_02` | `Layout` | Image |
| `STRDetailInfoGroup` | `Panel_STRDetailGroup_1_01` | `STRDetailGroup_1` | — |
| `STRDetailInfoGroup` | `Panel_STRDetailGroup_04` | `STRDetailGroup` | — |
| `STRDetailInfoGroup` | `Panel_STRDetailGroup_03` | `STRDetailGroup` | — |
| `STRDetailInfoMainTxt` | `Txt_STRDetailGroup_1_01` | `STRDetailGroup_1` | TMP_Text |
| `STRDetailInfoMainTxt` | `Txt_STRDetailInfoGroup_02` | `STRDetailInfoGroup` | TMP_Text |
| `STRDetailInfoMainTxt` | `Txt_STRDetailInfoGroup_03` | `STRDetailInfoGroup` | TMP_Text |
| `STRDetailInfoMainTxt_1` | `Txt_STRDetailInfoGroup_09` | `STRDetailInfoGroup` | TMP_Text |
| `STRDetailInfoMainTxt_1` | `Txt_STRDetailInfoGroup_10` | `STRDetailInfoGroup` | TMP_Text |
| `STRDetailInfoMainTxt_1` | `Txt_STRDetailInfoGroup_11` | `STRDetailInfoGroup` | TMP_Text |
| `STRDetailTitleGroup` | `Panel_STRDetailGroup_01` | `STRDetailGroup` | — |
| `STRDetailTitleGroup` | `Panel_STRDetailGroup_02` | `STRDetailGroup` | — |
| `STRDetailTitleTxt` | `Txt_STRDetailTitleGroup_01` | `STRDetailTitleGroup` | TMP_Text |
| `STRDetailTitleTxt` | `Txt_STRDetailTitleGroup_02` | `STRDetailTitleGroup` | TMP_Text |
| `StatNameTxt` | `Txt_VITStatGroup_01` | `VITStatGroup` | TMP_Text |
| `StatNameTxt` | `Txt_STRStatGroup_02` | `STRStatGroup` | TMP_Text |
| `StatNameTxt` | `Txt_DEXStatGroup_03` | `DEXStatGroup` | TMP_Text |
| `StatNameTxt` | `Txt_MAGStatGroup_04` | `MAGStatGroup` | TMP_Text |
| `Text` | `Txt_PointsandElixirBar_01` | `PointsandElixirBar` | TMP_Text |
| `Text` | `Txt_PointsandElixirBar_02` | `PointsandElixirBar` | TMP_Text |
| `Text` | `Txt_StatDetailBtn_03` | `StatDetailBtn` | TMP_Text |
| `VITDetailGroup` | `Img_Layout_3_03` | `Layout_3` | Image |
| `VITDetailGroup` | `Img_Layout_04` | `Layout` | Image |
| `VITDetailInfoGroup` | `Panel_VITDetailGroup_03` | `VITDetailGroup` | — |
| `VITDetailInfoGroup` | `Panel_VITDetailGroup_04` | `VITDetailGroup` | — |
| `VITDetailInfoMainTxt` | `Txt_VITDetailInfoGroup_09` | `VITDetailInfoGroup` | TMP_Text |
| `VITDetailInfoMainTxt` | `Txt_VITDetailInfoGroup_10` | `VITDetailInfoGroup` | TMP_Text |
| `VITDetailInfoMainTxt_1` | `Txt_VITDetailInfoGroup_01` | `VITDetailInfoGroup` | TMP_Text |
| `VITDetailInfoMainTxt_1` | `Txt_VITDetailInfoGroup_02` | `VITDetailInfoGroup` | TMP_Text |
| `VITDetailTitleGroup` | `Panel_VITDetailGroup_01` | `VITDetailGroup` | — |
| `VITDetailTitleGroup` | `Panel_VITDetailGroup_02` | `VITDetailGroup` | — |
| `VITDetailTitleTxt` | `Txt_VITDetailTitleGroup_01` | `VITDetailTitleGroup` | TMP_Text |
| `VITDetailTitleTxt` | `Txt_VITDetailTitleGroup_02` | `VITDetailTitleGroup` | TMP_Text |
| `_` | `Img_CharacterPopupTitle_01` | `CharacterPopupTitle` | Image |
| `_` | `Img_CharacterPopupTitle_02` | `CharacterPopupTitle` | Image |
| `_` | `Img_CharacterPopupTitle_03` | `CharacterPopupTitle` | Image |
| `_` | `Img_CharacterPopupTitle_04` | `CharacterPopupTitle` | Image |
| `_` | `Img_DEXDetailTitleGroup_05` | `DEXDetailTitleGroup` | Image |
| `_` | `Img_VITDetailTitleGroup_06` | `VITDetailTitleGroup` | Image |
| `_` | `Img_DEXDetailTitleGroup_07` | `DEXDetailTitleGroup` | Image |
| `_` | `Img_STRDetailTitleGroup_08` | `STRDetailTitleGroup` | Image |
| `_` | `Obj_Right_09` | `Right` | — |
| `_` | `Img_VITDetailTitleGroup_10` | `VITDetailTitleGroup` | Image |
| `_` | `Img_DEXDetailTitleGroup_11` | `DEXDetailTitleGroup` | Image |
| `_` | `Img_STRDetailTitleGroup_12` | `STRDetailTitleGroup` | Image |

## `Assets/@Resources/Prefab/UI/Popup/UI_BestiaryPopup.prefab`

- 바인드 스크립트: 없음 (enum 보호 대상 없음)
- 변경된 오브젝트: 199개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `ActiveEf_1` | `Panel_BeastiayBox_10_01` | `BeastiayBox_10` | — |
| `ActiveEf_1` | `Panel_BeastiayBox_8_02` | `BeastiayBox_8` | — |
| `ActiveEf_1` | `Panel_BeastiayBox_14_03` | `BeastiayBox_14` | — |
| `ActiveEf_1` | `Panel_BeastiayBox_9_04` | `BeastiayBox_9` | — |
| `ActiveEf_1` | `Panel_BestiayBox_3_05` | `BestiayBox_3` | — |
| `ActiveEf_1` | `Panel_BeastiayBox_11_06` | `BeastiayBox_11` | — |
| `ActiveEf_1` | `Panel_BestiayBox_5_07` | `BestiayBox_5` | — |
| `ActiveEf_1` | `Panel_BeastiayBox_7_08` | `BeastiayBox_7` | — |
| `ActiveEf_1` | `Panel_BestiayBox_1_09` | `BestiayBox_1` | — |
| `ActiveEf_1` | `Panel_BestiayBox_6_10` | `BestiayBox_6` | — |
| `ActiveEf_1` | `Panel_BestiayBox_2_11` | `BestiayBox_2` | — |
| `ActiveEf_1` | `Panel_BeastiayBox_12_12` | `BeastiayBox_12` | — |
| `ActiveEf_1` | `Panel_BeastiayBox_13_13` | `BeastiayBox_13` | — |
| `ActiveEf_1` | `Panel_BestiayBox_4_14` | `BestiayBox_4` | — |
| `AllSStar` | `Img_BeastiayBox_12_01` | `BeastiayBox_12` | Image |
| `AllSStar` | `Img_BestiayBox_6_02` | `BestiayBox_6` | Image |
| `AllSStar` | `Img_BestiayBox_2_03` | `BestiayBox_2` | Image |
| `AllSStar` | `Img_BestiayBox_3_04` | `BestiayBox_3` | Image |
| `AllSStar` | `Img_BestiayBox_5_05` | `BestiayBox_5` | Image |
| `AllSStar` | `Img_BeastiayBox_9_06` | `BeastiayBox_9` | Image |
| `AllSStar` | `Img_BeastiayBox_13_07` | `BeastiayBox_13` | Image |
| `AllSStar` | `Img_BeastiayBox_10_08` | `BeastiayBox_10` | Image |
| `AllSStar` | `Img_BeastiayBox_8_09` | `BeastiayBox_8` | Image |
| `AllSStar` | `Img_BestiayBox_4_10` | `BestiayBox_4` | Image |
| `AllSStar` | `Img_BeastiayBox_14_11` | `BeastiayBox_14` | Image |
| `AllSStar` | `Img_BeastiayBox_11_12` | `BeastiayBox_11` | Image |
| `AllSStar` | `Img_BeastiayBox_7_13` | `BeastiayBox_7` | Image |
| `AllSStar` | `Img_BestiayBox_1_14` | `BestiayBox_1` | Image |
| `Background` | `Img_Bestiary_RetrievionSkill_01` | `Bestiary_RetrievionSkill` | Image |
| `Background` | `Img_Bestiary_HybridKinshipSk_04` | `Bestiary_HybridKinshipSkill` | Image |
| `Background` | `Img_Bestiary_MasterHandlerSk_03` | `Bestiary_MasterHandlerSkill` | Image |
| `Background` | `Img_Bestiary_MemoryExpansion_04` | `Bestiary_MemoryExpansionSkill` | Image |
| `BeastiaryBoxMonsterExist` | `Mask_BeastiayBox_9_01` | `BeastiayBox_9` | Image,Mask |
| `BeastiaryBoxMonsterExist` | `Mask_BestiayBox_4_02` | `BestiayBox_4` | Image,Mask |
| `BeastiaryBoxMonsterExist` | `Mask_BestiayBox_5_03` | `BestiayBox_5` | Image,Mask |
| `BeastiaryBoxMonsterExist` | `Mask_BestiayBox_2_04` | `BestiayBox_2` | Image,Mask |
| `BeastiaryBoxMonsterExist` | `Mask_BeastiayBox_13_05` | `BeastiayBox_13` | Image,Mask |
| `BeastiaryBoxMonsterExist` | `Mask_BestiayBox_6_06` | `BestiayBox_6` | Image,Mask |
| `BeastiaryBoxMonsterExist` | `Mask_BeastiayBox_8_07` | `BeastiayBox_8` | Image,Mask |
| `BeastiaryBoxMonsterExist` | `Mask_BeastiayBox_14_08` | `BeastiayBox_14` | Image,Mask |
| `BeastiaryBoxMonsterExist` | `Mask_BeastiayBox_11_09` | `BeastiayBox_11` | Image,Mask |
| `BeastiaryBoxMonsterExist` | `Mask_BestiayBox_1_10` | `BestiayBox_1` | Image,Mask |
| `BeastiaryBoxMonsterExist` | `Mask_BeastiayBox_7_11` | `BeastiayBox_7` | Image,Mask |
| `BeastiaryBoxMonsterExist` | `Mask_BeastiayBox_10_12` | `BeastiayBox_10` | Image,Mask |
| `BeastiaryBoxMonsterExist` | `Mask_BeastiayBox_12_13` | `BeastiayBox_12` | Image,Mask |
| `BeastiaryBoxMonsterExist` | `Mask_BestiayBox_3_14` | `BestiayBox_3` | Image,Mask |
| `BeastiaryBoxMonsterMissing` | `Mask_BestiayBox_5_01` | `BestiayBox_5` | Image,Mask |
| `BeastiaryBoxMonsterMissing` | `Mask_BestiayBox_2_02` | `BestiayBox_2` | Image,Mask |
| `BeastiaryBoxMonsterMissing` | `Mask_BeastiayBox_13_03` | `BeastiayBox_13` | Image,Mask |
| `BeastiaryBoxMonsterMissing` | `Mask_BeastiayBox_14_04` | `BeastiayBox_14` | Image,Mask |
| `BeastiaryBoxMonsterMissing` | `Mask_BeastiayBox_11_05` | `BeastiayBox_11` | Image,Mask |
| `BeastiaryBoxMonsterMissing` | `Mask_BeastiayBox_12_06` | `BeastiayBox_12` | Image,Mask |
| `BeastiaryBoxMonsterMissing` | `Mask_BeastiayBox_7_07` | `BeastiayBox_7` | Image,Mask |
| `BeastiaryBoxMonsterMissing` | `Mask_BeastiayBox_10_08` | `BeastiayBox_10` | Image,Mask |
| `BeastiaryBoxMonsterMissing` | `Mask_BeastiayBox_9_09` | `BeastiayBox_9` | Image,Mask |
| `BeastiaryBoxMonsterMissing` | `Mask_BestiayBox_3_10` | `BestiayBox_3` | Image,Mask |
| `BeastiaryBoxMonsterMissing` | `Mask_BestiayBox_1_11` | `BestiayBox_1` | Image,Mask |
| `BeastiaryBoxMonsterMissing` | `Mask_BestiayBox_6_12` | `BestiayBox_6` | Image,Mask |
| `BeastiaryBoxMonsterMissing` | `Mask_BestiayBox_4_13` | `BestiayBox_4` | Image,Mask |
| `BeastiaryBoxMonsterMissing` | `Mask_BeastiayBox_8_14` | `BeastiayBox_8` | Image,Mask |
| `BeastiaryNumTxt` | `Txt_Image_01` | `Image` | TMP_Text |
| `BeastiaryNumTxt` | `Txt_Image_02` | `Image` | TMP_Text |
| `BeastiaryNumTxt` | `Txt_Image_03` | `Image` | TMP_Text |
| `BeastiaryNumTxt` | `Txt_Image_04` | `Image` | TMP_Text |
| `BeastiaryNumTxt` | `Txt_Image_05` | `Image` | TMP_Text |
| `BeastiaryNumTxt` | `Txt_Image_06` | `Image` | TMP_Text |
| `BeastiaryNumTxt` | `Txt_Image_07` | `Image` | TMP_Text |
| `BeastiaryNumTxt` | `Txt_Image_08` | `Image` | TMP_Text |
| `BeastiaryNumTxt` | `Txt_Image_09` | `Image` | TMP_Text |
| `BeastiaryNumTxt` | `Txt_Image_10` | `Image` | TMP_Text |
| `BeastiaryNumTxt` | `Txt_Image_11` | `Image` | TMP_Text |
| `BeastiaryNumTxt` | `Txt_Image_12` | `Image` | TMP_Text |
| `BeastiaryNumTxt` | `Txt_Image_13` | `Image` | TMP_Text |
| `BeastiaryNumTxt` | `Txt_Image_14` | `Image` | TMP_Text |
| `EFrame` | `Img_ActiveEf_1_01` | `ActiveEf_1` | Image |
| `EFrame` | `Img_ActiveEf_1_02` | `ActiveEf_1` | Image |
| `EFrame` | `Img_ActiveEf_1_03` | `ActiveEf_1` | Image |
| `EFrame` | `Img_ActiveEf_1_04` | `ActiveEf_1` | Image |
| `EFrame` | `Img_ActiveEf_1_05` | `ActiveEf_1` | Image |
| `EFrame` | `Img_ActiveEf_1_06` | `ActiveEf_1` | Image |
| `EFrame` | `Img_ActiveEf_1_07` | `ActiveEf_1` | Image |
| `EFrame` | `Img_ActiveEf_1_08` | `ActiveEf_1` | Image |
| `EFrame` | `Img_ActiveEf_1_09` | `ActiveEf_1` | Image |
| `EFrame` | `Img_ActiveEf_1_10` | `ActiveEf_1` | Image |
| `EFrame` | `Img_ActiveEf_1_11` | `ActiveEf_1` | Image |
| `EFrame` | `Img_ActiveEf_1_12` | `ActiveEf_1` | Image |
| `EFrame` | `Img_ActiveEf_1_13` | `ActiveEf_1` | Image |
| `EFrame` | `Img_ActiveEf_1_14` | `ActiveEf_1` | Image |
| `Frame` | `Img_Bestiary_MemoryExpansion_01` | `Bestiary_MemoryExpansionSkill` | Image |
| `Frame` | `Img_Bestiary_HybridKinshipSk_02` | `Bestiary_HybridKinshipSkill` | Image |
| `Frame` | `Img_Bestiary_RetrievionSkill_03` | `Bestiary_RetrievionSkill` | Image |
| `Frame` | `Img_Bestiary_MasterHandlerSk_04` | `Bestiary_MasterHandlerSkill` | Image |
| `Frame` | `Img_Bestiary_Beezing_05` | `Bestiary_Beezing` | Image |
| `Frame` | `Img_Beastiary_NOTHING_06` | `Beastiary_NOTHING` | Image |
| `Frame` | `Img_BeastiayBox_11_07` | `BeastiayBox_11` | Image |
| `Frame` | `Img_BeastiayBox_9_08` | `BeastiayBox_9` | Image |
| `Frame` | `Img_BestiayBox_4_09` | `BestiayBox_4` | Image |
| `Frame` | `Img_BestiayBox_3_10` | `BestiayBox_3` | Image |
| `Frame` | `Img_BestiayBox_2_11` | `BestiayBox_2` | Image |
| `Frame` | `Img_BeastiayBox_10_12` | `BeastiayBox_10` | Image |
| `Frame` | `Img_BeastiayBox_12_13` | `BeastiayBox_12` | Image |
| `Frame` | `Img_BeastiayBox_13_14` | `BeastiayBox_13` | Image |
| `Frame` | `Img_BestiayBox_1_15` | `BestiayBox_1` | Image |
| `Frame` | `Img_BeastiayBox_8_16` | `BeastiayBox_8` | Image |
| `Frame` | `Img_BeastiayBox_14_17` | `BeastiayBox_14` | Image |
| `Frame` | `Img_BestiayBox_6_18` | `BestiayBox_6` | Image |
| `Frame` | `Img_BestiayBox_5_19` | `BestiayBox_5` | Image |
| `Frame` | `Img_BeastiayBox_7_20` | `BeastiayBox_7` | Image |
| `GameObject` | `Txt_Xbutton_01` | `Xbutton` | TMP_Text |
| `GameObject` | `Txt_BeastiaryBoxMonsterMissi_02` | `BeastiaryBoxMonsterMissing` | TMP_Text |
| `GameObject` | `Txt_BeastiaryBoxMonsterMissi_03` | `BeastiaryBoxMonsterMissing` | TMP_Text |
| `GameObject` | `Txt_BeastiaryBoxMonsterMissi_04` | `BeastiaryBoxMonsterMissing` | TMP_Text |
| `GameObject` | `Txt_BeastiaryBoxMonsterMissi_05` | `BeastiaryBoxMonsterMissing` | TMP_Text |
| `GameObject` | `Txt_BeastiaryBoxMonsterMissi_06` | `BeastiaryBoxMonsterMissing` | TMP_Text |
| `GameObject` | `Txt_BeastiaryBoxMonsterMissi_07` | `BeastiaryBoxMonsterMissing` | TMP_Text |
| `GameObject` | `Txt_BeastiaryBoxMonsterMissi_08` | `BeastiaryBoxMonsterMissing` | TMP_Text |
| `GameObject` | `Txt_BeastiaryBoxMonsterMissi_09` | `BeastiaryBoxMonsterMissing` | TMP_Text |
| `GameObject` | `Txt_BeastiaryBoxMonsterMissi_10` | `BeastiaryBoxMonsterMissing` | TMP_Text |
| `GameObject` | `Txt_BeastiaryBoxMonsterMissi_11` | `BeastiaryBoxMonsterMissing` | TMP_Text |
| `GameObject` | `Txt_BeastiaryBoxMonsterMissi_12` | `BeastiaryBoxMonsterMissing` | TMP_Text |
| `GameObject` | `Txt_BeastiaryBoxMonsterMissi_13` | `BeastiaryBoxMonsterMissing` | TMP_Text |
| `GameObject` | `Txt_BeastiaryBoxMonsterMissi_14` | `BeastiaryBoxMonsterMissing` | TMP_Text |
| `GameObject` | `Txt_BeastiaryBoxMonsterMissi_15` | `BeastiaryBoxMonsterMissing` | TMP_Text |
| `Icon_2` | `Img_LvlIcon_4_01` | `LvlIcon_4` | Image |
| `Icon_2` | `Img_LvlIcon_02` | `LvlIcon` | Image |
| `Icon_2` | `Img_LvlIcon_3_03` | `LvlIcon_3` | Image |
| `Icon_2` | `Img_LvlIcon_2_04` | `LvlIcon_2` | Image |
| `Image` | `Img_BeastiaryAreaTextGroup_01` | `BeastiaryAreaTextGroup` | Image |
| `Image` | `Img_BestiaryAreaTextGroup_02` | `BestiaryAreaTextGroup` | Image |
| `Image` | `Img_BeastiayBox_11_03` | `BeastiayBox_11` | Image |
| `Image` | `Img_BestiayBox_4_04` | `BestiayBox_4` | Image |
| `Image` | `Img_BestiayBox_3_05` | `BestiayBox_3` | Image |
| `Image` | `Img_BestiayBox_2_06` | `BestiayBox_2` | Image |
| `Image` | `Img_BestiayBox_6_07` | `BestiayBox_6` | Image |
| `Image` | `Img_BestiayBox_1_08` | `BestiayBox_1` | Image |
| `Image` | `Img_BeastiayBox_7_09` | `BeastiayBox_7` | Image |
| `Image` | `Img_BeastiayBox_8_10` | `BeastiayBox_8` | Image |
| `Image` | `Img_BeastiayBox_9_11` | `BeastiayBox_9` | Image |
| `Image` | `Img_BestiayBox_5_12` | `BestiayBox_5` | Image |
| `Image` | `Img_BeastiayBox_14_13` | `BeastiayBox_14` | Image |
| `Image` | `Img_BeastiayBox_12_14` | `BeastiayBox_12` | Image |
| `Image` | `Img_BeastiayBox_13_15` | `BeastiayBox_13` | Image |
| `Image` | `Img_BeastiayBox_10_16` | `BeastiayBox_10` | Image |
| `Img` | `Img_BeastiaryBoxMonsterExist_05` | `BeastiaryBoxMonsterExist` | Image |
| `Img` | `Img_BeastiaryBoxMonsterExist_06` | `BeastiaryBoxMonsterExist` | Image |
| `Img` | `Img_BeastiaryBoxMonsterExist_08` | `BeastiaryBoxMonsterExist` | Image |
| `Img` | `Img_BeastiaryBoxMonsterExist_09` | `BeastiaryBoxMonsterExist` | Image |
| `Img` | `Img_BeastiaryBoxMonsterExist_11` | `BeastiaryBoxMonsterExist` | Image |
| `Img` | `Img_BeastiaryBoxMonsterExist_13` | `BeastiaryBoxMonsterExist` | Image |
| `Img` | `Img_BeastiaryBoxMonsterExist_15` | `BeastiaryBoxMonsterExist` | Image |
| `Img` | `Img_BeastiaryBoxMonsterExist_17` | `BeastiaryBoxMonsterExist` | Image |
| `Img` | `Img_BeastiaryBoxMonsterExist_18` | `BeastiaryBoxMonsterExist` | Image |
| `Img` | `Img_BeastiaryBoxMonsterExist_22` | `BeastiaryBoxMonsterExist` | Image |
| `Img` | `Img_BeastiaryBoxMonsterExist_24` | `BeastiaryBoxMonsterExist` | Image |
| `Img` | `Img_BeastiaryBoxMonsterExist_26` | `BeastiaryBoxMonsterExist` | Image |
| `Img` | `Img_BeastiaryBoxMonsterExist_27` | `BeastiaryBoxMonsterExist` | Image |
| `Img` | `Img_BeastiaryBoxMonsterExist_28` | `BeastiaryBoxMonsterExist` | Image |
| `Text` | `Txt_LvlIcon_4_01` | `LvlIcon_4` | TMP_Text |
| `Text` | `Txt_LvlIcon_3_02` | `LvlIcon_3` | TMP_Text |
| `Text` | `Txt_LvlIcon_03` | `LvlIcon` | TMP_Text |
| `Text` | `Txt_LvlIcon_2_04` | `LvlIcon_2` | TMP_Text |
| `WIcon` | `Btn_Bestiary_HybridKinshipSk_01` | `Bestiary_HybridKinshipSkill` | Image,Button |
| `WIcon` | `Btn_Bestiary_MemoryExpansion_02` | `Bestiary_MemoryExpansionSkill` | Image,Button |
| `WIcon` | `Btn_Bestiary_MasterHandlerSk_03` | `Bestiary_MasterHandlerSkill` | Image,Button |
| `WIcon` | `Btn_Bestiary_RetrievionSkill_04` | `Bestiary_RetrievionSkill` | Image,Button |
| `_` | `Img_BeastiaryBoxMonsterExist_01` | `BeastiaryBoxMonsterExist` | Image |
| `_` | `Img_BeastiaryBoxMonsterExist_02` | `BeastiaryBoxMonsterExist` | Image |
| `_` | `Img_BeastiaryBoxMonsterExist_03` | `BeastiaryBoxMonsterExist` | Image |
| `_` | `Img_BeastiaryBoxMonsterExist_04` | `BeastiaryBoxMonsterExist` | Image |
| `_` | `Img_BeastiaryBoxMonsterMissi_05` | `BeastiaryBoxMonsterMissing` | Image |
| `_` | `Img_BeastiaryBoxMonsterMissi_06` | `BeastiaryBoxMonsterMissing` | Image |
| `_` | `Img_BeastiaryBoxMonsterExist_07` | `BeastiaryBoxMonsterExist` | Image |
| `_` | `Img_BeastiaryBoxMonsterMissi_08` | `BeastiaryBoxMonsterMissing` | Image |
| `_` | `Img_BeastiaryBoxMonsterMissi_09` | `BeastiaryBoxMonsterMissing` | Image |
| `_` | `Img_BeastiaryBoxMonsterExist_10` | `BeastiaryBoxMonsterExist` | Image |
| `_` | `Img_BeastiaryBoxMonsterMissi_11` | `BeastiaryBoxMonsterMissing` | Image |
| `_` | `Img_BeastiaryBoxMonsterExist_12` | `BeastiaryBoxMonsterExist` | Image |
| `_` | `Img_BeastiaryBoxMonsterMissi_13` | `BeastiaryBoxMonsterMissing` | Image |
| `_` | `Img_BeastiaryBoxMonsterExist_14` | `BeastiaryBoxMonsterExist` | Image |
| `_` | `Img_BeastiaryBoxMonsterMissi_15` | `BeastiaryBoxMonsterMissing` | Image |
| `_` | `Img_BeastiaryBoxMonsterExist_16` | `BeastiaryBoxMonsterExist` | Image |
| `_` | `Img_BeastiaryBoxMonsterMissi_17` | `BeastiaryBoxMonsterMissing` | Image |
| `_` | `Img_BeastiaryBoxMonsterMissi_18` | `BeastiaryBoxMonsterMissing` | Image |
| `_` | `Img_BeastiaryBoxMonsterExist_19` | `BeastiaryBoxMonsterExist` | Image |
| `_` | `Img_BeastiaryBoxMonsterExist_20` | `BeastiaryBoxMonsterExist` | Image |
| `_` | `Img_BeastiaryBoxMonsterExist_21` | `BeastiaryBoxMonsterExist` | Image |
| `_` | `Img_BeastiaryBoxMonsterMissi_22` | `BeastiaryBoxMonsterMissing` | Image |
| `_` | `Img_BeastiaryBoxMonsterExist_23` | `BeastiaryBoxMonsterExist` | Image |
| `_` | `Img_BeastiaryBoxMonsterMissi_24` | `BeastiaryBoxMonsterMissing` | Image |
| `_` | `Img_BeastiaryBoxMonsterExist_25` | `BeastiaryBoxMonsterExist` | Image |
| `_` | `Img_BeastiaryBoxMonsterMissi_26` | `BeastiaryBoxMonsterMissing` | Image |
| `_` | `Img_BeastiaryBoxMonsterMissi_27` | `BeastiaryBoxMonsterMissing` | Image |
| `_` | `Img_BeastiaryBoxMonsterMissi_28` | `BeastiaryBoxMonsterMissing` | Image |
| `__1` | `Txt_BestiaryAreaTextGroup_01` | `BestiaryAreaTextGroup` | TMP_Text |
| `__1` | `Txt_BeastiaryAreaTextGroup_02` | `BeastiaryAreaTextGroup` | TMP_Text |
| `lockimg` | `Img_Bestiary_SkillLock_Hybri_01` | `Bestiary_SkillLock_HybridKinship` | Image |
| `lockimg` | `Img_Bestiary_SkillLock_Maste_02` | `Bestiary_SkillLock_MasterHandler` | Image |
| `lockimg` | `Img_Bestiary_SkillLock_Retri_03` | `Bestiary_SkillLock_Retrievion` | Image |
| `lockimg` | `Img_Bestiary_SkillLock_Memor_04` | `Bestiary_SkillLock_MemoryExpansion` | Image |

## `Assets/@Resources/Prefab/UI/Popup/UI_CharacterPopup.prefab`

- 바인드 스크립트: `Assets/@Scripts/UI/Popup/UI_CharacterPopup.cs` (enum 멤버 169개 보호)
- 변경된 오브젝트: 38개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `CharacterPopupTitle` | `Panel_StatDetailFrame_01` | `StatDetailFrame` | — |
| `CharacterPopupTitle` | `Panel_CharacterPopupFrame_02` | `CharacterPopupFrame` | — |
| `Frame` | `Img_ASDetailGroup_01` | `ASDetailGroup` | Image |
| `Frame` | `Img_HPDetailGroup_02` | `HPDetailGroup` | Image |
| `Frame` | `Img_DROP_RATEDetailGroup_03` | `DROP_RATEDetailGroup` | Image |
| `Frame` | `Img_DEFDetailGroup_04` | `DEFDetailGroup` | Image |
| `Frame` | `Img_CDMGDetailGroup_05` | `CDMGDetailGroup` | Image |
| `Frame` | `Img_EVADetailGroup_06` | `EVADetailGroup` | Image |
| `Frame` | `Img_M_RESDetailGroup_07` | `M_RESDetailGroup` | Image |
| `Frame` | `Img_P_RESDetailGroup_08` | `P_RESDetailGroup` | Image |
| `Frame` | `Img_CCDetailGroup_09` | `CCDetailGroup` | Image |
| `Frame` | `Img_RECOVDetailGroup_10` | `RECOVDetailGroup` | Image |
| `Frame` | `Img_ACCDetailGroup_11` | `ACCDetailGroup` | Image |
| `Frame` | `Img_EXP_RATEDetailGroup_12` | `EXP_RATEDetailGroup` | Image |
| `Frame` | `Img_MATKDetailGroup_13` | `MATKDetailGroup` | Image |
| `Frame` | `Img_PATKDetailGroup_14` | `PATKDetailGroup` | Image |
| `GameObject` | `Txt_Xbutton_01` | `Xbutton` | TMP_Text |
| `GameObject` | `Img_VITInPointBtn_02` | `VITInPointBtn` | Image |
| `GameObject` | `Img_VITDePointBtn_03` | `VITDePointBtn` | Image |
| `GameObject` | `Img_STRInPointBtn_04` | `STRInPointBtn` | Image |
| `GameObject` | `Img_STRDePointBtn_05` | `STRDePointBtn` | Image |
| `GameObject` | `Img_MAGDePointBtn_06` | `MAGDePointBtn` | Image |
| `GameObject` | `Img_DEXDePointBtn_07` | `DEXDePointBtn` | Image |
| `GameObject_1` | `Img_MAGInPointBtn_01` | `MAGInPointBtn` | Image |
| `GameObject_1` | `Img_DEXInPointBtn_02` | `DEXInPointBtn` | Image |
| `Image` | `Img_PointsandElixirBar_01` | `PointsandElixirBar` | Image |
| `Image` | `Img_SubTitle_ATK_02` | `SubTitle_ATK` | Image |
| `Image` | `Img_SubTitle_UTIL_03` | `SubTitle_UTIL` | Image |
| `Image` | `Img_SubTitle_DEF_04` | `SubTitle_DEF` | Image |
| `Image_1` | `Img_SubTitle_ATK_01` | `SubTitle_ATK` | Image |
| `Image_1` | `Img_SubTitle_DEF_02` | `SubTitle_DEF` | Image |
| `Image_1` | `Img_SubTitle_UTIL_05` | `SubTitle_UTIL` | Image |
| `Layout` | `Panel_CharacterPopup_01` | `CharacterPopup` | — |
| `Layout` | `Panel_CharacterPopupTitle_02` | `CharacterPopupTitle` | — |
| `_` | `Img_CharacterPopupTitle_01` | `CharacterPopupTitle` | Image |
| `_` | `Img_CharacterPopupTitle_02` | `CharacterPopupTitle` | Image |
| `_` | `Img_CharacterPopupTitle_03` | `CharacterPopupTitle` | Image |
| `_` | `Obj_Right_04` | `Right` | — |

## `Assets/@Resources/Prefab/UI/Popup/UI_ClassPopup.prefab`

- 바인드 스크립트: 없음 (enum 보호 대상 없음)
- 변경된 오브젝트: 132개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `===================` | `Obj_Content_01` | `Content` | — |
| `===================` | `Obj_Content_02` | `Content` | — |
| `Background` | `Img_SkillBookDefenseSkill_0_04` | `SkillBookDefenseSkill_0` | Image |
| `Background` | `Img_SkillBookSkill_1_02` | `SkillBookSkill_1` | Image |
| `Background` | `Img_SkillBookSkill_6_03` | `SkillBookSkill_6` | Image |
| `Background` | `Img_SkillBookSkill_4_07` | `SkillBookSkill_4` | Image |
| `Background` | `Img_SkillBookSkill_5_05` | `SkillBookSkill_5` | Image |
| `Background` | `Img_SkillBookSkill_2_06` | `SkillBookSkill_2` | Image |
| `Background` | `Img_SkillBookSkill_3_07` | `SkillBookSkill_3` | Image |
| `Background` | `Img_SkillBookSkill_0_10` | `SkillBookSkill_0` | Image |
| `Background` | `Img_SkillBookAttackSkill_0_09` | `SkillBookAttackSkill_0` | Image |
| `EFrame` | `Img_SkillDetailActiveEf_1_01` | `SkillDetailActiveEf_1` | Image |
| `EFrame` | `Img_SkillDetailActiveEf_1_02` | `SkillDetailActiveEf_1` | Image |
| `EFrame` | `Img_SkillDetailActiveEf_1_03` | `SkillDetailActiveEf_1` | Image |
| `EFrame` | `Img_SkillDetailActiveEf_1_04` | `SkillDetailActiveEf_1` | Image |
| `EFrame` | `Img_SkillDetailActiveEf_1_05` | `SkillDetailActiveEf_1` | Image |
| `EFrame` | `Img_SkillDetailActiveEf_1_06` | `SkillDetailActiveEf_1` | Image |
| `EFrame` | `Img_SkillDetailActiveEf_1_07` | `SkillDetailActiveEf_1` | Image |
| `EFrame` | `Img_SkillDetailActiveEf_1_08` | `SkillDetailActiveEf_1` | Image |
| `EFrame` | `Img_SkillDetailActiveEf_1_09` | `SkillDetailActiveEf_1` | Image |
| `Frame` | `Img_SkillBookDefenseSkill_0_01` | `SkillBookDefenseSkill_0` | Image |
| `Frame` | `Img_SkillBookSkill_3_04` | `SkillBookSkill_3` | Image |
| `Frame` | `Img_SkillBookSkill_2_05` | `SkillBookSkill_2` | Image |
| `Frame` | `Img_SkillBookAttackSkill_0_04` | `SkillBookAttackSkill_0` | Image |
| `Frame` | `Img_SkillBookSkill_0_05` | `SkillBookSkill_0` | Image |
| `Frame` | `Img_SkillBookSkill_4_06` | `SkillBookSkill_4` | Image |
| `Frame` | `Img_SkillBookSkill_5_07` | `SkillBookSkill_5` | Image |
| `Frame` | `Img_SkillBookSkill_6_08` | `SkillBookSkill_6` | Image |
| `Frame` | `Img_SkillBookSkill_1_09` | `SkillBookSkill_1` | Image |
| `Icon_2` | `Img_MemorizedIcon_01` | `MemorizedIcon` | Image |
| `Icon_2` | `Img_MemorizedIcon_02` | `MemorizedIcon` | Image |
| `Icon_2` | `Img_MemorizedIcon_03` | `MemorizedIcon` | Image |
| `Icon_2` | `Img_MemorizedIcon_04` | `MemorizedIcon` | Image |
| `Icon_2` | `Img_MemorizedIcon_05` | `MemorizedIcon` | Image |
| `Icon_2` | `Img_MemorizedIcon_06` | `MemorizedIcon` | Image |
| `Icon_2` | `Img_MemorizedIcon_07` | `MemorizedIcon` | Image |
| `Icon_2` | `Img_MemorizedIcon_08` | `MemorizedIcon` | Image |
| `Icon_2` | `Img_MemorizedIcon_09` | `MemorizedIcon` | Image |
| `LockedIcon` | `Img_SkillBookSkill_1_01` | `SkillBookSkill_1` | Image |
| `LockedIcon` | `Img_SkillBookSkill_0_02` | `SkillBookSkill_0` | Image |
| `LockedIcon` | `Img_SkillBookSkill_2_03` | `SkillBookSkill_2` | Image |
| `LockedIcon` | `Img_SkillBookSkill_4_04` | `SkillBookSkill_4` | Image |
| `LockedIcon` | `Img_SkillBookAttackSkill_0_05` | `SkillBookAttackSkill_0` | Image |
| `LockedIcon` | `Img_SkillBookSkill_6_06` | `SkillBookSkill_6` | Image |
| `LockedIcon` | `Img_SkillBookDefenseSkill_0_07` | `SkillBookDefenseSkill_0` | Image |
| `LockedIcon` | `Img_SkillBookSkill_5_08` | `SkillBookSkill_5` | Image |
| `LockedIcon` | `Img_SkillBookSkill_3_09` | `SkillBookSkill_3` | Image |
| `LvlIcon` | `Img_SkillBookSkill_0_01` | `SkillBookSkill_0` | Image |
| `LvlIcon` | `Img_SkillBookSkill_4_02` | `SkillBookSkill_4` | Image |
| `LvlIcon` | `Img_SkillBookDefenseSkill_0_03` | `SkillBookDefenseSkill_0` | Image |
| `LvlIcon` | `Img_SkillBookAttackSkill_0_06` | `SkillBookAttackSkill_0` | Image |
| `LvlIcon` | `Img_SkillBookSkill_1_05` | `SkillBookSkill_1` | Image |
| `LvlIcon` | `Img_SkillBookSkill_5_06` | `SkillBookSkill_5` | Image |
| `LvlIcon` | `Img_SkillBookSkill_2_07` | `SkillBookSkill_2` | Image |
| `LvlIcon` | `Img_SkillBookSkill_3_08` | `SkillBookSkill_3` | Image |
| `LvlIcon` | `Img_SkillBookSkill_6_09` | `SkillBookSkill_6` | Image |
| `MemorizedIcon` | `Img_SkillBookSkill_6_01` | `SkillBookSkill_6` | Image |
| `MemorizedIcon` | `Img_SkillBookSkill_3_02` | `SkillBookSkill_3` | Image |
| `MemorizedIcon` | `Img_SkillBookSkill_5_03` | `SkillBookSkill_5` | Image |
| `MemorizedIcon` | `Img_SkillBookSkill_2_04` | `SkillBookSkill_2` | Image |
| `MemorizedIcon` | `Img_SkillBookDefenseSkill_0_05` | `SkillBookDefenseSkill_0` | Image |
| `MemorizedIcon` | `Img_SkillBookSkill_1_06` | `SkillBookSkill_1` | Image |
| `MemorizedIcon` | `Img_SkillBookAttackSkill_0_07` | `SkillBookAttackSkill_0` | Image |
| `MemorizedIcon` | `Img_SkillBookSkill_0_08` | `SkillBookSkill_0` | Image |
| `MemorizedIcon` | `Img_SkillBookSkill_4_09` | `SkillBookSkill_4` | Image |
| `SkillBookNewTxt_6` | `Txt_SkillBookSkill_6_01` | `SkillBookSkill_6` | TMP_Text |
| `SkillBookNewTxt_6` | `Txt_SkillBookAttackSkill_0_02` | `SkillBookAttackSkill_0` | TMP_Text |
| `SkillBookNewTxt_6` | `Txt_SkillBookDefenseSkill_0_03` | `SkillBookDefenseSkill_0` | TMP_Text |
| `SkillDetailActiveEf_1` | `Panel_Frame_01` | `Frame` | — |
| `SkillDetailActiveEf_1` | `Panel_Frame_02` | `Frame` | — |
| `SkillDetailActiveEf_1` | `Panel_Frame_03` | `Frame` | — |
| `SkillDetailActiveEf_1` | `Panel_Frame_04` | `Frame` | — |
| `SkillDetailActiveEf_1` | `Panel_Frame_05` | `Frame` | — |
| `SkillDetailActiveEf_1` | `Panel_Frame_06` | `Frame` | — |
| `SkillDetailActiveEf_1` | `Panel_Frame_07` | `Frame` | — |
| `SkillDetailActiveEf_1` | `Panel_Frame_08` | `Frame` | — |
| `SkillDetailActiveEf_1` | `Panel_Frame_09` | `Frame` | — |
| `Text` | `Txt_MemorizedIcon_01` | `MemorizedIcon` | TMP_Text |
| `Text` | `Txt_MemorizedIcon_02` | `MemorizedIcon` | TMP_Text |
| `Text` | `Txt_LvlIcon_03` | `LvlIcon` | TMP_Text |
| `Text` | `Txt_LvlIcon_04` | `LvlIcon` | TMP_Text |
| `Text` | `Txt_MemorizedIcon_05` | `MemorizedIcon` | TMP_Text |
| `Text` | `Txt_MemorizedIcon_06` | `MemorizedIcon` | TMP_Text |
| `Text` | `Txt_MemorizedIcon_07` | `MemorizedIcon` | TMP_Text |
| `Text` | `Txt_LvlIcon_08` | `LvlIcon` | TMP_Text |
| `Text` | `Txt_LvlIcon_09` | `LvlIcon` | TMP_Text |
| `Text` | `Txt_LvlIcon_10` | `LvlIcon` | TMP_Text |
| `Text` | `Txt_LvlIcon_11` | `LvlIcon` | TMP_Text |
| `Text` | `Txt_MemorizedIcon_12` | `MemorizedIcon` | TMP_Text |
| `Text` | `Txt_MemorizedIcon_13` | `MemorizedIcon` | TMP_Text |
| `Text` | `Txt_LvlIcon_14` | `LvlIcon` | TMP_Text |
| `Text` | `Txt_MemorizedIcon_15` | `MemorizedIcon` | TMP_Text |
| `Text` | `Txt_LvlIcon_16` | `LvlIcon` | TMP_Text |
| `Text` | `Txt_MemorizedIcon_17` | `MemorizedIcon` | TMP_Text |
| `Text` | `Txt_LvlIcon_18` | `LvlIcon` | TMP_Text |
| `WIcon` | `Btn_SkillBookDefenseSkill_0_01` | `SkillBookDefenseSkill_0` | Image,Button |
| `WIcon` | `Btn_SkillBookAttackSkill_0_02` | `SkillBookAttackSkill_0` | Image,Button |
| `WIcon` | `Btn_SkillBookSkill_1_03` | `SkillBookSkill_1` | Image,Button |
| `WIcon` | `Btn_SkillBookSkill_5_04` | `SkillBookSkill_5` | Image,Button |
| `WIcon` | `Btn_SkillBookSkill_3_05` | `SkillBookSkill_3` | Image,Button |
| `WIcon` | `Btn_SkillBookSkill_0_06` | `SkillBookSkill_0` | Image,Button |
| `WIcon` | `Btn_SkillBookSkill_4_07` | `SkillBookSkill_4` | Image,Button |
| `WIcon` | `Btn_SkillBookSkill_2_08` | `SkillBookSkill_2` | Image,Button |
| `WIcon` | `Btn_SkillBookSkill_6_09` | `SkillBookSkill_6` | Image,Button |
| `frame` | `Img_LvlIcon_01` | `LvlIcon` | Image |
| `frame` | `Img_LvlIcon_02` | `LvlIcon` | Image |
| `frame` | `Img_LvlIcon_03` | `LvlIcon` | Image |
| `frame` | `Img_LvlIcon_04` | `LvlIcon` | Image |
| `frame` | `Img_LvlIcon_05` | `LvlIcon` | Image |
| `frame` | `Img_LvlIcon_06` | `LvlIcon` | Image |
| `frame` | `Img_LvlIcon_07` | `LvlIcon` | Image |
| `frame` | `Img_LvlIcon_08` | `LvlIcon` | Image |
| `frame` | `Img_LvlIcon_09` | `LvlIcon` | Image |
| `lockimg` | `Img_LockedIcon_01` | `LockedIcon` | Image |
| `lockimg` | `Img_LockedIcon_02` | `LockedIcon` | Image |
| `lockimg` | `Img_LockedIcon_03` | `LockedIcon` | Image |
| `lockimg` | `Img_LockedIcon_04` | `LockedIcon` | Image |
| `lockimg` | `Img_LockedIcon_05` | `LockedIcon` | Image |
| `lockimg` | `Img_LockedIcon_06` | `LockedIcon` | Image |
| `lockimg` | `Img_LockedIcon_07` | `LockedIcon` | Image |
| `lockimg` | `Img_LockedIcon_08` | `LockedIcon` | Image |
| `lockimg` | `Img_LockedIcon_09` | `LockedIcon` | Image |
| `pointBox` | `Img_text_3_01` | `text_3` | Image |
| `pointBox` | `Img_text_3_02` | `text_3` | Image |
| `pointBox` | `Img_text_3_03` | `text_3` | Image |
| `pointBox` | `Img_text_3_04` | `text_3` | Image |
| `pointBox` | `Img_text_3_05` | `text_3` | Image |
| `text_3` | `Txt_SkillPopup_ShowLocked_Bt_01` | `SkillPopup_ShowLocked_Btn` | TMP_Text |
| `text_3` | `Txt_SkillPopup_ShowAll_Btn_02` | `SkillPopup_ShowAll_Btn` | TMP_Text |
| `text_3` | `Txt_SkillPopup_ShowTalent_Bt_03` | `SkillPopup_ShowTalent_Btn` | TMP_Text |
| `text_3` | `Txt_SkillPopup_ShowNotOnMemo_04` | `SkillPopup_ShowNotOnMemory_Btn` | TMP_Text |
| `text_3` | `Txt_SkillPopup_ShowOnMemory__05` | `SkillPopup_ShowOnMemory_Btn` | TMP_Text |

## `Assets/@Resources/Prefab/UI/Popup/UI_CraftAnimPopup.prefab`

- 바인드 스크립트: 없음 (enum 보호 대상 없음)
- 변경된 오브젝트: 5개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `Background` | `Img_CraftAnim_01` | `CraftAnim` | Image |
| `Background` | `Img_CraftEquipIcon_02` | `CraftEquipIcon` | Image |
| `Dust_01` | `VFX_God_ray_02_01` | `God_ray_02` | ParticleSystem |
| `Dust_01` | `VFX_GlowDot_01_1_02` | `GlowDot_01_1` | ParticleSystem |
| `Dust_01` | `VFX_BtnEmbers_03` | `BtnEmbers` | ParticleSystem |

## `Assets/@Resources/Prefab/UI/Popup/UI_CustomOrderPopup.prefab`

- 바인드 스크립트: `Assets/@Scripts/UI/Popup/UI_CustomOrderPopup.cs` (enum 멤버 11개 보호)
- 변경된 오브젝트: 16개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `Frame` | `Img_CustomOrderPopupFrame_01` | `CustomOrderPopupFrame` | Image |
| `Frame` | `Img_CustomOrder_AutoPotionCo_02` | `CustomOrder_AutoPotionCooltimeSkill` | Image |
| `GameObject` | `Txt_Xbutton_01` | `Xbutton` | TMP_Text |
| `GameObject` | `Txt_CustomOrderPotionBtn_02` | `CustomOrderPotionBtn` | TMP_Text |
| `GameObject_1` | `Img_CustomOrderPotionBtn_01` | `CustomOrderPotionBtn` | Image |
| `GameObject_1` | `Txt_CustomOrderSkillBtn_02` | `CustomOrderSkillBtn` | TMP_Text |
| `GlowMask_3` | `Mask_CustomOrderSPDPotionFram_01` | `CustomOrderSPDPotionFrame` | Image,Mask |
| `GlowMask_3` | `Mask_CustomOrderLuckPotionFra_02` | `CustomOrderLuckPotionFrame` | Image,Mask |
| `Icon_2` | `Img_LvlIcon_01` | `LvlIcon` | Image |
| `Icon_2` | `Img_MasterIcon_02` | `MasterIcon` | Image |
| `Text` | `Txt_LvlIcon_01` | `LvlIcon` | TMP_Text |
| `Text` | `Txt_MasterIcon_02` | `MasterIcon` | TMP_Text |
| `_` | `Img_Alch_Img_mask_01` | `Alch_Img_mask` | Image |
| `_` | `Img_GlowMask_2_02` | `GlowMask_2` | Image |
| `_` | `Img_GlowMask_3_03` | `GlowMask_3` | Image |
| `_` | `Img_GlowMask_3_04` | `GlowMask_3` | Image |

## `Assets/@Resources/Prefab/UI/Popup/UI_EnhacePopup.prefab`

- 바인드 스크립트: 없음 (enum 보호 대상 없음)
- 변경된 오브젝트: 85개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `AmountTxt` | `Txt_SOMBox3_01` | `SOMBox3` | TMP_Text |
| `AmountTxt` | `Txt_SOMBox1_02` | `SOMBox1` | TMP_Text |
| `AmountTxt` | `Txt_SOMBox2_03` | `SOMBox2` | TMP_Text |
| `AmountTxt` | `Txt_EnhScrollBox_04` | `EnhScrollBox` | TMP_Text |
| `AmountTxt` | `Txt_EnhScrollBox4_05` | `EnhScrollBox4` | TMP_Text |
| `AmountTxt` | `Txt_EnhScrollBox3_06` | `EnhScrollBox3` | TMP_Text |
| `AmountTxt` | `Txt_EnhScrollBox2_07` | `EnhScrollBox2` | TMP_Text |
| `Background` | `Img_EnhacePopup_01` | `EnhacePopup` | Image |
| `Background` | `Img_CraftEquipIcon_02` | `CraftEquipIcon` | Image |
| `Background` | `Img_isSkipBtn_03` | `isSkipBtn` | Image |
| `Background` | `Img_Fixed_04` | `Fixed` | Image |
| `BlockImg` | `Img_SOMBox2_01` | `SOMBox2` | Image |
| `BlockImg` | `Img_SOMBox1_02` | `SOMBox1` | Image |
| `BlockImg` | `Img_SOMBox3_03` | `SOMBox3` | Image |
| `BlockImg` | `Img_EnhScrollBox4_04` | `EnhScrollBox4` | Image |
| `BlockImg` | `Img_EnhScrollBox2_05` | `EnhScrollBox2` | Image |
| `BlockImg` | `Img_EnhScrollBox_06` | `EnhScrollBox` | Image |
| `BlockImg` | `Img_EnhScrollBox3_07` | `EnhScrollBox3` | Image |
| `Dust_01` | `VFX_God_ray_01_01` | `God_ray_01` | ParticleSystem |
| `Dust_01` | `VFX_God_ray_02_02` | `God_ray_02` | ParticleSystem |
| `Dust_01` | `VFX_GlowDot_01_1_03` | `GlowDot_01_1` | ParticleSystem |
| `Dust_01` | `VFX_BtnEmbers_04` | `BtnEmbers` | ParticleSystem |
| `EFrame` | `Img_FrameEf_01` | `FrameEf` | Image |
| `EFrame` | `Img_FrameEf_02` | `FrameEf` | Image |
| `EnhScrollBox` | `Btn_EnhacePopup_01` | `EnhacePopup` | Image,Button |
| `EnhScrollBox` | `Btn_Layout_02` | `Layout` | Image,Button |
| `Frame` | `Img_CraftEquipIcon_01` | `CraftEquipIcon` | Image |
| `Frame` | `Img_PotentialBox_02` | `PotentialBox` | Image |
| `Frame` | `Img_EnhScrollBox_03` | `EnhScrollBox` | Image |
| `Frame` | `Img_SOMBox3_04` | `SOMBox3` | Image |
| `Frame` | `Img_SOMBox2_05` | `SOMBox2` | Image |
| `Frame` | `Img_SOMBox1_06` | `SOMBox1` | Image |
| `Frame` | `Img_EnhScrollBox_07` | `EnhScrollBox` | Image |
| `Frame` | `Img_EnhScrollBox3_08` | `EnhScrollBox3` | Image |
| `Frame` | `Img_PotenBox_3_09` | `PotenBox_3` | Image |
| `Frame` | `Img_EnhScrollBox4_10` | `EnhScrollBox4` | Image |
| `Frame` | `Img_PotenBox_11` | `PotenBox` | Image |
| `Frame` | `Img_PotenBox_1_12` | `PotenBox_1` | Image |
| `Frame` | `Img_EnhScrollBox2_13` | `EnhScrollBox2` | Image |
| `Frame` | `Img_PotenBox_2_14` | `PotenBox_2` | Image |
| `FrameEf` | `Panel_EnhScrollBox_01` | `EnhScrollBox` | — |
| `FrameEf` | `Panel_PotentialBox_02` | `PotentialBox` | — |
| `GlowMask` | `Mask_PotentialBox_01` | `PotentialBox` | Image,Mask |
| `GlowMask` | `Mask_EnhScrollBox_02` | `EnhScrollBox` | Image,Mask |
| `GlowMask` | `Mask_SOMBox1_03` | `SOMBox1` | Image,Mask |
| `GlowMask` | `Mask_SOMBox3_04` | `SOMBox3` | Image,Mask |
| `GlowMask` | `Mask_SOMBox2_05` | `SOMBox2` | Image,Mask |
| `GlowMask` | `Mask_EnhScrollBox4_06` | `EnhScrollBox4` | Image,Mask |
| `GlowMask` | `Mask_EnhScrollBox_07` | `EnhScrollBox` | Image,Mask |
| `GlowMask` | `Mask_EnhScrollBox3_08` | `EnhScrollBox3` | Image,Mask |
| `GlowMask` | `Mask_EnhScrollBox2_09` | `EnhScrollBox2` | Image,Mask |
| `Image_1` | `Img_Glow_01` | `Glow` | Image |
| `Image_1` | `Img_Fixed_02` | `Fixed` | Image |
| `Img` | `Img_PotentialBox_01` | `PotentialBox` | Image |
| `Img` | `Img_EnhScrollBox_02` | `EnhScrollBox` | Image |
| `Img` | `Img_SOMBox2_03` | `SOMBox2` | Image |
| `Img` | `Img_SOMBox3_06` | `SOMBox3` | Image |
| `Img` | `Img_SOMBox1_05` | `SOMBox1` | Image |
| `Img` | `Img_EnhScrollBox2_06` | `EnhScrollBox2` | Image |
| `Img` | `Img_EnhScrollBox3_09` | `EnhScrollBox3` | Image |
| `Img` | `Img_EnhScrollBox4_08` | `EnhScrollBox4` | Image |
| `Img` | `Img_EnhScrollBox_09` | `EnhScrollBox` | Image |
| `Layout` | `Img_PotenSelectPopup_01` | `PotenSelectPopup` | Image |
| `Layout` | `Img_EnhScrollSelectPopup_02` | `EnhScrollSelectPopup` | Image |
| `Lock` | `Img_Frame_01` | `Frame` | Image |
| `Lock` | `Img_Frame_02` | `Frame` | Image |
| `Lock` | `Img_Frame_03` | `Frame` | Image |
| `Lock` | `Img_Frame_04` | `Frame` | Image |
| `Text` | `Txt_Enhance_Btn_01` | `Enhance_Btn` | TMP_Text |
| `Text` | `Txt_PotentialBox_02` | `PotentialBox` | TMP_Text |
| `Text` | `Txt_AttRealText_03` | `AttRealText` | TMP_Text |
| `Text` | `Txt_Frame_04` | `Frame` | TMP_Text |
| `Text` | `Txt_Frame_05` | `Frame` | TMP_Text |
| `Text` | `Txt_Frame_06` | `Frame` | TMP_Text |
| `Text` | `Txt_Frame_07` | `Frame` | TMP_Text |
| `Text` | `Txt_Handle_08` | `Handle` | TMP_Text |
| `_` | `Img_GlowMask_01` | `GlowMask` | Image |
| `_` | `Img_GlowMask_02` | `GlowMask` | Image |
| `_` | `Img_GlowMask_03` | `GlowMask` | Image |
| `_` | `Img_GlowMask_04` | `GlowMask` | Image |
| `_` | `Img_GlowMask_05` | `GlowMask` | Image |
| `_` | `Img_GlowMask_06` | `GlowMask` | Image |
| `_` | `Img_GlowMask_07` | `GlowMask` | Image |
| `_` | `Img_GlowMask_08` | `GlowMask` | Image |
| `_` | `Img_GlowMask_09` | `GlowMask` | Image |

## `Assets/@Resources/Prefab/UI/Popup/UI_EnhanceProgressPopup.prefab`

- 바인드 스크립트: 없음 (enum 보호 대상 없음)
- 변경된 오브젝트: 10개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `Image` | `Img_EnhanceProgressPopup_01` | `EnhanceProgressPopup` | Image |
| `Image` | `Img_Text_1_02` | `Text_1` | Image |
| `Text_1` | `Txt_PotenGroup_01` | `PotenGroup` | TMP_Text |
| `Text_1` | `Txt_AttRealText_02` | `AttRealText` | TMP_Text |
| `_0` | `Txt_SuccessTxtGroup_01` | `SuccessTxtGroup` | TMP_Text |
| `_0` | `Txt_FailTxtGroup_02` | `FailTxtGroup` | TMP_Text |
| `_1` | `Txt_FailTxtGroup_01` | `FailTxtGroup` | TMP_Text |
| `_1` | `Img_SuccessTxtGroup_02` | `SuccessTxtGroup` | Image |
| `_3` | `Txt_SuccessTxtGroup_03` | `SuccessTxtGroup` | TMP_Text |
| `_3` | `Txt_FailTxtGroup_04` | `FailTxtGroup` | TMP_Text |

## `Assets/@Resources/Prefab/UI/Popup/UI_EquipDetailPopup.prefab`

- 바인드 스크립트: 없음 (enum 보호 대상 없음)
- 변경된 오브젝트: 36개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `Flash` | `Img_TopFrame_03` | `TopFrame` | Image |
| `Flash` | `Img_EquipPopupIcon_02` | `EquipPopupIcon` | Image |
| `Frame` | `Img_TopFrame_04` | `TopFrame` | Image |
| `Frame` | `Img_EquipPopupIcon_04` | `EquipPopupIcon` | Image |
| `GameObject` | `Txt_Xbutton_01` | `Xbutton` | TMP_Text |
| `GameObject` | `Img_EquipAmountDetailBtn_02` | `EquipAmountDetailBtn` | Image |
| `Glow` | `Img_TopFrame_01` | `TopFrame` | Image |
| `Glow` | `Img_Headline_02` | `Headline` | Image |
| `Glow` | `Mask_EquipPopupIcon_03` | `EquipPopupIcon` | Image,Mask |
| `Image` | `Img_DontHaveBtn_01` | `DontHaveBtn` | Image |
| `Image` | `Img_EnhaceBtn_02` | `EnhaceBtn` | Image |
| `Image` | `Img_OnEquipBtn_03` | `OnEquipBtn` | Image |
| `Image` | `Img_EquipBtn_04` | `EquipBtn` | Image |
| `Image` | `Img_mask_05` | `mask` | Image |
| `Image` | `Img_mask_1_06` | `mask_1` | Image |
| `Image` | `Img_mask_1_07` | `mask_1` | Image |
| `Text` | `Txt_TopFrame_01` | `TopFrame` | TMP_Text |
| `Text` | `Txt_OnEquipBtn_02` | `OnEquipBtn` | TMP_Text |
| `Text` | `Txt_DontHaveBtn_03` | `DontHaveBtn` | TMP_Text |
| `Text` | `Txt_EquipBtn_04` | `EquipBtn` | TMP_Text |
| `Text` | `Txt_EnhaceBtn_05` | `EnhaceBtn` | TMP_Text |
| `Text` | `Txt_EIcon_06` | `EIcon` | TMP_Text |
| `_` | `Btn_EquipAmountDetailPopup_01` | `EquipAmountDetailPopup` | Image,Button |
| `_` | `Txt_EquipStatTxtGroup_02` | `EquipStatTxtGroup` | TMP_Text |
| `_` | `Img_X_1_03` | `__1` | Image |
| `_` | `Txt_Headline_04` | `Headline` | TMP_Text |
| `_` | `Txt_EquipSkillTxtGroup_05` | `EquipSkillTxtGroup` | TMP_Text |
| `_` | `Img_X_1_06` | `__1` | Image |
| `_` | `Txt_EquipDuriTxtGroup_07` | `EquipDuriTxtGroup` | TMP_Text |
| `_` | `Txt_X_1_08` | `__1` | TMP_Text |
| `__1` | `Btn_EquipAmountDetailPopup_03` | `EquipAmountDetailPopup` | Button,TMP_Text |
| `__1` | `Txt_X_1_02` | `__1` | TMP_Text |
| `__2` | `Img_EquipPopupFrame_01` | `EquipPopupFrame` | Image |
| `__2` | `Txt_X_1_04` | `__1` | TMP_Text |
| `mask_1` | `Mask_EnhaceBtn_01` | `EnhaceBtn` | Image,Mask |
| `mask_1` | `Mask_OnEquipBtn_02` | `OnEquipBtn` | Image,Mask |

## `Assets/@Resources/Prefab/UI/Popup/UI_EquipmentPopup.prefab`

- 바인드 스크립트: 없음 (enum 보호 대상 없음)
- 변경된 오브젝트: 418개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `Background` | `Img_B00Btn_01` | `B00Btn` | Image |
| `Background` | `Img_H00Btn_02` | `H00Btn` | Image |
| `Background` | `Img_W02Btn_07` | `W02Btn` | Image |
| `Background` | `Img_W00Btn_04` | `W00Btn` | Image |
| `Background` | `Img_W44Btn_07` | `W44Btn` | Image |
| `Background` | `Img_G00Btn_06` | `G00Btn` | Image |
| `Background` | `Img_C00Btn_10` | `C00Btn` | Image |
| `Background` | `Img_A00Btn_08` | `A00Btn` | Image |
| `Background` | `Img_W01Btn_11` | `W01Btn` | Image |
| `Background` | `Img_R00Btn_10` | `R00Btn` | Image |
| `Backlight` | `Img_Mask_01` | `Mask` | Image |
| `Backlight` | `Img_Mask_02` | `Mask` | Image |
| `Backlight` | `Img_Mask_03` | `Mask` | Image |
| `Backlight` | `Img_Mask_04` | `Mask` | Image |
| `Backlight` | `Img_Mask_05` | `Mask` | Image |
| `Backlight` | `Img_Mask_06` | `Mask` | Image |
| `Backlight` | `Img_Mask_07` | `Mask` | Image |
| `BlockImg` | `Img_W00Btn_01` | `W00Btn` | Image |
| `BlockImg` | `Img_C00Btn_04` | `C00Btn` | Image |
| `BlockImg` | `Img_W02Btn_05` | `W02Btn` | Image |
| `BlockImg` | `Img_H00Btn_04` | `H00Btn` | Image |
| `BlockImg` | `Img_W44Btn_05` | `W44Btn` | Image |
| `BlockImg` | `Img_R00Btn_08` | `R00Btn` | Image |
| `BlockImg` | `Img_B00Btn_09` | `B00Btn` | Image |
| `BlockImg` | `Img_W01Btn_10` | `W01Btn` | Image |
| `BlockImg` | `Img_G00Btn_09` | `G00Btn` | Image |
| `BlockImg` | `Img_A00Btn_12` | `A00Btn` | Image |
| `Content` | `Obj_Viewport_01` | `Viewport` | — |
| `Content` | `Panel_Viewport_02` | `Viewport` | — |
| `Content` | `Obj_Viewport_03` | `Viewport` | — |
| `Content` | `Panel_Viewport_04` | `Viewport` | — |
| `Content` | `Obj_Viewport_05` | `Viewport` | — |
| `Content` | `Obj_Viewport_06` | `Viewport` | — |
| `Content` | `Obj_Viewport_07` | `Viewport` | — |
| `Content` | `Panel_Viewport_08` | `Viewport` | — |
| `Content` | `Obj_Viewport_09` | `Viewport` | — |
| `Content` | `Obj_Viewport_10` | `Viewport` | — |
| `Content` | `Obj_Viewport_11` | `Viewport` | — |
| `Content` | `Obj_Viewport_12` | `Viewport` | — |
| `Content` | `Panel_Viewport_13` | `Viewport` | — |
| `Content` | `Panel_Viewport_14` | `Viewport` | — |
| `Content` | `Panel_Viewport_15` | `Viewport` | — |
| `Content` | `Obj_Viewport_16` | `Viewport` | — |
| `Content` | `Obj_Viewport_17` | `Viewport` | — |
| `Content` | `Obj_Viewport_18` | `Viewport` | — |
| `Content` | `Panel_Viewport_19` | `Viewport` | — |
| `Content` | `Obj_Viewport_20` | `Viewport` | — |
| `Content` | `Obj_Viewport_21` | `Viewport` | — |
| `EFrame` | `Img_FrameEf_11` | `FrameEf` | Image |
| `EFrame` | `Img_FrameEf_12` | `FrameEf` | Image |
| `EFrame` | `Img_FrameEf_13` | `FrameEf` | Image |
| `EFrame` | `Img_FrameEf_14` | `FrameEf` | Image |
| `EFrame` | `Img_FrameEf_15` | `FrameEf` | Image |
| `EFrame` | `Img_FrameEf_16` | `FrameEf` | Image |
| `EFrame` | `Img_FrameEf_17` | `FrameEf` | Image |
| `EFrame` | `Img_FrameEf_18` | `FrameEf` | Image |
| `EFrame` | `Img_FrameEf_19` | `FrameEf` | Image |
| `EFrame` | `Img_FrameEf_20` | `FrameEf` | Image |
| `EFrame_1` | `Img_FrameEf_01` | `FrameEf` | Image |
| `EFrame_1` | `Img_FrameEf_02` | `FrameEf` | Image |
| `EFrame_1` | `Img_FrameEf_03` | `FrameEf` | Image |
| `EFrame_1` | `Img_FrameEf_04` | `FrameEf` | Image |
| `EFrame_1` | `Img_FrameEf_05` | `FrameEf` | Image |
| `EFrame_1` | `Img_FrameEf_06` | `FrameEf` | Image |
| `EFrame_1` | `Img_FrameEf_07` | `FrameEf` | Image |
| `EFrame_1` | `Img_FrameEf_08` | `FrameEf` | Image |
| `EFrame_1` | `Img_FrameEf_09` | `FrameEf` | Image |
| `EFrame_1` | `Img_FrameEf_10` | `FrameEf` | Image |
| `EIcon` | `Img_EquipedEf_01` | `EquipedEf` | Image |
| `EIcon` | `Img_EquipedEf_02` | `EquipedEf` | Image |
| `EIcon` | `Img_EquipedEf_03` | `EquipedEf` | Image |
| `EIcon` | `Img_EquipedEf_04` | `EquipedEf` | Image |
| `EIcon` | `Img_EquipedEf_05` | `EquipedEf` | Image |
| `EIcon` | `Img_EquipedEf_06` | `EquipedEf` | Image |
| `EIcon` | `Img_EquipedEf_07` | `EquipedEf` | Image |
| `EIcon` | `Img_EquipedEf_08` | `EquipedEf` | Image |
| `EIcon` | `Img_EquipedEf_09` | `EquipedEf` | Image |
| `EIcon` | `Img_EquipedEf_10` | `EquipedEf` | Image |
| `ENumber` | `Txt_C00Btn_01` | `C00Btn` | TMP_Text |
| `ENumber` | `Txt_A00Btn_02` | `A00Btn` | TMP_Text |
| `ENumber` | `Txt_B00Btn_03` | `B00Btn` | TMP_Text |
| `ENumber` | `Txt_W00Btn_04` | `W00Btn` | TMP_Text |
| `ENumber` | `Txt_W02Btn_05` | `W02Btn` | TMP_Text |
| `ENumber` | `Txt_H00Btn_06` | `H00Btn` | TMP_Text |
| `ENumber` | `Txt_G00Btn_07` | `G00Btn` | TMP_Text |
| `ENumber` | `Txt_R00Btn_08` | `R00Btn` | TMP_Text |
| `ENumber` | `Txt_W44Btn_09` | `W44Btn` | TMP_Text |
| `ENumber` | `Txt_W01Btn_10` | `W01Btn` | TMP_Text |
| `EquipedEf` | `Panel_W44Btn_01` | `W44Btn` | — |
| `EquipedEf` | `Panel_H00Btn_02` | `H00Btn` | — |
| `EquipedEf` | `Panel_W02Btn_03` | `W02Btn` | — |
| `EquipedEf` | `Panel_B00Btn_04` | `B00Btn` | — |
| `EquipedEf` | `Panel_A00Btn_05` | `A00Btn` | — |
| `EquipedEf` | `Panel_R00Btn_06` | `R00Btn` | — |
| `EquipedEf` | `Panel_G00Btn_07` | `G00Btn` | — |
| `EquipedEf` | `Panel_C00Btn_08` | `C00Btn` | — |
| `EquipedEf` | `Panel_W01Btn_09` | `W01Btn` | — |
| `EquipedEf` | `Panel_W00Btn_10` | `W00Btn` | — |
| `Flash` | `Img_R00Btn_01` | `R00Btn` | Image |
| `Flash` | `Img_G00Btn_02` | `G00Btn` | Image |
| `Flash` | `Img_W02Btn_06` | `W02Btn` | Image |
| `Flash` | `Img_A00Btn_06` | `A00Btn` | Image |
| `Flash` | `Img_W00Btn_07` | `W00Btn` | Image |
| `Flash` | `Img_B00Btn_06` | `B00Btn` | Image |
| `Flash` | `Img_C00Btn_07` | `C00Btn` | Image |
| `Flash` | `Img_W44Btn_11` | `W44Btn` | Image |
| `Flash` | `Img_W01Btn_09` | `W01Btn` | Image |
| `Flash` | `Img_H00Btn_10` | `H00Btn` | Image |
| `Frame` | `Img_ArmorList_01` | `ArmorList` | Image |
| `Frame` | `Img_CloakList_02` | `CloakList` | Image |
| `Frame` | `Img_BootsList_03` | `BootsList` | Image |
| `Frame` | `Img_GloveList_04` | `GloveList` | Image |
| `Frame` | `Img_WeaponList_05` | `WeaponList` | Image |
| `Frame` | `Img_HelmetList_06` | `HelmetList` | Image |
| `Frame` | `Img_RingList_07` | `RingList` | Image |
| `Frame` | `Img_W44Btn_08` | `W44Btn` | Image |
| `Frame` | `Img_C00Btn_09` | `C00Btn` | Image |
| `Frame` | `Img_A00Btn_10` | `A00Btn` | Image |
| `Frame` | `Img_R00Btn_11` | `R00Btn` | Image |
| `Frame` | `Img_G00Btn_12` | `G00Btn` | Image |
| `Frame` | `Img_W02Btn_13` | `W02Btn` | Image |
| `Frame` | `Img_H00Btn_14` | `H00Btn` | Image |
| `Frame` | `Img_B00Btn_15` | `B00Btn` | Image |
| `Frame` | `Img_W01Btn_16` | `W01Btn` | Image |
| `Frame` | `Img_W00Btn_17` | `W00Btn` | Image |
| `FrameEf` | `Panel_EquipedEf_01` | `EquipedEf` | — |
| `FrameEf` | `Panel_EquipedEf_02` | `EquipedEf` | — |
| `FrameEf` | `Panel_EquipedEf_03` | `EquipedEf` | — |
| `FrameEf` | `Panel_EquipedEf_04` | `EquipedEf` | — |
| `FrameEf` | `Panel_EquipedEf_05` | `EquipedEf` | — |
| `FrameEf` | `Panel_EquipedEf_06` | `EquipedEf` | — |
| `FrameEf` | `Panel_EquipedEf_07` | `EquipedEf` | — |
| `FrameEf` | `Panel_EquipedEf_08` | `EquipedEf` | — |
| `FrameEf` | `Panel_EquipedEf_09` | `EquipedEf` | — |
| `FrameEf` | `Panel_EquipedEf_10` | `EquipedEf` | — |
| `Glow` | `Mask_W00Btn_01` | `W00Btn` | Image,Mask |
| `Glow` | `Mask_H00Btn_02` | `H00Btn` | Image,Mask |
| `Glow` | `Mask_W44Btn_03` | `W44Btn` | Image,Mask |
| `Glow` | `Mask_W02Btn_04` | `W02Btn` | Image,Mask |
| `Glow` | `Mask_A00Btn_05` | `A00Btn` | Image,Mask |
| `Glow` | `Mask_G00Btn_06` | `G00Btn` | Image,Mask |
| `Glow` | `Mask_C00Btn_07` | `C00Btn` | Image,Mask |
| `Glow` | `Mask_W01Btn_08` | `W01Btn` | Image,Mask |
| `Glow` | `Mask_R00Btn_09` | `R00Btn` | Image,Mask |
| `Glow` | `Mask_B00Btn_10` | `B00Btn` | Image,Mask |
| `Handle` | `Img_SlidingArea_01` | `SlidingArea` | Image |
| `Handle` | `Img_SlidingArea_02` | `SlidingArea` | Image |
| `Handle` | `Img_SlidingArea_03` | `SlidingArea` | Image |
| `Handle` | `Img_SlidingArea_04` | `SlidingArea` | Image |
| `Handle` | `Img_SlidingArea_05` | `SlidingArea` | Image |
| `Handle` | `Img_SlidingArea_06` | `SlidingArea` | Image |
| `Handle` | `Img_SlidingArea_07` | `SlidingArea` | Image |
| `Handle` | `Img_SlidingArea_08` | `SlidingArea` | Image |
| `Handle` | `Img_SlidingArea_09` | `SlidingArea` | Image |
| `Handle` | `Img_SlidingArea_10` | `SlidingArea` | Image |
| `Handle` | `Img_SlidingArea_11` | `SlidingArea` | Image |
| `Handle` | `Img_SlidingArea_12` | `SlidingArea` | Image |
| `Handle` | `Img_SlidingArea_13` | `SlidingArea` | Image |
| `Handle` | `Img_SlidingArea_14` | `SlidingArea` | Image |
| `Handle` | `Img_SlidingArea_15` | `SlidingArea` | Image |
| `Handle` | `Img_SlidingArea_16` | `SlidingArea` | Image |
| `Handle` | `Img_SlidingArea_17` | `SlidingArea` | Image |
| `Handle` | `Img_SlidingArea_18` | `SlidingArea` | Image |
| `Handle` | `Img_SlidingArea_19` | `SlidingArea` | Image |
| `Handle` | `Img_SlidingArea_20` | `SlidingArea` | Image |
| `Handle` | `Img_SlidingArea_21` | `SlidingArea` | Image |
| `Icon_2` | `Img_EIcon_01` | `EIcon` | Image |
| `Icon_2` | `Img_EIcon_02` | `EIcon` | Image |
| `Icon_2` | `Img_EIcon_03` | `EIcon` | Image |
| `Icon_2` | `Img_EIcon_04` | `EIcon` | Image |
| `Icon_2` | `Img_EIcon_05` | `EIcon` | Image |
| `Icon_2` | `Img_EIcon_06` | `EIcon` | Image |
| `Icon_2` | `Img_EIcon_07` | `EIcon` | Image |
| `Icon_2` | `Img_EIcon_08` | `EIcon` | Image |
| `Icon_2` | `Img_EIcon_09` | `EIcon` | Image |
| `Icon_2` | `Img_EIcon_10` | `EIcon` | Image |
| `Icon_LowDuri` | `Img_W00Btn_03` | `W00Btn` | Image,Animator |
| `Icon_LowDuri` | `Img_W44Btn_02` | `W44Btn` | Image,Animator |
| `Icon_LowDuri` | `Img_W01Btn_03` | `W01Btn` | Image,Animator |
| `Icon_LowDuri` | `Img_W02Btn_04` | `W02Btn` | Image,Animator |
| `Icon_LowDuri_1` | `Img_H00Btn_01` | `H00Btn` | Image,Animator |
| `Icon_LowDuri_1` | `Img_C00Btn_05` | `C00Btn` | Image,Animator |
| `Image` | `Img_HelmetList_01` | `HelmetList` | Image |
| `Image` | `Img_ArmorList_02` | `ArmorList` | Image |
| `Image` | `Img_RingList_03` | `RingList` | Image |
| `Image` | `Img_CloakList_04` | `CloakList` | Image |
| `Image` | `Img_GloveList_05` | `GloveList` | Image |
| `Image` | `Img_WeaponList_06` | `WeaponList` | Image |
| `Image` | `Img_BootsList_07` | `BootsList` | Image |
| `Image` | `Img_CloakT2Btn_08` | `CloakT2Btn` | Image |
| `Image` | `Img_WeaponT1Btn_09` | `WeaponT1Btn` | Image |
| `Image` | `Img_CloakT1Btn_10` | `CloakT1Btn` | Image |
| `Image` | `Img_ArmorT1Btn_11` | `ArmorT1Btn` | Image |
| `Image` | `Img_GloveT2Btn_12` | `GloveT2Btn` | Image |
| `Image` | `Img_RingT2Btn_13` | `RingT2Btn` | Image |
| `Image` | `Img_RingT3Btn_14` | `RingT3Btn` | Image |
| `Image` | `Img_RingT1Btn_15` | `RingT1Btn` | Image |
| `Image` | `Img_GloveT1Btn_16` | `GloveT1Btn` | Image |
| `Image` | `Img_WeaponT2Btn_17` | `WeaponT2Btn` | Image |
| `Image` | `Img_ArmorT2Btn_18` | `ArmorT2Btn` | Image |
| `Image` | `Img_BootsT3Btn_19` | `BootsT3Btn` | Image |
| `Image` | `Img_WeaponT2Btn_20` | `WeaponT2Btn` | Image |
| `Image` | `Img_BootsT1Btn_21` | `BootsT1Btn` | Image |
| `Image` | `Img_ArmorT3Btn_22` | `ArmorT3Btn` | Image |
| `Image` | `Img_CloakT3Btn_23` | `CloakT3Btn` | Image |
| `Image` | `Img_WeaponT1Btn_24` | `WeaponT1Btn` | Image |
| `Image` | `Img_BootsT2Btn_25` | `BootsT2Btn` | Image |
| `Image` | `Img_WeaponT3Btn_26` | `WeaponT3Btn` | Image |
| `Image` | `Img_WeaponT3Btn_27` | `WeaponT3Btn` | Image |
| `Image` | `Img_GloveT3Btn_28` | `GloveT3Btn` | Image |
| `Image` | `Img_Mask_29` | `Mask` | Image |
| `Image` | `Img_Mask_30` | `Mask` | Image |
| `Image` | `Img_Mask_31` | `Mask` | Image |
| `Image` | `Img_Mask_32` | `Mask` | Image |
| `Image` | `Img_Mask_33` | `Mask` | Image |
| `Image` | `Img_Mask_34` | `Mask` | Image |
| `Image` | `Img_Mask_35` | `Mask` | Image |
| `Image` | `Img_Mask_36` | `Mask` | Image |
| `Image` | `Img_Mask_37` | `Mask` | Image |
| `Image` | `Img_Mask_38` | `Mask` | Image |
| `Image` | `Img_Mask_39` | `Mask` | Image |
| `Image` | `Img_Mask_40` | `Mask` | Image |
| `Image` | `Img_Mask_41` | `Mask` | Image |
| `Image` | `Img_Mask_42` | `Mask` | Image |
| `Image` | `Img_Mask_43` | `Mask` | Image |
| `Image` | `Img_Mask_44` | `Mask` | Image |
| `Image` | `Img_Mask_45` | `Mask` | Image |
| `Image` | `Img_Mask_46` | `Mask` | Image |
| `Image` | `Img_Mask_47` | `Mask` | Image |
| `Image` | `Img_Mask_48` | `Mask` | Image |
| `Image` | `Img_Mask_49` | `Mask` | Image |
| `Image` | `Img_E_W_Arrow1_50` | `E_W_Arrow1` | Image |
| `Image_1` | `Img_Glow_01` | `Glow` | Image |
| `Image_1` | `Img_Glow_02` | `Glow` | Image |
| `Image_1` | `Img_Glow_03` | `Glow` | Image |
| `Image_1` | `Img_Glow_04` | `Glow` | Image |
| `Image_1` | `Img_Glow_05` | `Glow` | Image |
| `Image_1` | `Img_Glow_06` | `Glow` | Image |
| `Image_1` | `Img_Glow_07` | `Glow` | Image |
| `Image_1` | `Img_Glow_08` | `Glow` | Image |
| `Image_1` | `Img_Glow_09` | `Glow` | Image |
| `Image_1` | `Img_Glow_10` | `Glow` | Image |
| `Image_2` | `Img_Glow_11` | `Glow` | Image |
| `Image_2` | `Img_Glow_12` | `Glow` | Image |
| `Image_2` | `Img_Glow_13` | `Glow` | Image |
| `Image_2` | `Img_Glow_14` | `Glow` | Image |
| `Image_2` | `Img_Glow_15` | `Glow` | Image |
| `Image_2` | `Img_Glow_16` | `Glow` | Image |
| `Image_2` | `Img_Glow_17` | `Glow` | Image |
| `Image_2` | `Img_Glow_18` | `Glow` | Image |
| `Image_2` | `Img_Glow_19` | `Glow` | Image |
| `Image_2` | `Img_Glow_20` | `Glow` | Image |
| `Mask` | `Mask_OpenGloveListBtn_01` | `OpenGloveListBtn` | Image,Mask |
| `Mask` | `Mask_OpenBootsListBtn_02` | `OpenBootsListBtn` | Image,Mask |
| `Mask` | `Mask_OpenHelemtListBtn_03` | `OpenHelemtListBtn` | Image,Mask |
| `Mask` | `Mask_OpenCloakListBtn_04` | `OpenCloakListBtn` | Image,Mask |
| `Mask` | `Mask_OpenRingListBtn_05` | `OpenRingListBtn` | Image,Mask |
| `Mask` | `Mask_OpenWeaponListBtn_06` | `OpenWeaponListBtn` | Image,Mask |
| `Mask` | `Mask_OpenArmorListBtn_07` | `OpenArmorListBtn` | Image,Mask |
| `Mask` | `Mask_GloveT2Btn_08` | `GloveT2Btn` | Image,Mask |
| `Mask` | `Mask_ArmorT1Btn_09` | `ArmorT1Btn` | Image,Mask |
| `Mask` | `Mask_WeaponT3Btn_10` | `WeaponT3Btn` | Image,Mask |
| `Mask` | `Mask_WeaponT1Btn_11` | `WeaponT1Btn` | Image,Mask |
| `Mask` | `Mask_ArmorT3Btn_12` | `ArmorT3Btn` | Image,Mask |
| `Mask` | `Mask_RingT2Btn_13` | `RingT2Btn` | Image,Mask |
| `Mask` | `Mask_GloveT3Btn_14` | `GloveT3Btn` | Image,Mask |
| `Mask` | `Mask_BootsT2Btn_15` | `BootsT2Btn` | Image,Mask |
| `Mask` | `Mask_CloakT1Btn_16` | `CloakT1Btn` | Image,Mask |
| `Mask` | `Mask_BootsT3Btn_17` | `BootsT3Btn` | Image,Mask |
| `Mask` | `Mask_BootsT1Btn_18` | `BootsT1Btn` | Image,Mask |
| `Mask` | `Mask_RingT1Btn_19` | `RingT1Btn` | Image,Mask |
| `Mask` | `Mask_CloakT3Btn_20` | `CloakT3Btn` | Image,Mask |
| `Mask` | `Mask_GloveT1Btn_21` | `GloveT1Btn` | Image,Mask |
| `Mask` | `Mask_CloakT2Btn_22` | `CloakT2Btn` | Image,Mask |
| `Mask` | `Mask_RingT3Btn_23` | `RingT3Btn` | Image,Mask |
| `Mask` | `Mask_WeaponT2Btn_24` | `WeaponT2Btn` | Image,Mask |
| `Mask` | `Mask_ArmorT2Btn_25` | `ArmorT2Btn` | Image,Mask |
| `Mask` | `Mask_WeaponT1Btn_26` | `WeaponT1Btn` | Image,Mask |
| `Mask` | `Mask_WeaponT3Btn_27` | `WeaponT3Btn` | Image,Mask |
| `Mask` | `Mask_WeaponT2Btn_28` | `WeaponT2Btn` | Image,Mask |
| `ScrollbarVertical` | `Sb_HelmetT1List_01` | `HelmetT1List` | Image,Scrollbar |
| `ScrollbarVertical` | `Sb_RingT2List_02` | `RingT2List` | Image,Scrollbar |
| `ScrollbarVertical` | `Sb_RingT3List_03` | `RingT3List` | Image,Scrollbar |
| `ScrollbarVertical` | `Sb_GloveT3List_04` | `GloveT3List` | Image,Scrollbar |
| `ScrollbarVertical` | `Sb_CloakT3List_05` | `CloakT3List` | Image,Scrollbar |
| `ScrollbarVertical` | `Sb_GloveT1List_06` | `GloveT1List` | Image,Scrollbar |
| `ScrollbarVertical` | `Sb_WeaponT1List_07` | `WeaponT1List` | Image,Scrollbar |
| `ScrollbarVertical` | `Sb_ArmorT1List_08` | `ArmorT1List` | Image,Scrollbar |
| `ScrollbarVertical` | `Sb_BootsT2List_09` | `BootsT2List` | Image,Scrollbar |
| `ScrollbarVertical` | `Sb_WeaponT2List_10` | `WeaponT2List` | Image,Scrollbar |
| `ScrollbarVertical` | `Sb_HelmetT2List_11` | `HelmetT2List` | Image,Scrollbar |
| `ScrollbarVertical` | `Sb_WeaponT3List_12` | `WeaponT3List` | Image,Scrollbar |
| `ScrollbarVertical` | `Sb_CloakT2List_13` | `CloakT2List` | Image,Scrollbar |
| `ScrollbarVertical` | `Sb_HelmetT3List_14` | `HelmetT3List` | Image,Scrollbar |
| `ScrollbarVertical` | `Sb_ArmorT2List_15` | `ArmorT2List` | Image,Scrollbar |
| `ScrollbarVertical` | `Sb_BootsT1List_16` | `BootsT1List` | Image,Scrollbar |
| `ScrollbarVertical` | `Sb_RingT1List_17` | `RingT1List` | Image,Scrollbar |
| `ScrollbarVertical` | `Sb_BootsT3List_18` | `BootsT3List` | Image,Scrollbar |
| `ScrollbarVertical` | `Sb_GloveT2List_19` | `GloveT2List` | Image,Scrollbar |
| `ScrollbarVertical` | `Sb_CloakT1List_20` | `CloakT1List` | Image,Scrollbar |
| `ScrollbarVertical` | `Sb_ArmorT3List_21` | `ArmorT3List` | Image,Scrollbar |
| `Select` | `Img_OpenWeaponListBtn_01` | `OpenWeaponListBtn` | Image |
| `Select` | `Img_OpenArmorListBtn_02` | `OpenArmorListBtn` | Image |
| `Select` | `Img_OpenGloveListBtn_03` | `OpenGloveListBtn` | Image |
| `Select` | `Img_OpenRingListBtn_04` | `OpenRingListBtn` | Image |
| `Select` | `Img_OpenCloakListBtn_05` | `OpenCloakListBtn` | Image |
| `Select` | `Img_OpenHelemtListBtn_06` | `OpenHelemtListBtn` | Image |
| `Select` | `Img_OpenBootsListBtn_07` | `OpenBootsListBtn` | Image |
| `SelectLine` | `Img_ArmorT1Btn_01` | `ArmorT1Btn` | Image |
| `SelectLine` | `Img_WeaponT2Btn_02` | `WeaponT2Btn` | Image |
| `SelectLine` | `Img_ArmorT2Btn_03` | `ArmorT2Btn` | Image |
| `SelectLine` | `Img_GloveT2Btn_04` | `GloveT2Btn` | Image |
| `SelectLine` | `Img_BootsT1Btn_05` | `BootsT1Btn` | Image |
| `SelectLine` | `Img_CloakT3Btn_06` | `CloakT3Btn` | Image |
| `SelectLine` | `Img_WeaponT3Btn_07` | `WeaponT3Btn` | Image |
| `SelectLine` | `Img_BootsT2Btn_08` | `BootsT2Btn` | Image |
| `SelectLine` | `Img_WeaponT1Btn_11` | `WeaponT1Btn` | Image |
| `SelectLine` | `Img_WeaponT2Btn_10` | `WeaponT2Btn` | Image |
| `SelectLine` | `Img_WeaponT1Btn_13` | `WeaponT1Btn` | Image |
| `SelectLine` | `Img_CloakT2Btn_12` | `CloakT2Btn` | Image |
| `SelectLine` | `Img_GloveT3Btn_13` | `GloveT3Btn` | Image |
| `SelectLine` | `Img_RingT3Btn_16` | `RingT3Btn` | Image |
| `SelectLine` | `Img_RingT1Btn_17` | `RingT1Btn` | Image |
| `SelectLine` | `Img_CloakT1Btn_16` | `CloakT1Btn` | Image |
| `SelectLine` | `Img_RingT2Btn_17` | `RingT2Btn` | Image |
| `SelectLine` | `Img_BootsT3Btn_18` | `BootsT3Btn` | Image |
| `SelectLine` | `Img_WeaponT3Btn_19` | `WeaponT3Btn` | Image |
| `SelectLine` | `Img_GloveT1Btn_20` | `GloveT1Btn` | Image |
| `SelectLine` | `Img_ArmorT3Btn_21` | `ArmorT3Btn` | Image |
| `SlidingArea` | `Panel_ScrollbarVertical_01` | `ScrollbarVertical` | — |
| `SlidingArea` | `Panel_ScrollbarVertical_02` | `ScrollbarVertical` | — |
| `SlidingArea` | `Panel_ScrollbarVertical_03` | `ScrollbarVertical` | — |
| `SlidingArea` | `Panel_ScrollbarVertical_04` | `ScrollbarVertical` | — |
| `SlidingArea` | `Panel_ScrollbarVertical_05` | `ScrollbarVertical` | — |
| `SlidingArea` | `Panel_ScrollbarVertical_06` | `ScrollbarVertical` | — |
| `SlidingArea` | `Panel_ScrollbarVertical_07` | `ScrollbarVertical` | — |
| `SlidingArea` | `Panel_ScrollbarVertical_08` | `ScrollbarVertical` | — |
| `SlidingArea` | `Panel_ScrollbarVertical_09` | `ScrollbarVertical` | — |
| `SlidingArea` | `Panel_ScrollbarVertical_10` | `ScrollbarVertical` | — |
| `SlidingArea` | `Panel_ScrollbarVertical_11` | `ScrollbarVertical` | — |
| `SlidingArea` | `Panel_ScrollbarVertical_12` | `ScrollbarVertical` | — |
| `SlidingArea` | `Panel_ScrollbarVertical_13` | `ScrollbarVertical` | — |
| `SlidingArea` | `Panel_ScrollbarVertical_14` | `ScrollbarVertical` | — |
| `SlidingArea` | `Panel_ScrollbarVertical_15` | `ScrollbarVertical` | — |
| `SlidingArea` | `Panel_ScrollbarVertical_16` | `ScrollbarVertical` | — |
| `SlidingArea` | `Panel_ScrollbarVertical_17` | `ScrollbarVertical` | — |
| `SlidingArea` | `Panel_ScrollbarVertical_18` | `ScrollbarVertical` | — |
| `SlidingArea` | `Panel_ScrollbarVertical_19` | `ScrollbarVertical` | — |
| `SlidingArea` | `Panel_ScrollbarVertical_20` | `ScrollbarVertical` | — |
| `SlidingArea` | `Panel_ScrollbarVertical_21` | `ScrollbarVertical` | — |
| `Text` | `Txt_CloakT1Btn_01` | `CloakT1Btn` | TMP_Text |
| `Text` | `Txt_ArmorT1Btn_02` | `ArmorT1Btn` | TMP_Text |
| `Text` | `Txt_RingT1Btn_03` | `RingT1Btn` | TMP_Text |
| `Text` | `Txt_ArmorT3Btn_04` | `ArmorT3Btn` | TMP_Text |
| `Text` | `Txt_WeaponT3Btn_05` | `WeaponT3Btn` | TMP_Text |
| `Text` | `Txt_RingT2Btn_06` | `RingT2Btn` | TMP_Text |
| `Text` | `Txt_GloveT2Btn_07` | `GloveT2Btn` | TMP_Text |
| `Text` | `Txt_RingT3Btn_08` | `RingT3Btn` | TMP_Text |
| `Text` | `Txt_BootsT3Btn_09` | `BootsT3Btn` | TMP_Text |
| `Text` | `Txt_WeaponT1Btn_10` | `WeaponT1Btn` | TMP_Text |
| `Text` | `Txt_ArmorT2Btn_11` | `ArmorT2Btn` | TMP_Text |
| `Text` | `Txt_CloakT2Btn_12` | `CloakT2Btn` | TMP_Text |
| `Text` | `Txt_BootsT2Btn_13` | `BootsT2Btn` | TMP_Text |
| `Text` | `Txt_GloveT3Btn_14` | `GloveT3Btn` | TMP_Text |
| `Text` | `Txt_WeaponT2Btn_15` | `WeaponT2Btn` | TMP_Text |
| `Text` | `Txt_CloakT3Btn_16` | `CloakT3Btn` | TMP_Text |
| `Text` | `Txt_WeaponT1Btn_17` | `WeaponT1Btn` | TMP_Text |
| `Text` | `Txt_BootsT1Btn_18` | `BootsT1Btn` | TMP_Text |
| `Text` | `Txt_WeaponT3Btn_19` | `WeaponT3Btn` | TMP_Text |
| `Text` | `Txt_WeaponT2Btn_20` | `WeaponT2Btn` | TMP_Text |
| `Text` | `Txt_GloveT1Btn_21` | `GloveT1Btn` | TMP_Text |
| `Text` | `Txt_EIcon_22` | `EIcon` | TMP_Text |
| `Text` | `Txt_EIcon_23` | `EIcon` | TMP_Text |
| `Text` | `Txt_EIcon_24` | `EIcon` | TMP_Text |
| `Text` | `Txt_EIcon_25` | `EIcon` | TMP_Text |
| `Text` | `Txt_EIcon_26` | `EIcon` | TMP_Text |
| `Text` | `Txt_EIcon_27` | `EIcon` | TMP_Text |
| `Text` | `Txt_EIcon_28` | `EIcon` | TMP_Text |
| `Text` | `Txt_EIcon_29` | `EIcon` | TMP_Text |
| `Text` | `Txt_EIcon_30` | `EIcon` | TMP_Text |
| `Text` | `Txt_EIcon_31` | `EIcon` | TMP_Text |
| `Viewport` | `Mask_HelmetT1List_01` | `HelmetT1List` | Image,Mask |
| `Viewport` | `Mask_RingT2List_02` | `RingT2List` | Image,Mask |
| `Viewport` | `Mask_BootsT1List_03` | `BootsT1List` | Image,Mask |
| `Viewport` | `Mask_ArmorT1List_04` | `ArmorT1List` | Image,Mask |
| `Viewport` | `Mask_CloakT2List_05` | `CloakT2List` | Image,Mask |
| `Viewport` | `Mask_GloveT1List_06` | `GloveT1List` | Image,Mask |
| `Viewport` | `Mask_WeaponT1List_07` | `WeaponT1List` | Image,Mask |
| `Viewport` | `Mask_HelmetT3List_08` | `HelmetT3List` | Image,Mask |
| `Viewport` | `Mask_GloveT2List_09` | `GloveT2List` | Image,Mask |
| `Viewport` | `Mask_ArmorT3List_10` | `ArmorT3List` | Image,Mask |
| `Viewport` | `Mask_CloakT3List_11` | `CloakT3List` | Image,Mask |
| `Viewport` | `Mask_GloveT3List_12` | `GloveT3List` | Image,Mask |
| `Viewport` | `Mask_RingT1List_13` | `RingT1List` | Image,Mask |
| `Viewport` | `Mask_BootsT2List_14` | `BootsT2List` | Image,Mask |
| `Viewport` | `Mask_BootsT3List_15` | `BootsT3List` | Image,Mask |
| `Viewport` | `Mask_RingT3List_16` | `RingT3List` | Image,Mask |
| `Viewport` | `Mask_ArmorT2List_17` | `ArmorT2List` | Image,Mask |
| `Viewport` | `Mask_WeaponT3List_18` | `WeaponT3List` | Image,Mask |
| `Viewport` | `Mask_CloakT1List_19` | `CloakT1List` | Image,Mask |
| `Viewport` | `Mask_WeaponT2List_20` | `WeaponT2List` | Image,Mask |
| `Viewport` | `Mask_HelmetT2List_21` | `HelmetT2List` | Image,Mask |
| `WIcon` | `Img_G00Btn_01` | `G00Btn` | Image,Animator |
| `WIcon` | `Img_C00Btn_02` | `C00Btn` | Image,Animator |
| `WIcon` | `Img_W02Btn_03` | `W02Btn` | Image,Animator |
| `WIcon` | `Img_A00Btn_04` | `A00Btn` | Image,Animator |
| `WIcon` | `Img_W00Btn_05` | `W00Btn` | Image,Animator |
| `WIcon` | `Img_R00Btn_06` | `R00Btn` | Image,Animator |
| `WIcon` | `Img_B00Btn_07` | `B00Btn` | Image,Animator |
| `WIcon` | `Img_W01Btn_08` | `W01Btn` | Image,Animator |
| `WIcon` | `Img_H00Btn_09` | `H00Btn` | Image,Animator |
| `WIcon` | `Img_W44Btn_10` | `W44Btn` | Image,Animator |
| `WeaponT1Btn` | `Btn_HelmetTierFrame_01` | `HelmetTierFrame` | Button |
| `WeaponT1Btn` | `Btn_WeaponTierFrame_02` | `WeaponTierFrame` | Button |
| `WeaponT2Btn` | `Btn_HelmetTierFrame_03` | `HelmetTierFrame` | Button |
| `WeaponT2Btn` | `Btn_WeaponTierFrame_04` | `WeaponTierFrame` | Button |
| `WeaponT3Btn` | `Btn_WeaponTierFrame_01` | `WeaponTierFrame` | Button |
| `WeaponT3Btn` | `Btn_HelmetTierFrame_02` | `HelmetTierFrame` | Button |

## `Assets/@Resources/Prefab/UI/Popup/UI_ForgePopup.prefab`

- 바인드 스크립트: 없음 (enum 보호 대상 없음)
- 변경된 오브젝트: 239개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `AmountTxt` | `Txt_BMORBox_01` | `BMORBox` | TMP_Text |
| `AmountTxt` | `Txt_MORBox_02` | `MORBox` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox2_03` | `ReqItemBox2` | TMP_Text |
| `AmountTxt` | `Txt_Scroll1Box_04` | `Scroll1Box` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox1_05` | `ReqItemBox1` | TMP_Text |
| `AmountTxt` | `Txt_SOM3Box_06` | `SOM3Box` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox4_07` | `ReqItemBox4` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox6_08` | `ReqItemBox6` | TMP_Text |
| `AmountTxt` | `Txt_Scroll3Box_09` | `Scroll3Box` | TMP_Text |
| `AmountTxt` | `Txt_SOM1Box_10` | `SOM1Box` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox3_11` | `ReqItemBox3` | TMP_Text |
| `AmountTxt` | `Txt_SOM2Box_12` | `SOM2Box` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox5_13` | `ReqItemBox5` | TMP_Text |
| `AmountTxt` | `Txt_Scroll4Box_14` | `Scroll4Box` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox8_15` | `ReqItemBox8` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox7_16` | `ReqItemBox7` | TMP_Text |
| `AmountTxt` | `Txt_Scroll2Box_17` | `Scroll2Box` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox3_18` | `ReqItemBox3` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox2_19` | `ReqItemBox2` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox7_20` | `ReqItemBox7` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox1_21` | `ReqItemBox1` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox8_22` | `ReqItemBox8` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox6_23` | `ReqItemBox6` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox5_24` | `ReqItemBox5` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox4_25` | `ReqItemBox4` | TMP_Text |
| `BlockImg` | `Img_ForgePopupIcon_01` | `ForgePopupIcon` | Image |
| `BlockImg` | `Img_BMORBox_02` | `BMORBox` | Image |
| `BlockImg` | `Img_MORBox_03` | `MORBox` | Image |
| `BlockImg` | `Img_SOM1Box_04` | `SOM1Box` | Image |
| `BlockImg` | `Img_ReqItemBox8_05` | `ReqItemBox8` | Image |
| `BlockImg` | `Img_Scroll4Box_06` | `Scroll4Box` | Image |
| `BlockImg` | `Img_Scroll1Box_07` | `Scroll1Box` | Image |
| `BlockImg` | `Img_ReqItemBox5_08` | `ReqItemBox5` | Image |
| `BlockImg` | `Img_ReqItemBox3_09` | `ReqItemBox3` | Image |
| `BlockImg` | `Img_ReqItemBox7_10` | `ReqItemBox7` | Image |
| `BlockImg` | `Img_ReqItemBox1_11` | `ReqItemBox1` | Image |
| `BlockImg` | `Img_Scroll3Box_12` | `Scroll3Box` | Image |
| `BlockImg` | `Img_ReqItemBox6_13` | `ReqItemBox6` | Image |
| `BlockImg` | `Img_ReqItemBox4_14` | `ReqItemBox4` | Image |
| `BlockImg` | `Img_ReqItemBox2_17` | `ReqItemBox2` | Image |
| `BlockImg` | `Img_Scroll2Box_16` | `Scroll2Box` | Image |
| `BlockImg` | `Img_SOM3Box_17` | `SOM3Box` | Image |
| `BlockImg` | `Img_SOM2Box_18` | `SOM2Box` | Image |
| `BlockImg` | `Img_ReqItemBox4_19` | `ReqItemBox4` | Image |
| `BlockImg` | `Img_ReqItemBox5_20` | `ReqItemBox5` | Image |
| `BlockImg` | `Img_ReqItemBox1_23` | `ReqItemBox1` | Image |
| `BlockImg` | `Img_ReqItemBox3_22` | `ReqItemBox3` | Image |
| `BlockImg` | `Img_ReqItemBox2_23` | `ReqItemBox2` | Image |
| `BlockImg` | `Img_ReqItemBox8_24` | `ReqItemBox8` | Image |
| `BlockImg` | `Img_ReqItemBox7_25` | `ReqItemBox7` | Image |
| `BlockImg` | `Img_ReqItemBox6_26` | `ReqItemBox6` | Image |
| `BtmImg` | `Img_DoEnhaceBtn_01` | `DoEnhaceBtn` | Image |
| `BtmImg` | `Img_DoEnhaceBtn_02` | `DoEnhaceBtn` | Image |
| `BtnGroup` | `Img_EnhancePart_01` | `EnhancePart` | Image |
| `BtnGroup` | `Img_RestorePart_02` | `RestorePart` | Image |
| `CantEnhaceCraft` | `Img_BtnGroup_01` | `BtnGroup` | Image |
| `CantEnhaceCraft` | `Img_BtnGroup_02` | `BtnGroup` | Image |
| `Deco` | `Img_Title_1_01` | `Title_1` | Image |
| `Deco` | `Img_BtnPanel_02` | `BtnPanel` | Image |
| `Deco` | `Img_Title_03` | `Title` | Image |
| `Deco` | `Img_BtnGroup_04` | `BtnGroup` | Image |
| `Deco` | `Img_Title_1_05` | `Title_1` | Image |
| `Deco` | `Img_BtnGroup_06` | `BtnGroup` | Image |
| `Deco_1` | `Img_Title_1_03` | `Title_1` | Image |
| `Deco_1` | `Img_Title_02` | `Title` | Image |
| `Deco_1` | `Img_Title_1_06` | `Title_1` | Image |
| `DoEnhaceBtn` | `Btn_BtnGroup_01` | `BtnGroup` | Image,Button |
| `DoEnhaceBtn` | `Btn_BtnGroup_02` | `BtnGroup` | Image,Button |
| `Frame` | `Img_ForgePopupIcon_03` | `ForgePopupIcon` | Image |
| `Frame` | `Img_ForgePanel_02` | `ForgePanel` | Image |
| `Frame` | `Img_BMORBox_03` | `BMORBox` | Image |
| `Frame` | `Img_MORBox_04` | `MORBox` | Image |
| `Frame` | `Img_PotenBox_6_05` | `PotenBox_6` | Image |
| `Frame` | `Img_ReqItemBox3_06` | `ReqItemBox3` | Image |
| `Frame` | `Img_ReqItemBox8_07` | `ReqItemBox8` | Image |
| `Frame` | `Img_SOM3Box_08` | `SOM3Box` | Image |
| `Frame` | `Img_ReqItemBox6_09` | `ReqItemBox6` | Image |
| `Frame` | `Img_Scroll2Box_10` | `Scroll2Box` | Image |
| `Frame` | `Img_Scroll4Box_11` | `Scroll4Box` | Image |
| `Frame` | `Img_ReqItemBox4_12` | `ReqItemBox4` | Image |
| `Frame` | `Img_SOM2Box_13` | `SOM2Box` | Image |
| `Frame` | `Img_ReqItemBox1_14` | `ReqItemBox1` | Image |
| `Frame` | `Img_Scroll3Box_15` | `Scroll3Box` | Image |
| `Frame` | `Img_PotenBox_5_16` | `PotenBox_5` | Image |
| `Frame` | `Img_Scroll1Box_17` | `Scroll1Box` | Image |
| `Frame` | `Img_ReqItemBox7_20` | `ReqItemBox7` | Image |
| `Frame` | `Img_SOM1Box_19` | `SOM1Box` | Image |
| `Frame` | `Img_ReqItemBox5_22` | `ReqItemBox5` | Image |
| `Frame` | `Img_PotenBox_4_21` | `PotenBox_4` | Image |
| `Frame` | `Img_ReqItemBox2_22` | `ReqItemBox2` | Image |
| `Frame` | `Img_PotenBox_7_23` | `PotenBox_7` | Image |
| `Frame` | `Img_ReqItemBox3_26` | `ReqItemBox3` | Image |
| `Frame` | `Img_ReqItemBox4_25` | `ReqItemBox4` | Image |
| `Frame` | `Img_ReqItemBox5_26` | `ReqItemBox5` | Image |
| `Frame` | `Img_ReqItemBox1_27` | `ReqItemBox1` | Image |
| `Frame` | `Img_ReqItemBox8_28` | `ReqItemBox8` | Image |
| `Frame` | `Img_ReqItemBox2_29` | `ReqItemBox2` | Image |
| `Frame` | `Img_ReqItemBox6_30` | `ReqItemBox6` | Image |
| `Frame` | `Img_ReqItemBox7_31` | `ReqItemBox7` | Image |
| `GlowMask` | `Mask_BMORBox_01` | `BMORBox` | Image,Mask |
| `GlowMask` | `Mask_MORBox_02` | `MORBox` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox5_03` | `ReqItemBox5` | Image,Mask |
| `GlowMask` | `Mask_SOM1Box_04` | `SOM1Box` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox2_05` | `ReqItemBox2` | Image,Mask |
| `GlowMask` | `Mask_Scroll2Box_06` | `Scroll2Box` | Image,Mask |
| `GlowMask` | `Mask_Scroll3Box_07` | `Scroll3Box` | Image,Mask |
| `GlowMask` | `Mask_Scroll1Box_08` | `Scroll1Box` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox8_09` | `ReqItemBox8` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox6_10` | `ReqItemBox6` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox1_11` | `ReqItemBox1` | Image,Mask |
| `GlowMask` | `Mask_SOM2Box_12` | `SOM2Box` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox7_13` | `ReqItemBox7` | Image,Mask |
| `GlowMask` | `Mask_Scroll4Box_14` | `Scroll4Box` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox4_15` | `ReqItemBox4` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox3_16` | `ReqItemBox3` | Image,Mask |
| `GlowMask` | `Mask_SOM3Box_17` | `SOM3Box` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox6_18` | `ReqItemBox6` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox8_19` | `ReqItemBox8` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox1_20` | `ReqItemBox1` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox4_21` | `ReqItemBox4` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox5_22` | `ReqItemBox5` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox7_23` | `ReqItemBox7` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox2_24` | `ReqItemBox2` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox3_25` | `ReqItemBox3` | Image,Mask |
| `Image` | `Img_DuriOverRecovPopup_01` | `DuriOverRecovPopup` | Image |
| `Image` | `Img_Image_02` | `Image` | Image |
| `Image` | `Img_Mask_03` | `Mask` | Image |
| `Image` | `Mask_Title_1_04` | `Title_1` | Image,Mask |
| `Image` | `Mask_Title_1_05` | `Title_1` | Image,Mask |
| `Image` | `Mask_Title_06` | `Title` | Image,Mask |
| `Image` | `Img_Image_07` | `Image` | Image |
| `Image` | `Img_Image_08` | `Image` | Image |
| `Image` | `Img_Image_09` | `Image` | Image |
| `Image` | `Img_mask_2_10` | `mask_2` | Image |
| `Image` | `Img_mask_2_11` | `mask_2` | Image |
| `Image` | `Img_mask_2_12` | `mask_2` | Image |
| `Image` | `Img_mask_2_13` | `mask_2` | Image |
| `Image_1` | `Img_Mask_01` | `Mask` | Image |
| `Image_1` | `Img_Glow_02` | `Glow` | Image |
| `Image_1` | `Img_Item_03` | `_` | Image |
| `Image_2` | `Img_Glow_01` | `Glow` | Image |
| `Image_2` | `Img_Item_02` | `_` | Image |
| `Img` | `Img_BMORBox_01` | `BMORBox` | Image |
| `Img` | `Img_MORBox_02` | `MORBox` | Image |
| `Img` | `Img_ReqItemBox3_03` | `ReqItemBox3` | Image |
| `Img` | `Img_ReqItemBox1_04` | `ReqItemBox1` | Image |
| `Img` | `Img_ReqItemBox7_05` | `ReqItemBox7` | Image |
| `Img` | `Img_Scroll1Box_06` | `Scroll1Box` | Image |
| `Img` | `Img_Scroll4Box_07` | `Scroll4Box` | Image |
| `Img` | `Img_ReqItemBox6_08` | `ReqItemBox6` | Image |
| `Img` | `Img_SOM2Box_09` | `SOM2Box` | Image |
| `Img` | `Img_ReqItemBox8_10` | `ReqItemBox8` | Image |
| `Img` | `Img_Scroll3Box_11` | `Scroll3Box` | Image |
| `Img` | `Img_SOM1Box_12` | `SOM1Box` | Image |
| `Img` | `Img_SOM3Box_13` | `SOM3Box` | Image |
| `Img` | `Img_Scroll2Box_14` | `Scroll2Box` | Image |
| `Img` | `Img_ReqItemBox2_15` | `ReqItemBox2` | Image |
| `Img` | `Img_ReqItemBox4_16` | `ReqItemBox4` | Image |
| `Img` | `Img_ReqItemBox5_17` | `ReqItemBox5` | Image |
| `Img` | `Img_ReqItemBox7_18` | `ReqItemBox7` | Image |
| `Img` | `Img_ReqItemBox8_19` | `ReqItemBox8` | Image |
| `Img` | `Img_ReqItemBox2_20` | `ReqItemBox2` | Image |
| `Img` | `Img_ReqItemBox1_21` | `ReqItemBox1` | Image |
| `Img` | `Img_ReqItemBox4_22` | `ReqItemBox4` | Image |
| `Img` | `Img_ReqItemBox5_23` | `ReqItemBox5` | Image |
| `Img` | `Img_ReqItemBox3_24` | `ReqItemBox3` | Image |
| `Img` | `Img_ReqItemBox6_25` | `ReqItemBox6` | Image |
| `Lock` | `Img_Frame_01` | `Frame` | Image |
| `Lock` | `Img_Frame_02` | `Frame` | Image |
| `Lock` | `Img_Frame_03` | `Frame` | Image |
| `Lock` | `Img_Frame_04` | `Frame` | Image |
| `ReqItemBox1` | `Btn_Panel1_05` | `Panel1` | Image,Button |
| `ReqItemBox1` | `Btn_Content_10` | `Content` | Image,Button |
| `ReqItemBox2` | `Btn_Panel1_03` | `Panel1` | Image,Button |
| `ReqItemBox2` | `Btn_Content_05` | `Content` | Image,Button |
| `ReqItemBox3` | `Btn_Panel2_05` | `Panel2` | Image,Button |
| `ReqItemBox3` | `Btn_Content_09` | `Content` | Image,Button |
| `ReqItemBox4` | `Btn_Panel2_01` | `Panel2` | Image,Button |
| `ReqItemBox4` | `Btn_Content_04` | `Content` | Image,Button |
| `ReqItemBox5` | `Btn_Panel1_04` | `Panel1` | Image,Button |
| `ReqItemBox5` | `Btn_Content_06` | `Content` | Image,Button |
| `ReqItemBox6` | `Btn_Panel2_03` | `Panel2` | Image,Button |
| `ReqItemBox6` | `Btn_Content_07` | `Content` | Image,Button |
| `ReqItemBox7` | `Btn_Panel1_01` | `Panel1` | Image,Button |
| `ReqItemBox7` | `Btn_Content_02` | `Content` | Image,Button |
| `ReqItemBox8` | `Btn_Panel2_04` | `Panel2` | Image,Button |
| `ReqItemBox8` | `Btn_Content_08` | `Content` | Image,Button |
| `Text` | `Txt_Image_01` | `Image` | TMP_Text |
| `Text` | `Txt_EIcon_02` | `EIcon` | TMP_Text |
| `Text` | `Txt_Frame_03` | `Frame` | TMP_Text |
| `Text` | `Txt_Frame_04` | `Frame` | TMP_Text |
| `Text` | `Txt_Frame_05` | `Frame` | TMP_Text |
| `Text` | `Txt_Frame_06` | `Frame` | TMP_Text |
| `Text_Legacy_1` | `Txt_Image_04` | `Image` | TMP_Text |
| `Text_Legacy_1` | `Txt_ForceEnhTxtGroup_02` | `ForceEnhTxtGroup` | TMP_Text |
| `Text_Legacy_2` | `Txt_ForceUseBtn_01` | `ForceUseBtn` | TMP_Text |
| `Text_Legacy_2` | `Txt_ForceEnhTxtGroup_04` | `ForceEnhTxtGroup` | TMP_Text |
| `Text_Legacy_3` | `Txt_Image_03` | `Image` | TMP_Text |
| `Text_Legacy_3` | `Txt_EnhancePart_02` | `EnhancePart` | TMP_Text |
| `Title_1` | `Panel_CraftingPanel_01` | `CraftingPanel` | — |
| `Title_1` | `Panel_EnhancePart_02` | `EnhancePart` | — |
| `_` | `Obj_ForgePopupFrame_01` | `ForgePopupFrame` | — |
| `_` | `Txt_Title_1_02` | `Title_1` | TMP_Text |
| `_` | `Mask_CraftBtn_03` | `CraftBtn` | Image,Mask |
| `_` | `Txt_Title_04` | `Title` | TMP_Text |
| `_` | `Txt_Title_1_05` | `Title_1` | TMP_Text |
| `_` | `Txt_CantCraft_06` | `CantCraft` | TMP_Text |
| `_` | `Txt_CraftBtn_07` | `CraftBtn` | TMP_Text |
| `_` | `Img_GlowMask_08` | `GlowMask` | Image |
| `_` | `Txt_CantEnhaceCraft_09` | `CantEnhaceCraft` | TMP_Text |
| `_` | `Txt_DoEnhaceBtn_10` | `DoEnhaceBtn` | TMP_Text |
| `_` | `Txt_DoEnhaceBtn_11` | `DoEnhaceBtn` | TMP_Text |
| `_` | `Img_GlowMask_12` | `GlowMask` | Image |
| `_` | `Txt_CantEnhaceCraft_13` | `CantEnhaceCraft` | TMP_Text |
| `_` | `Img_GlowMask_14` | `GlowMask` | Image |
| `_` | `Img_GlowMask_15` | `GlowMask` | Image |
| `_` | `Img_GlowMask_16` | `GlowMask` | Image |
| `_` | `Img_GlowMask_17` | `GlowMask` | Image |
| `_` | `Img_GlowMask_18` | `GlowMask` | Image |
| `_` | `Img_GlowMask_19` | `GlowMask` | Image |
| `_` | `Img_GlowMask_20` | `GlowMask` | Image |
| `_` | `Img_GlowMask_21` | `GlowMask` | Image |
| `_` | `Img_GlowMask_22` | `GlowMask` | Image |
| `_` | `Img_GlowMask_23` | `GlowMask` | Image |
| `_` | `Img_GlowMask_24` | `GlowMask` | Image |
| `_` | `Img_GlowMask_25` | `GlowMask` | Image |
| `_` | `Img_GlowMask_26` | `GlowMask` | Image |
| `_` | `Img_GlowMask_27` | `GlowMask` | Image |
| `_` | `Img_GlowMask_28` | `GlowMask` | Image |
| `_` | `Img_GlowMask_29` | `GlowMask` | Image |
| `_` | `Img_GlowMask_30` | `GlowMask` | Image |
| `_` | `Img_GlowMask_31` | `GlowMask` | Image |
| `_` | `Img_GlowMask_32` | `GlowMask` | Image |
| `_` | `Img_GlowMask_33` | `GlowMask` | Image |
| `_` | `Img_GlowMask_34` | `GlowMask` | Image |
| `_` | `Img_GlowMask_35` | `GlowMask` | Image |
| `_` | `Img_GlowMask_36` | `GlowMask` | Image |
| `mask_2` | `Mask_DoEnhaceBtn_01` | `DoEnhaceBtn` | Image,Mask |
| `mask_2` | `Mask_DoEnhaceBtn_02` | `DoEnhaceBtn` | Image,Mask |

## `Assets/@Resources/Prefab/UI/Popup/UI_InventoryPopup.prefab`

- 바인드 스크립트: `Assets/@Scripts/UI/Popup/UI_InventoryPopup.cs` (enum 멤버 24개 보호)
- 변경된 오브젝트: 4개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `Frame` | `Img_InventoryList_01` | `InventoryList` | Image |
| `Frame` | `Img_GameObject_02` | `GameObject` | Image |
| `GameObject` | `Txt_Xbutton_01` | `Xbutton` | TMP_Text |
| `GameObject` | `Mask_ImportantBagBtn_02` | `ImportantBagBtn` | Image,Mask |

## `Assets/@Resources/Prefab/UI/Popup/UI_ItemPopup.prefab`

- 바인드 스크립트: `Assets/@Scripts/UI/Popup/UI_ItemPopup.cs` (enum 멤버 45개 보호)
- 변경된 오브젝트: 15개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `Frame` | `Img_ItemPopupImgGroup_01` | `ItemPopupImgGroup` | Image |
| `Frame` | `Img_ItemPopupTitle_02` | `ItemPopupTitle` | Image |
| `Image` | `Img_Mask_01` | `Mask` | Image |
| `Image` | `Img_Mask_02` | `Mask` | Image |
| `Mask` | `Mask_ItemPopupTitle_01` | `ItemPopupTitle` | Image,Mask |
| `Mask` | `Mask_ItemPopupImgGroup_02` | `ItemPopupImgGroup` | Image,Mask |
| `_` | `Img_ItemPopupFrame_01` | `ItemPopupFrame` | Image |
| `_` | `Img_dot_1_02` | `dot_1` | Image |
| `_` | `Img_dot_03` | `dot` | Image |
| `_` | `Txt_MORUseBtn_04` | `MORUseBtn` | TMP_Text |
| `_` | `Txt_MORNotHaveBtn_05` | `MORNotHaveBtn` | TMP_Text |
| `_` | `Txt_ScrollUseBtn_06` | `ScrollUseBtn` | TMP_Text |
| `_` | `Txt_ScrollNotHaveBtn_07` | `ScrollNotHaveBtn` | TMP_Text |
| `_` | `Txt_SOMNotHaveBtn_08` | `SOMNotHaveBtn` | TMP_Text |
| `_` | `Txt_SOMUseBtn_09` | `SOMUseBtn` | TMP_Text |

## `Assets/@Resources/Prefab/UI/Popup/UI_MapSelectPopup.prefab`

- 바인드 스크립트: `Assets/@Scripts/UI/Popup/UI_MapSelectPopup.cs` (enum 멤버 0개 보호)
- 변경된 오브젝트: 25개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `Handle` | `Img_SlidingArea_01` | `SlidingArea` | Image |
| `Handle` | `Img_SlidingArea_02` | `SlidingArea` | Image |
| `Handle` | `Img_SlidingArea_03` | `SlidingArea` | Image |
| `Image` | `Img_MapSelectAsdalBtn_01` | `MapSelectAsdalBtn` | Image |
| `Image` | `Img_Image_02` | `Image` | Image |
| `QuestMarker` | `Img_to_FieldQuestVFX_01` | `to_FieldQuestVFX` | Image |
| `QuestMarker` | `Img_to_TownButtonQuestVFX_02` | `to_TownButtonQuestVFX` | Image |
| `QuestMarker` | `Img_ButtontoBWFVFX_03` | `ButtontoBWFVFX` | Image |
| `QuestMarker` | `Img_AsdalButtonQuestVFX_04` | `AsdalButtonQuestVFX` | Image |
| `QuestMarker` | `Img_MapSelectBWF_EntryVFX_05` | `MapSelectBWF_EntryVFX` | Image |
| `ScrollView` | `Scr_FieldType_01` | `FieldType` | Image,ScrollRect |
| `ScrollView` | `Scr_TownType_02` | `TownType` | Image,ScrollRect |
| `ScrollView` | `Scr_BWFTypeFrame_03` | `BWFTypeFrame` | Image,ScrollRect |
| `ScrollbarVertical` | `Sb_ScrollView_01` | `ScrollView` | Image,Scrollbar |
| `ScrollbarVertical` | `Sb_ScrollView_02` | `ScrollView` | Image,Scrollbar |
| `ScrollbarVertical` | `Sb_ScrollView_03` | `ScrollView` | Image,Scrollbar |
| `SlidingArea` | `Panel_ScrollbarVertical_01` | `ScrollbarVertical` | — |
| `SlidingArea` | `Panel_ScrollbarVertical_02` | `ScrollbarVertical` | — |
| `SlidingArea` | `Panel_ScrollbarVertical_03` | `ScrollbarVertical` | — |
| `TitleText` | `Img_TownType_01` | `TownType` | Image |
| `TitleText` | `Img_FieldType_02` | `FieldType` | Image |
| `TitleText` | `Txt_TitleTextFrame_03` | `TitleTextFrame` | TMP_Text |
| `Viewport` | `Mask_ScrollView_01` | `ScrollView` | Image,Mask |
| `Viewport` | `Mask_ScrollView_02` | `ScrollView` | Image,Mask |
| `Viewport` | `Mask_ScrollView_03` | `ScrollView` | Image,Mask |

## `Assets/@Resources/Prefab/UI/Popup/UI_MinionInfoPopup.prefab`

- 바인드 스크립트: 없음 (enum 보호 대상 없음)
- 변경된 오브젝트: 40개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `BtmImg_1` | `Img_MinionSummonedBtn_01` | `MinionSummonedBtn` | Image |
| `BtmImg_1` | `Img_MinionSummonBtn_02` | `MinionSummonBtn` | Image |
| `BtmImg_1` | `Img_MinionCantSummonBtn_03` | `MinionCantSummonBtn` | Image |
| `Dust_01` | `VFX_SYNC_GlowDot_01` | `SYNC_GlowDot` | ParticleSystem |
| `Dust_01` | `VFX_Star_GlowDot_02` | `Star_GlowDot` | ParticleSystem |
| `Frame` | `Panel_MinionInfoPopup_01` | `MinionInfoPopup` | — |
| `Frame` | `Img_MinionUsingSkill_02` | `MinionUsingSkill` | Image |
| `GameObject` | `Txt_Xbutton_01` | `Xbutton` | TMP_Text |
| `GameObject` | `Txt_MinionNotUseSkill_02` | `MinionNotUseSkill` | TMP_Text |
| `GameObject` | `Txt_BlockImg_03` | `BlockImg` | TMP_Text |
| `GlowDot_01_End` | `VFX_MinionInfoPopupASUpgrade_01` | `MinionInfoPopupASUpgradeSFX` | ParticleSystem |
| `GlowDot_01_End` | `VFX_MinionInfoPopupDmgUpgrad_02` | `MinionInfoPopupDmgUpgradeSFX` | ParticleSystem |
| `GlowDot_01_End` | `VFX_MinionInfoPopupCritUpgra_03` | `MinionInfoPopupCritUpgradeSFX` | ParticleSystem |
| `GlowDot_01_Start` | `VFX_MinionInfoPopupASUpgrade_04` | `MinionInfoPopupASUpgradeSFX` | ParticleSystem |
| `GlowDot_01_Start` | `VFX_MinionInfoPopupDmgUpgrad_05` | `MinionInfoPopupDmgUpgradeSFX` | ParticleSystem |
| `GlowDot_01_Start` | `VFX_MinionInfoPopupCritUpgra_07` | `MinionInfoPopupCritUpgradeSFX` | ParticleSystem |
| `GlowDot_02_End` | `VFX_MinionInfoPopupDmgUpgrad_01` | `MinionInfoPopupDmgUpgradeSFX` | ParticleSystem |
| `GlowDot_02_End` | `VFX_MinionInfoPopupASUpgrade_02` | `MinionInfoPopupASUpgradeSFX` | ParticleSystem |
| `GlowDot_02_End` | `VFX_MinionInfoPopupCritUpgra_05` | `MinionInfoPopupCritUpgradeSFX` | ParticleSystem |
| `GlowDot_02_End_Light` | `VFX_GlowDot_02_End_01` | `GlowDot_02_End` | ParticleSystem |
| `GlowDot_02_End_Light` | `VFX_GlowDot_02_End_02` | `GlowDot_02_End` | ParticleSystem |
| `GlowDot_02_End_Light` | `VFX_GlowDot_02_End_03` | `GlowDot_02_End` | ParticleSystem |
| `GlowDot_02_Start` | `VFX_MinionInfoPopupASUpgrade_03` | `MinionInfoPopupASUpgradeSFX` | ParticleSystem |
| `GlowDot_02_Start` | `VFX_MinionInfoPopupDmgUpgrad_04` | `MinionInfoPopupDmgUpgradeSFX` | ParticleSystem |
| `GlowDot_02_Start` | `VFX_MinionInfoPopupCritUpgra_06` | `MinionInfoPopupCritUpgradeSFX` | ParticleSystem |
| `Image_1` | `Img_MinionInfoPopupNameFrame_01` | `MinionInfoPopupNameFrame` | Image |
| `Image_1` | `Img_Item_02` | `_` | Image |
| `Image_1` | `Img_Item_03` | `_` | Image |
| `Image_1` | `Img_Item_04` | `_` | Image |
| `Image_2` | `Img_Item_01` | `_` | Image |
| `Image_2` | `Img_Item_05` | `_` | Image |
| `Image_2` | `Img_Item_06` | `_` | Image |
| `Ring_01` | `VFX_SYNC_GlowDot_03` | `SYNC_GlowDot` | ParticleSystem |
| `Ring_01` | `VFX_Star_GlowDot_04` | `Star_GlowDot` | ParticleSystem |
| `_` | `Mask_MinionCantSummonBtn_01` | `MinionCantSummonBtn` | Image,Mask |
| `_` | `Txt_MinionSummonBtn_02` | `MinionSummonBtn` | TMP_Text |
| `_` | `Mask_MinionSummonedBtn_03` | `MinionSummonedBtn` | Image,Mask |
| `_` | `Mask_MinionSummonBtn_04` | `MinionSummonBtn` | Image,Mask |
| `_` | `Txt_MinionSummonedBtn_05` | `MinionSummonedBtn` | TMP_Text |
| `_` | `Txt_MinionCantSummonBtn_06` | `MinionCantSummonBtn` | TMP_Text |

## `Assets/@Resources/Prefab/UI/Popup/UI_MonsterDetailPopup.prefab`

- 바인드 스크립트: 없음 (enum 보호 대상 없음)
- 변경된 오브젝트: 81개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `AmountTxt` | `Txt_MonsterMoneyDropListBox_01` | `MonsterMoneyDropListBox` | TMP_Text |
| `AmountTxt` | `Txt_MonsterMoneyDropListBox__02` | `MonsterMoneyDropListBox_1` | TMP_Text |
| `BlockImg` | `Txt_GameObject_01` | `GameObject` | TMP_Text |
| `BlockImg` | `Txt_GameObject_02` | `GameObject` | TMP_Text |
| `BlockImg` | `Txt_GameObject_03` | `GameObject` | TMP_Text |
| `BlockImg` | `Txt_GameObject_04` | `GameObject` | TMP_Text |
| `BlockImg` | `Txt_GameObject_05` | `GameObject` | TMP_Text |
| `BlockImg` | `Txt_GameObject_06` | `GameObject` | TMP_Text |
| `BlockImg` | `Txt_GameObject_07` | `GameObject` | TMP_Text |
| `BlockImg` | `Txt_GameObject_08` | `GameObject` | TMP_Text |
| `BlockImg` | `Txt_GameObject_09` | `GameObject` | TMP_Text |
| `BlockImg` | `Txt_GameObject_10` | `GameObject` | TMP_Text |
| `ChanceTxt` | `Txt_MonsterMoneyDropListBox__01` | `MonsterMoneyDropListBox_1` | TMP_Text |
| `ChanceTxt` | `Txt_MonsterItemDropListBox_7_02` | `MonsterItemDropListBox_7` | TMP_Text |
| `ChanceTxt` | `Txt_MonsterItemDropListBox_03` | `MonsterItemDropListBox` | TMP_Text |
| `ChanceTxt` | `Txt_MonsterItemDropListBox_2_04` | `MonsterItemDropListBox_2` | TMP_Text |
| `ChanceTxt` | `Txt_MonsterItemDropListBox_4_05` | `MonsterItemDropListBox_4` | TMP_Text |
| `ChanceTxt` | `Txt_MonsterItemDropListBox_6_06` | `MonsterItemDropListBox_6` | TMP_Text |
| `ChanceTxt` | `Txt_MonsterItemDropListBox_1_07` | `MonsterItemDropListBox_1` | TMP_Text |
| `ChanceTxt` | `Txt_MonsterItemDropListBox_3_08` | `MonsterItemDropListBox_3` | TMP_Text |
| `ChanceTxt` | `Txt_MonsterMoneyDropListBox_09` | `MonsterMoneyDropListBox` | TMP_Text |
| `ChanceTxt` | `Txt_MonsterItemDropListBox_5_10` | `MonsterItemDropListBox_5` | TMP_Text |
| `Frame` | `Img_MonsterDeatilPopupMonste_01` | `MonsterDeatilPopupMonsterSkill` | Image |
| `Frame` | `Img_GameObject_21` | `GameObject` | Image |
| `Frame` | `Img_GameObject_22` | `GameObject` | Image |
| `Frame` | `Img_GameObject_23` | `GameObject` | Image |
| `Frame` | `Img_GameObject_24` | `GameObject` | Image |
| `Frame` | `Img_GameObject_25` | `GameObject` | Image |
| `Frame` | `Img_GameObject_26` | `GameObject` | Image |
| `Frame` | `Img_GameObject_27` | `GameObject` | Image |
| `Frame` | `Img_GameObject_28` | `GameObject` | Image |
| `Frame` | `Img_GameObject_29` | `GameObject` | Image |
| `Frame` | `Img_GameObject_30` | `GameObject` | Image |
| `GameObject` | `Txt_Xbutton_01` | `Xbutton` | TMP_Text |
| `GameObject` | `Panel_MonsterDetailPopupInfoBo_02` | `MonsterDetailPopupInfoBox` | — |
| `GameObject` | `Mask_MonsterMoneyDropListBox_03` | `MonsterMoneyDropListBox` | Image,Mask |
| `GameObject` | `Mask_MonsterItemDropListBox_04` | `MonsterItemDropListBox` | Image,Mask |
| `GameObject` | `Mask_MonsterItemDropListBox_3_05` | `MonsterItemDropListBox_3` | Image,Mask |
| `GameObject` | `Mask_MonsterItemDropListBox_5_06` | `MonsterItemDropListBox_5` | Image,Mask |
| `GameObject` | `Mask_MonsterItemDropListBox_2_07` | `MonsterItemDropListBox_2` | Image,Mask |
| `GameObject` | `Mask_MonsterItemDropListBox_6_08` | `MonsterItemDropListBox_6` | Image,Mask |
| `GameObject` | `Mask_MonsterItemDropListBox_4_09` | `MonsterItemDropListBox_4` | Image,Mask |
| `GameObject` | `Mask_MonsterMoneyDropListBox__10` | `MonsterMoneyDropListBox_1` | Image,Mask |
| `GameObject` | `Mask_MonsterItemDropListBox_7_11` | `MonsterItemDropListBox_7` | Image,Mask |
| `GameObject` | `Mask_MonsterItemDropListBox_1_12` | `MonsterItemDropListBox_1` | Image,Mask |
| `Glow` | `Img_GameObject_11` | `GameObject` | Image |
| `Glow` | `Img_GameObject_12` | `GameObject` | Image |
| `Glow` | `Img_GameObject_13` | `GameObject` | Image |
| `Glow` | `Img_GameObject_14` | `GameObject` | Image |
| `Glow` | `Img_GameObject_15` | `GameObject` | Image |
| `Glow` | `Img_GameObject_16` | `GameObject` | Image |
| `Glow` | `Img_GameObject_17` | `GameObject` | Image |
| `Glow` | `Img_GameObject_18` | `GameObject` | Image |
| `Glow` | `Img_GameObject_19` | `GameObject` | Image |
| `Glow` | `Img_GameObject_20` | `GameObject` | Image |
| `Icon_2` | `Img_ClaimedIcon_01` | `ClaimedIcon` | Image |
| `Icon_2` | `Img_LvlIcon_02` | `LvlIcon` | Image |
| `Image_1` | `Img_Item_03` | `_` | Image |
| `Image_1` | `Img_X_1_04` | `__1` | Image |
| `Image_2` | `Img_Item_01` | `_` | Image |
| `Image_2` | `Img_X_1_02` | `__1` | Image |
| `Img` | `Img_GameObject_01` | `GameObject` | Image |
| `Img` | `Img_GameObject_02` | `GameObject` | Image |
| `Img` | `Img_GameObject_03` | `GameObject` | Image |
| `Img` | `Img_GameObject_04` | `GameObject` | Image |
| `Img` | `Img_GameObject_05` | `GameObject` | Image |
| `Img` | `Img_GameObject_06` | `GameObject` | Image |
| `Img` | `Img_GameObject_07` | `GameObject` | Image |
| `Img` | `Img_GameObject_08` | `GameObject` | Image |
| `Img` | `Img_GameObject_09` | `GameObject` | Image |
| `Img` | `Img_GameObject_10` | `GameObject` | Image |
| `Text` | `Txt_LvlIcon_01` | `LvlIcon` | TMP_Text |
| `Text` | `Txt_ClaimedIcon_02` | `ClaimedIcon` | TMP_Text |
| `Txt` | `Txt_MonsterDetailPopupSkillB_01` | `MonsterDetailPopupSkillBox` | TMP_Text |
| `Txt` | `Txt_MonsterDetailPopupInfoBo_02` | `MonsterDetailPopupInfoBox` | TMP_Text |
| `_` | `Txt_MonsterDetailPopupCheckM_01` | `MonsterDetailPopupCheckMinionBtn` | TMP_Text |
| `_` | `Mask_MonsterDetailPopupCheckM_02` | `MonsterDetailPopupCheckMinionBtn` | Image,Mask |
| `__1` | `Mask_BtmImg_2_01` | `BtmImg_2` | Image,Mask |
| `__1` | `Txt_BtmImg_2_02` | `BtmImg_2` | TMP_Text |
| `text` | `Txt_MonsterDetailPopupSkillB_03` | `MonsterDetailPopupSkillBoxLockGroup` | TMP_Text |
| `text` | `Txt_MonsterDetailPopupSkillB_02` | `MonsterDetailPopupSkillBoxFullLockGroup` | TMP_Text |

## `Assets/@Resources/Prefab/UI/Popup/UI_MOROverUsePopup.prefab`

- 바인드 스크립트: 없음 (enum 보호 대상 없음)
- 변경된 오브젝트: 2개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `Image` | `Img_MOROverUsePopup_01` | `MOROverUsePopup` | Image |
| `Image` | `Img_Image_02` | `Image` | Image |

## `Assets/@Resources/Prefab/UI/Popup/UI_PathPopup0.prefab`

- 바인드 스크립트: 없음 (enum 보호 대상 없음)
- 변경된 오브젝트: 4개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `Image` | `Img_PathPopup_01` | `PathPopup` | Image |
| `Image` | `Img_PathPopup_02` | `PathPopup` | Image |
| `Image_1` | `Img_PathPopup_03` | `PathPopup` | Image |
| `Image_1` | `Img_Image_02` | `Image` | Image |

## `Assets/@Resources/Prefab/UI/Popup/UI_PathPopup1.prefab`

- 바인드 스크립트: 없음 (enum 보호 대상 없음)
- 변경된 오브젝트: 2개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `Image` | `Img_PathPopup1_01` | `PathPopup1` | Image |
| `Image` | `Img_PathPopup1_02` | `PathPopup1` | Image |

## `Assets/@Resources/Prefab/UI/Popup/UI_PathPopup2.prefab`

- 바인드 스크립트: 없음 (enum 보호 대상 없음)
- 변경된 오브젝트: 2개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `Image` | `Img_PathPopup2_01` | `PathPopup2` | Image |
| `Image` | `Img_PathPopup2_02` | `PathPopup2` | Image |

## `Assets/@Resources/Prefab/UI/Popup/UI_PathPopup3.prefab`

- 바인드 스크립트: 없음 (enum 보호 대상 없음)
- 변경된 오브젝트: 51개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `Image` | `Img_Path_Image_Knight_01` | `Path_Image_Knight` | Image |
| `Image` | `Img_mask_04` | `mask` | Image |
| `Image` | `Img_mask_03` | `mask` | Image |
| `Image` | `Img_mask_06` | `mask` | Image |
| `Image` | `Img_Image_4_08` | `Image_4` | Image |
| `Image` | `Img_Image_3_08` | `Image_3` | Image |
| `Image` | `Img_Image_2_07` | `Image_2` | Image |
| `Image0` | `Img_Image_2_01` | `Image_2` | Image |
| `Image0` | `Img_Image_3_02` | `Image_3` | Image |
| `Image0` | `Img_Image_4_05` | `Image_4` | Image |
| `Image_1` | `Img_Image_4_01` | `Image_4` | Image |
| `Image_1` | `Img_Image_2_04` | `Image_2` | Image |
| `Image_1` | `Img_Image_3_05` | `Image_3` | Image |
| `Image_2` | `Img_mask_01` | `mask` | Image |
| `Image_2` | `Img_Image_2_02` | `Image_2` | Image |
| `Image_2` | `Img_Image_4_03` | `Image_4` | Image |
| `Image_2` | `Img_Image_3_04` | `Image_3` | Image |
| `Image_3` | `Img_Path_Image_Assassin_01` | `Path_Image_Assassin` | Image |
| `Image_3` | `Img_mask_02` | `mask` | Image |
| `Image_3` | `Img_Image_3_03` | `Image_3` | Image |
| `Image_3` | `Img_Image_4_04` | `Image_4` | Image |
| `Image_3` | `Img_Image_2_05` | `Image_2` | Image |
| `Image_4` | `Img_Path_Image_BeastLord_01` | `Path_Image_BeastLord` | Image |
| `Image_4` | `Img_mask_05` | `mask` | Image |
| `Image_4` | `Img_Image_3_07` | `Image_3` | Image |
| `Image_4` | `Img_Image_2_08` | `Image_2` | Image |
| `Image_4` | `Img_Image_4_09` | `Image_4` | Image |
| `Image_5` | `Img_Image_4_06` | `Image_4` | Image |
| `Image_5` | `Img_Image_3_06` | `Image_3` | Image |
| `Image_5` | `Img_Image_2_03` | `Image_2` | Image |
| `Image_6` | `Img_Image_3_01` | `Image_3` | Image |
| `Image_6` | `Img_Image_2_06` | `Image_2` | Image |
| `Image_6` | `Img_Image_4_07` | `Image_4` | Image |
| `Selected_Mat` | `Img_Crimson_Knights_01` | `Crimson_Knights` | Image |
| `Selected_Mat` | `Img_Sons_Of_Kasha_02` | `Sons_Of_Kasha` | Image |
| `Text` | `Txt_PathPopup3_01` | `PathPopup3` | TMP_Text |
| `Text` | `Txt_KOR_02` | `KOR` | TMP_Text |
| `Text` | `Txt_KOR_03` | `KOR` | TMP_Text |
| `Text` | `Txt_mask_07` | `mask` | TMP_Text |
| `Text` | `Txt_mask_05` | `mask` | TMP_Text |
| `Text_1` | `Txt_ENG_01` | `ENG` | TMP_Text |
| `Text_1` | `Txt_ENG_02` | `ENG` | TMP_Text |
| `Text_1` | `Txt_ENG_03` | `ENG` | TMP_Text |
| `Text_1` | `Txt_mask_04` | `mask` | TMP_Text |
| `Text_1` | `Txt_ENG_05` | `ENG` | TMP_Text |
| `Text_1` | `Txt_mask_06` | `mask` | TMP_Text |
| `Text_2` | `Txt_mask_01` | `mask` | TMP_Text |
| `Text_2` | `Txt_mask_02` | `mask` | TMP_Text |
| `mask` | `Mask_The_Pathless_01` | `The_Pathless` | Image,Mask |
| `mask` | `Mask_Crimson_Knights_02` | `Crimson_Knights` | Image,Mask |
| `mask` | `Mask_Sons_Of_Kasha_03` | `Sons_Of_Kasha` | Image,Mask |

## `Assets/@Resources/Prefab/UI/Popup/UI_PlayerRelatedTextPopup.prefab`

- 바인드 스크립트: 없음 (enum 보호 대상 없음)
- 변경된 오브젝트: 6개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `Image` | `Img_PlayerLevelUpText_01` | `PlayerLevelUpText` | Image |
| `Image` | `Img_PlayerRankUpText_02` | `PlayerRankUpText` | Image |
| `Image` | `Img_Image_03` | `Image` | Image |
| `Image` | `Img_Image_04` | `Image` | Image |
| `Text` | `Txt_Image_01` | `Image` | TMP_Text |
| `Text` | `Txt_Image_02` | `Image` | TMP_Text |

## `Assets/@Resources/Prefab/UI/Popup/UI_PotionDetailPopup.prefab`

- 바인드 스크립트: `Assets/@Scripts/UI/Popup/UI_PotionDetailPopup.cs` (enum 멤버 0개 보호)
- 변경된 오브젝트: 67개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `AmountTxt` | `Txt_ReqItemBox1_3_01` | `ReqItemBox1_3` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox1_5_02` | `ReqItemBox1_5` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox1_2_03` | `ReqItemBox1_2` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox1_4_04` | `ReqItemBox1_4` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox1_1_05` | `ReqItemBox1_1` | TMP_Text |
| `BlockImg` | `Img_ReqItemBox1_4_01` | `ReqItemBox1_4` | Image |
| `BlockImg` | `Img_ReqItemBox1_3_02` | `ReqItemBox1_3` | Image |
| `BlockImg` | `Img_ReqItemBox1_5_03` | `ReqItemBox1_5` | Image |
| `BlockImg` | `Img_ReqItemBox1_1_06` | `ReqItemBox1_1` | Image |
| `BlockImg` | `Img_ReqItemBox1_2_05` | `ReqItemBox1_2` | Image |
| `BtmImg_1` | `Img_SwitchPanelBtn_01` | `SwitchPanelBtn` | Image |
| `BtmImg_1` | `Img_NextLvlBtn_02` | `NextLvlBtn` | Image |
| `BtmImg_1` | `Img_BackBtn_03` | `BackBtn` | Image |
| `BtmImg_1` | `Img_CraftBtn_04` | `CraftBtn` | Image |
| `BtmImg_1` | `Img_CantCraftBtn_05` | `CantCraftBtn` | Image |
| `Frame` | `Img_PotionDetailPopupTitle_01` | `PotionDetailPopupTitle` | Image |
| `Frame` | `Img_ReqItemBox1_5_02` | `ReqItemBox1_5` | Image |
| `Frame` | `Img_ReqItemBox1_3_03` | `ReqItemBox1_3` | Image |
| `Frame` | `Img_ReqItemBox1_2_04` | `ReqItemBox1_2` | Image |
| `Frame` | `Img_ReqItemBox1_4_05` | `ReqItemBox1_4` | Image |
| `Frame` | `Img_ReqItemBox1_1_08` | `ReqItemBox1_1` | Image |
| `GameObject` | `Txt_Xbutton_01` | `Xbutton` | TMP_Text |
| `GameObject` | `Txt_ReqItemGroup_02` | `ReqItemGroup` | TMP_Text |
| `GlowMask` | `Mask_ReqItemBox1_3_01` | `ReqItemBox1_3` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox1_2_02` | `ReqItemBox1_2` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox1_4_03` | `ReqItemBox1_4` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox1_5_04` | `ReqItemBox1_5` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox1_1_05` | `ReqItemBox1_1` | Image,Mask |
| `Image` | `Img_Guage_NextLvl_01` | `Guage_NextLvl` | Image |
| `Image` | `Img_Guage_02` | `Guage` | Image |
| `Image` | `Img_Mask_03` | `Mask` | Image |
| `Image` | `Img_Mask_04` | `Mask` | Image |
| `Image_1` | `Img_Item_01` | `_` | Image |
| `Image_1` | `Img_Item_02` | `_` | Image |
| `Image_1` | `Img_Item_07` | `_` | Image |
| `Image_1` | `Img_Item_08` | `_` | Image |
| `Image_2` | `Img_Guage_NextLvl_03` | `Guage_NextLvl` | Image |
| `Image_2` | `Img_Guage_04` | `Guage` | Image |
| `Image_2` | `Img_Item_03` | `_` | Image |
| `Image_2` | `Img_Item_04` | `_` | Image |
| `Image_2` | `Img_Item_05` | `_` | Image |
| `Image_2` | `Img_Item_06` | `_` | Image |
| `Image_3` | `Img_Guage_01` | `Guage` | Image |
| `Image_3` | `Img_Guage_NextLvl_04` | `Guage_NextLvl` | Image |
| `Image_4` | `Img_PotionDetailPopupshineef_01` | `PotionDetailPopupshineeffect` | Image |
| `Image_4` | `Img_Guage_NextLvl_02` | `Guage_NextLvl` | Image |
| `Img` | `Btn_FrameImg_01` | `FrameImg` | Image,Button |
| `Img` | `Img_ReqItemBox1_4_02` | `ReqItemBox1_4` | Image |
| `Img` | `Img_ReqItemBox1_2_03` | `ReqItemBox1_2` | Image |
| `Img` | `Img_ReqItemBox1_1_04` | `ReqItemBox1_1` | Image |
| `Img` | `Img_ReqItemBox1_3_05` | `ReqItemBox1_3` | Image |
| `Img` | `Img_ReqItemBox1_5_06` | `ReqItemBox1_5` | Image |
| `_` | `Txt_SwitchPanelBtn_01` | `SwitchPanelBtn` | TMP_Text |
| `_` | `Mask_CraftBtn_02` | `CraftBtn` | Image,Mask |
| `_` | `Mask_BackBtn_03` | `BackBtn` | Image,Mask |
| `_` | `Mask_NextLvlBtn_04` | `NextLvlBtn` | Image,Mask |
| `_` | `Txt_NextLvlBtn_05` | `NextLvlBtn` | TMP_Text |
| `_` | `Txt_CraftBtn_06` | `CraftBtn` | TMP_Text |
| `_` | `Img_GlowMask_1_07` | `GlowMask_1` | Image |
| `_` | `Txt_CantCraftBtn_08` | `CantCraftBtn` | TMP_Text |
| `_` | `Txt_BackBtn_09` | `BackBtn` | TMP_Text |
| `_` | `Mask_CantCraftBtn_10` | `CantCraftBtn` | Image,Mask |
| `_` | `Img_GlowMask_11` | `GlowMask` | Image |
| `_` | `Img_GlowMask_12` | `GlowMask` | Image |
| `_` | `Img_GlowMask_13` | `GlowMask` | Image |
| `_` | `Img_GlowMask_14` | `GlowMask` | Image |
| `_` | `Img_GlowMask_15` | `GlowMask` | Image |

## `Assets/@Resources/Prefab/UI/Popup/UI_PotionPopupON.prefab`

- 바인드 스크립트: 없음 (enum 보호 대상 없음)
- 변경된 오브젝트: 13개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `Fill` | `Img_Fill_Area_01` | `Fill Area` | Image |
| `Fill` | `Img_Fill_Area_02` | `Fill Area` | Image |
| `Fill` | `Img_Fill_Area_03` | `Fill Area` | Image |
| `Fill Area` | `Panel_Slider_01` | `Slider` | — |
| `Fill Area` | `Panel_Speed_Potion_Cooldown_Ba_02` | `Speed Potion Cooldown Bar` | — |
| `Fill Area` | `Panel_HP_Potion_Cooldown_Bar_03` | `HP Potion Cooldown Bar` | — |
| `GameObject` | `Txt_Button_1_01` | `Button _1` | TMP_Text |
| `GameObject` | `Txt_Button_02` | `Button` | TMP_Text |
| `Image` | `Img_AutoBtn_01` | `AutoBtn` | Image |
| `Image` | `Img_AutoPotion_Guage_Lock_02` | `AutoPotion Guage Lock` | Image |
| `Text` | `Txt_Slider_01` | `Slider` | TMP_Text |
| `Text` | `Txt_Title_02` | `Title` | TMP_Text |
| `Text` | `Txt_AutoBtn_03` | `AutoBtn` | TMP_Text |

## `Assets/@Resources/Prefab/UI/Popup/UI_SettingPopup.prefab`

- 바인드 스크립트: `Assets/@Scripts/UI/Popup/UI_SettingPopup.cs` (enum 멤버 27개 보호)
- 변경된 오브젝트: 10개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `SettingPopupSoundTitle` | `Txt_SettingPopupSound_01` | `SettingPopupSound` | TMP_Text |
| `SettingPopupSoundTitle` | `Txt_SettingPopupLanguage_02` | `SettingPopupLanguage` | TMP_Text |
| `SettingPopupTitle` | `Panel_SettingPopupFrame_01` | `SettingPopupFrame` | — |
| `SettingPopupTitle` | `Txt_SettingPopupTitle_02` | `SettingPopupTitle` | TMP_Text |
| `Text_Legacy` | `Txt_EngBtn_01` | `EngBtn` | TMP_Text |
| `Text_Legacy` | `Txt_KorBtn_02` | `KorBtn` | TMP_Text |
| `_` | `Img_SettingPopupSound_01` | `SettingPopupSound` | Image |
| `_` | `Img_SettingPopupLanguage_02` | `SettingPopupLanguage` | Image |
| `_` | `Img_SettingPopupTitle_03` | `SettingPopupTitle` | Image |
| `_` | `Img_SettingPopupEffects_04` | `SettingPopupEffects` | Image |

## `Assets/@Resources/Prefab/UI/Popup/UI_SkillBookPopup.prefab`

- 바인드 스크립트: 없음 (enum 보호 대상 없음)
- 변경된 오브젝트: 146개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `===================` | `Obj_Content_01` | `Content` | — |
| `===================` | `Obj_Content_02` | `Content` | — |
| `Background` | `Img_SkillBookSkill_1_01` | `SkillBookSkill_1` | Image |
| `Background` | `Img_SkillBookSkill_3_02` | `SkillBookSkill_3` | Image |
| `Background` | `Img_SkillBookAttackSkill_0_03` | `SkillBookAttackSkill_0` | Image |
| `Background` | `Img_SkillBookSkill_0_04` | `SkillBookSkill_0` | Image |
| `Background` | `Img_SkillBookAttackSkill_1_05` | `SkillBookAttackSkill_1` | Image |
| `Background` | `Img_SkillBookSkill_6_06` | `SkillBookSkill_6` | Image |
| `Background` | `Img_SkillBookSkill_4_07` | `SkillBookSkill_4` | Image |
| `Background` | `Img_SkillBookDefenseSkill_0_08` | `SkillBookDefenseSkill_0` | Image |
| `Background` | `Img_SkillBookSkill_2_09` | `SkillBookSkill_2` | Image |
| `Background` | `Img_SkillBookSkill_5_10` | `SkillBookSkill_5` | Image |
| `EFrame` | `Img_SkillDetailActiveEf_1_01` | `SkillDetailActiveEf_1` | Image |
| `EFrame` | `Img_SkillDetailActiveEf_1_02` | `SkillDetailActiveEf_1` | Image |
| `EFrame` | `Img_SkillDetailActiveEf_1_03` | `SkillDetailActiveEf_1` | Image |
| `EFrame` | `Img_SkillDetailActiveEf_1_04` | `SkillDetailActiveEf_1` | Image |
| `EFrame` | `Img_SkillDetailActiveEf_1_05` | `SkillDetailActiveEf_1` | Image |
| `EFrame` | `Img_SkillDetailActiveEf_1_06` | `SkillDetailActiveEf_1` | Image |
| `EFrame` | `Img_SkillDetailActiveEf_1_07` | `SkillDetailActiveEf_1` | Image |
| `EFrame` | `Img_SkillDetailActiveEf_1_08` | `SkillDetailActiveEf_1` | Image |
| `EFrame` | `Img_SkillDetailActiveEf_1_09` | `SkillDetailActiveEf_1` | Image |
| `EFrame` | `Img_SkillDetailActiveEf_1_10` | `SkillDetailActiveEf_1` | Image |
| `Frame` | `Img_SkillBookSkill_6_01` | `SkillBookSkill_6` | Image |
| `Frame` | `Img_SkillBookSkill_2_02` | `SkillBookSkill_2` | Image |
| `Frame` | `Img_SkillBookAttackSkill_0_05` | `SkillBookAttackSkill_0` | Image |
| `Frame` | `Img_SkillBookAttackSkill_1_04` | `SkillBookAttackSkill_1` | Image |
| `Frame` | `Img_SkillBookSkill_4_05` | `SkillBookSkill_4` | Image |
| `Frame` | `Img_SkillBookSkill_3_06` | `SkillBookSkill_3` | Image |
| `Frame` | `Img_SkillBookDefenseSkill_0_07` | `SkillBookDefenseSkill_0` | Image |
| `Frame` | `Img_SkillBookSkill_1_08` | `SkillBookSkill_1` | Image |
| `Frame` | `Img_SkillBookSkill_0_09` | `SkillBookSkill_0` | Image |
| `Frame` | `Img_SkillBookSkill_5_12` | `SkillBookSkill_5` | Image |
| `Icon_2` | `Img_MemorizedIcon_01` | `MemorizedIcon` | Image |
| `Icon_2` | `Img_MemorizedIcon_02` | `MemorizedIcon` | Image |
| `Icon_2` | `Img_MemorizedIcon_03` | `MemorizedIcon` | Image |
| `Icon_2` | `Img_MemorizedIcon_04` | `MemorizedIcon` | Image |
| `Icon_2` | `Img_MemorizedIcon_05` | `MemorizedIcon` | Image |
| `Icon_2` | `Img_MemorizedIcon_06` | `MemorizedIcon` | Image |
| `Icon_2` | `Img_MemorizedIcon_07` | `MemorizedIcon` | Image |
| `Icon_2` | `Img_MemorizedIcon_08` | `MemorizedIcon` | Image |
| `Icon_2` | `Img_MemorizedIcon_09` | `MemorizedIcon` | Image |
| `Icon_2` | `Img_MemorizedIcon_10` | `MemorizedIcon` | Image |
| `LockedIcon` | `Img_SkillBookSkill_0_01` | `SkillBookSkill_0` | Image |
| `LockedIcon` | `Img_SkillBookAttackSkill_0_02` | `SkillBookAttackSkill_0` | Image |
| `LockedIcon` | `Img_SkillBookDefenseSkill_0_03` | `SkillBookDefenseSkill_0` | Image |
| `LockedIcon` | `Img_SkillBookSkill_4_04` | `SkillBookSkill_4` | Image |
| `LockedIcon` | `Img_SkillBookSkill_6_05` | `SkillBookSkill_6` | Image |
| `LockedIcon` | `Img_SkillBookSkill_1_06` | `SkillBookSkill_1` | Image |
| `LockedIcon` | `Img_SkillBookSkill_5_07` | `SkillBookSkill_5` | Image |
| `LockedIcon` | `Img_SkillBookSkill_3_11` | `SkillBookSkill_3` | Image |
| `LockedIcon` | `Img_SkillBookSkill_2_11` | `SkillBookSkill_2` | Image |
| `LockedIcon` | `Img_SkillBookAttackSkill_1_10` | `SkillBookAttackSkill_1` | Image |
| `LvlIcon` | `Img_SkillBookAttackSkill_1_01` | `SkillBookAttackSkill_1` | Image |
| `LvlIcon` | `Img_SkillBookSkill_5_04` | `SkillBookSkill_5` | Image |
| `LvlIcon` | `Img_SkillBookSkill_2_03` | `SkillBookSkill_2` | Image |
| `LvlIcon` | `Img_SkillBookSkill_6_04` | `SkillBookSkill_6` | Image |
| `LvlIcon` | `Img_SkillBookAttackSkill_0_08` | `SkillBookAttackSkill_0` | Image |
| `LvlIcon` | `Img_SkillBookSkill_3_08` | `SkillBookSkill_3` | Image |
| `LvlIcon` | `Img_SkillBookDefenseSkill_0_10` | `SkillBookDefenseSkill_0` | Image |
| `LvlIcon` | `Img_SkillBookSkill_4_10` | `SkillBookSkill_4` | Image |
| `LvlIcon` | `Img_SkillBookSkill_1_09` | `SkillBookSkill_1` | Image |
| `LvlIcon` | `Img_SkillBookSkill_0_10` | `SkillBookSkill_0` | Image |
| `MemorizedIcon` | `Img_SkillBookSkill_6_03` | `SkillBookSkill_6` | Image |
| `MemorizedIcon` | `Img_SkillBookSkill_5_02` | `SkillBookSkill_5` | Image |
| `MemorizedIcon` | `Img_SkillBookSkill_0_03` | `SkillBookSkill_0` | Image |
| `MemorizedIcon` | `Img_SkillBookSkill_2_04` | `SkillBookSkill_2` | Image |
| `MemorizedIcon` | `Img_SkillBookAttackSkill_0_07` | `SkillBookAttackSkill_0` | Image |
| `MemorizedIcon` | `Img_SkillBookAttackSkill_1_06` | `SkillBookAttackSkill_1` | Image |
| `MemorizedIcon` | `Img_SkillBookSkill_1_07` | `SkillBookSkill_1` | Image |
| `MemorizedIcon` | `Img_SkillBookSkill_4_08` | `SkillBookSkill_4` | Image |
| `MemorizedIcon` | `Img_SkillBookDefenseSkill_0_09` | `SkillBookDefenseSkill_0` | Image |
| `MemorizedIcon` | `Img_SkillBookSkill_3_10` | `SkillBookSkill_3` | Image |
| `SkillBookNewTxt_6` | `Txt_SkillBookSkill_6_01` | `SkillBookSkill_6` | TMP_Text |
| `SkillBookNewTxt_6` | `Txt_SkillBookAttackSkill_1_02` | `SkillBookAttackSkill_1` | TMP_Text |
| `SkillBookNewTxt_6` | `Txt_SkillBookAttackSkill_0_03` | `SkillBookAttackSkill_0` | TMP_Text |
| `SkillBookNewTxt_6` | `Txt_SkillBookDefenseSkill_0_04` | `SkillBookDefenseSkill_0` | TMP_Text |
| `SkillDetailActiveEf_1` | `Panel_Frame_01` | `Frame` | — |
| `SkillDetailActiveEf_1` | `Panel_Frame_02` | `Frame` | — |
| `SkillDetailActiveEf_1` | `Panel_Frame_03` | `Frame` | — |
| `SkillDetailActiveEf_1` | `Panel_Frame_04` | `Frame` | — |
| `SkillDetailActiveEf_1` | `Panel_Frame_05` | `Frame` | — |
| `SkillDetailActiveEf_1` | `Panel_Frame_06` | `Frame` | — |
| `SkillDetailActiveEf_1` | `Panel_Frame_07` | `Frame` | — |
| `SkillDetailActiveEf_1` | `Panel_Frame_08` | `Frame` | — |
| `SkillDetailActiveEf_1` | `Panel_Frame_09` | `Frame` | — |
| `SkillDetailActiveEf_1` | `Panel_Frame_10` | `Frame` | — |
| `Text` | `Txt_LvlIcon_01` | `LvlIcon` | TMP_Text |
| `Text` | `Txt_LvlIcon_02` | `LvlIcon` | TMP_Text |
| `Text` | `Txt_LvlIcon_03` | `LvlIcon` | TMP_Text |
| `Text` | `Txt_LvlIcon_04` | `LvlIcon` | TMP_Text |
| `Text` | `Txt_LvlIcon_05` | `LvlIcon` | TMP_Text |
| `Text` | `Txt_MemorizedIcon_06` | `MemorizedIcon` | TMP_Text |
| `Text` | `Txt_MemorizedIcon_07` | `MemorizedIcon` | TMP_Text |
| `Text` | `Txt_MemorizedIcon_08` | `MemorizedIcon` | TMP_Text |
| `Text` | `Txt_LvlIcon_09` | `LvlIcon` | TMP_Text |
| `Text` | `Txt_LvlIcon_10` | `LvlIcon` | TMP_Text |
| `Text` | `Txt_MemorizedIcon_11` | `MemorizedIcon` | TMP_Text |
| `Text` | `Txt_MemorizedIcon_12` | `MemorizedIcon` | TMP_Text |
| `Text` | `Txt_LvlIcon_13` | `LvlIcon` | TMP_Text |
| `Text` | `Txt_LvlIcon_14` | `LvlIcon` | TMP_Text |
| `Text` | `Txt_MemorizedIcon_15` | `MemorizedIcon` | TMP_Text |
| `Text` | `Txt_MemorizedIcon_16` | `MemorizedIcon` | TMP_Text |
| `Text` | `Txt_LvlIcon_17` | `LvlIcon` | TMP_Text |
| `Text` | `Txt_MemorizedIcon_18` | `MemorizedIcon` | TMP_Text |
| `Text` | `Txt_MemorizedIcon_19` | `MemorizedIcon` | TMP_Text |
| `Text` | `Txt_MemorizedIcon_20` | `MemorizedIcon` | TMP_Text |
| `WIcon` | `Btn_SkillBookSkill_5_01` | `SkillBookSkill_5` | Image,Button |
| `WIcon` | `Btn_SkillBookSkill_1_02` | `SkillBookSkill_1` | Image,Button |
| `WIcon` | `Btn_SkillBookSkill_6_03` | `SkillBookSkill_6` | Image,Button |
| `WIcon` | `Btn_SkillBookDefenseSkill_0_04` | `SkillBookDefenseSkill_0` | Image,Button |
| `WIcon` | `Btn_SkillBookSkill_4_05` | `SkillBookSkill_4` | Image,Button |
| `WIcon` | `Btn_SkillBookAttackSkill_1_06` | `SkillBookAttackSkill_1` | Image,Button |
| `WIcon` | `Btn_SkillBookSkill_0_07` | `SkillBookSkill_0` | Image,Button |
| `WIcon` | `Btn_SkillBookAttackSkill_0_08` | `SkillBookAttackSkill_0` | Image,Button |
| `WIcon` | `Btn_SkillBookSkill_2_09` | `SkillBookSkill_2` | Image,Button |
| `WIcon` | `Btn_SkillBookSkill_3_10` | `SkillBookSkill_3` | Image,Button |
| `frame` | `Img_LvlIcon_01` | `LvlIcon` | Image |
| `frame` | `Img_LvlIcon_02` | `LvlIcon` | Image |
| `frame` | `Img_LvlIcon_03` | `LvlIcon` | Image |
| `frame` | `Img_LvlIcon_04` | `LvlIcon` | Image |
| `frame` | `Img_LvlIcon_05` | `LvlIcon` | Image |
| `frame` | `Img_LvlIcon_06` | `LvlIcon` | Image |
| `frame` | `Img_LvlIcon_07` | `LvlIcon` | Image |
| `frame` | `Img_LvlIcon_08` | `LvlIcon` | Image |
| `frame` | `Img_LvlIcon_09` | `LvlIcon` | Image |
| `frame` | `Img_LvlIcon_10` | `LvlIcon` | Image |
| `lockimg` | `Img_LockedIcon_01` | `LockedIcon` | Image |
| `lockimg` | `Img_LockedIcon_02` | `LockedIcon` | Image |
| `lockimg` | `Img_LockedIcon_03` | `LockedIcon` | Image |
| `lockimg` | `Img_LockedIcon_04` | `LockedIcon` | Image |
| `lockimg` | `Img_LockedIcon_05` | `LockedIcon` | Image |
| `lockimg` | `Img_LockedIcon_06` | `LockedIcon` | Image |
| `lockimg` | `Img_LockedIcon_07` | `LockedIcon` | Image |
| `lockimg` | `Img_LockedIcon_08` | `LockedIcon` | Image |
| `lockimg` | `Img_LockedIcon_09` | `LockedIcon` | Image |
| `lockimg` | `Img_LockedIcon_10` | `LockedIcon` | Image |
| `pointBox` | `Img_text_3_01` | `text_3` | Image |
| `pointBox` | `Img_text_3_02` | `text_3` | Image |
| `pointBox` | `Img_text_3_03` | `text_3` | Image |
| `pointBox` | `Img_text_3_04` | `text_3` | Image |
| `pointBox` | `Img_text_3_05` | `text_3` | Image |
| `text_3` | `Txt_SkillPopup_ShowNotOnMemo_01` | `SkillPopup_ShowNotOnMemory_Btn` | TMP_Text |
| `text_3` | `Txt_SkillPopup_ShowLocked_Bt_02` | `SkillPopup_ShowLocked_Btn` | TMP_Text |
| `text_3` | `Txt_SkillPopup_ShowTalent_Bt_03` | `SkillPopup_ShowTalent_Btn` | TMP_Text |
| `text_3` | `Txt_SkillPopup_ShowAll_Btn_04` | `SkillPopup_ShowAll_Btn` | TMP_Text |
| `text_3` | `Txt_SkillPopup_ShowOnMemory__05` | `SkillPopup_ShowOnMemory_Btn` | TMP_Text |

## `Assets/@Resources/Prefab/UI/Popup/UI_SkillDetailPopup.prefab`

- 바인드 스크립트: `Assets/@Scripts/UI/Popup/UI_SkillDetailPopup.cs` (enum 멤버 3개 보호)
- 변경된 오브젝트: 205개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `AmountTxt` | `Txt_ReqItemBox1_5_01` | `ReqItemBox1_5` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox1_2_02` | `ReqItemBox1_2` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox_03` | `ReqItemBox` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox_2_04` | `ReqItemBox_2` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox1_3_05` | `ReqItemBox1_3` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox1_1_06` | `ReqItemBox1_1` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox_4_07` | `ReqItemBox_4` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox_3_08` | `ReqItemBox_3` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox_1_09` | `ReqItemBox_1` | TMP_Text |
| `AmountTxt` | `Txt_ReqItemBox1_4_10` | `ReqItemBox1_4` | TMP_Text |
| `Background` | `Img_NotChanging_01` | `NotChanging` | Image |
| `Background` | `Img_SkillDetailPopupIcon_04` | `SkillDetailPopupIcon` | Image |
| `BlockImg` | `Img_SkillDetailPopupIcon_01` | `SkillDetailPopupIcon` | Image |
| `BlockImg` | `Img_ReqItemBox1_4_02` | `ReqItemBox1_4` | Image |
| `BlockImg` | `Img_ReqMinionBox_3_03` | `ReqMinionBox_3` | Image |
| `BlockImg` | `Img_ReqItemBox_1_04` | `ReqItemBox_1` | Image |
| `BlockImg` | `Img_ReqItemBox_4_05` | `ReqItemBox_4` | Image |
| `BlockImg` | `Img_ReqMinionBox_4_06` | `ReqMinionBox_4` | Image |
| `BlockImg` | `Img_ReqItemBox_07` | `ReqItemBox` | Image |
| `BlockImg` | `Img_ReqItemBox1_1_08` | `ReqItemBox1_1` | Image |
| `BlockImg` | `Img_ReqItemBox1_5_09` | `ReqItemBox1_5` | Image |
| `BlockImg` | `Img_ReqItemBox_2_10` | `ReqItemBox_2` | Image |
| `BlockImg` | `Img_ReqItemBox1_2_11` | `ReqItemBox1_2` | Image |
| `BlockImg` | `Img_ReqMinionBox_2_12` | `ReqMinionBox_2` | Image |
| `BlockImg` | `Img_ReqMinionBox_1_13` | `ReqMinionBox_1` | Image |
| `BlockImg` | `Img_ReqMinionBox_14` | `ReqMinionBox` | Image |
| `BlockImg` | `Img_ReqItemBox_3_15` | `ReqItemBox_3` | Image |
| `BlockImg` | `Img_ReqItemBox1_3_16` | `ReqItemBox1_3` | Image |
| `BtmImg_1` | `Img_BackBtn_01` | `BackBtn` | Image |
| `BtmImg_1` | `Img_CantCraftBtn_02` | `CantCraftBtn` | Image |
| `BtmImg_1` | `Img_MemorizeBtn_03` | `MemorizeBtn` | Image |
| `BtmImg_1` | `Img_CraftBtn_04` | `CraftBtn` | Image |
| `BtmImg_1` | `Img_NotInPaceCraftBtn_05` | `NotInPaceCraftBtn` | Image |
| `BtmImg_1` | `Img_NextLvlBtn_06` | `NextLvlBtn` | Image |
| `CheckMark` | `Img_ReqMinionBox_1_01` | `ReqMinionBox_1` | Image |
| `CheckMark` | `Img_ReqMinionBox_3_02` | `ReqMinionBox_3` | Image |
| `CheckMark` | `Img_ReqMinionBox_4_03` | `ReqMinionBox_4` | Image |
| `CheckMark` | `Img_ReqMinionBox_06` | `ReqMinionBox` | Image |
| `CheckMark` | `Img_ReqMinionBox_2_05` | `ReqMinionBox_2` | Image |
| `DescTxt_00` | `Txt_UnlimitedPotenGroup_03` | `UnlimitedPotenGroup` | TMP_Text |
| `DescTxt_00` | `Txt_ClassGroup_04` | `ClassGroup` | TMP_Text |
| `DescTxt_01` | `Txt_UnlimitedPotenGroup_01` | `UnlimitedPotenGroup` | TMP_Text |
| `DescTxt_01` | `Txt_ClassGroup_02` | `ClassGroup` | TMP_Text |
| `Frame` | `Img_LvEffectTxt_Size_01` | `LvEffectTxt_Size` | Image |
| `Frame` | `Img_SkillDetailPopupIcon_02` | `SkillDetailPopupIcon` | Image |
| `Frame` | `Img_SkillDetailPopupTitle_03` | `SkillDetailPopupTitle` | Image |
| `Frame` | `Img_ReqMinionBox_1_04` | `ReqMinionBox_1` | Image |
| `Frame` | `Img_ReqItemBox1_4_05` | `ReqItemBox1_4` | Image |
| `Frame` | `Img_ReqItemBox1_3_06` | `ReqItemBox1_3` | Image |
| `Frame` | `Img_ReqMinionBox_2_07` | `ReqMinionBox_2` | Image |
| `Frame` | `Img_ReqMinionBox_3_08` | `ReqMinionBox_3` | Image |
| `Frame` | `Img_ReqItemBox1_1_09` | `ReqItemBox1_1` | Image |
| `Frame` | `Img_ReqItemBox_10` | `ReqItemBox` | Image |
| `Frame` | `Img_ReqItemBox_4_11` | `ReqItemBox_4` | Image |
| `Frame` | `Img_ReqMinionBox_12` | `ReqMinionBox` | Image |
| `Frame` | `Img_ReqItemBox1_2_13` | `ReqItemBox1_2` | Image |
| `Frame` | `Img_ReqMinionBox_4_14` | `ReqMinionBox_4` | Image |
| `Frame` | `Img_ReqItemBox_2_15` | `ReqItemBox_2` | Image |
| `Frame` | `Img_ReqItemBox_1_16` | `ReqItemBox_1` | Image |
| `Frame` | `Img_ReqItemBox_3_17` | `ReqItemBox_3` | Image |
| `Frame` | `Img_ReqItemBox1_5_18` | `ReqItemBox1_5` | Image |
| `GameObject` | `Txt_Xbutton_01` | `Xbutton` | TMP_Text |
| `GameObject` | `Txt_ReqMinionGroup_02` | `ReqMinionGroup` | TMP_Text |
| `GameObject` | `Txt_Title_03` | `Title` | TMP_Text |
| `GameObject` | `Txt_NoMEM_04` | `NoMEM` | TMP_Text |
| `GameObject` | `Txt_ClassGroup_05` | `ClassGroup` | TMP_Text |
| `GameObject` | `Txt_ReqEquipGroup_06` | `ReqEquipGroup` | TMP_Text |
| `GameObject` | `Txt_ReqItemGroup_07` | `ReqItemGroup` | TMP_Text |
| `GameObject` | `Txt_UnlimitedPotenGroup_08` | `UnlimitedPotenGroup` | TMP_Text |
| `GameObject` | `Txt_TalentHelpPopup_09` | `TalentHelpPopup` | TMP_Text |
| `GlowMask` | `Mask_ReqMinionBox_4_01` | `ReqMinionBox_4` | Image,Mask |
| `GlowMask` | `Mask_ReqMinionBox_2_02` | `ReqMinionBox_2` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox1_5_03` | `ReqItemBox1_5` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox1_2_04` | `ReqItemBox1_2` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox_2_05` | `ReqItemBox_2` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox_06` | `ReqItemBox` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox_1_07` | `ReqItemBox_1` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox_3_08` | `ReqItemBox_3` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox_4_09` | `ReqItemBox_4` | Image,Mask |
| `GlowMask` | `Mask_ReqMinionBox_10` | `ReqMinionBox` | Image,Mask |
| `GlowMask` | `Mask_ReqMinionBox_3_11` | `ReqMinionBox_3` | Image,Mask |
| `GlowMask` | `Mask_ReqMinionBox_1_12` | `ReqMinionBox_1` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox1_4_13` | `ReqItemBox1_4` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox1_1_14` | `ReqItemBox1_1` | Image,Mask |
| `GlowMask` | `Mask_ReqItemBox1_3_15` | `ReqItemBox1_3` | Image,Mask |
| `Icon_2` | `Img_MemorizedIcon_01` | `MemorizedIcon` | Image |
| `Icon_2` | `Img_MasterIcon_02` | `MasterIcon` | Image |
| `Image` | `Img_BlockImg_01` | `BlockImg` | Image |
| `Image` | `Btn_NoMEM_02` | `NoMEM` | Image,Button |
| `Image` | `Img_Mask_03` | `Mask` | Image |
| `Image` | `Img_MemoryIcon_04` | `MemoryIcon` | Image |
| `Image` | `Img_MemoryIcon_05` | `MemoryIcon` | Image |
| `Image_1` | `Img_Item_01` | `_` | Image |
| `Image_1` | `Img_Item_02` | `_` | Image |
| `Image_1` | `Img_Item_03` | `_` | Image |
| `Image_1` | `Img_Item_04` | `_` | Image |
| `Image_1` | `Img_Item_05` | `_` | Image |
| `Image_1` | `Img_Item_06` | `_` | Image |
| `Image_2` | `Img_SubFrame_01` | `SubFrame` | Image |
| `Image_2` | `Img_Item_07` | `_` | Image |
| `Image_2` | `Img_Item_08` | `_` | Image |
| `Image_2` | `Img_Item_09` | `_` | Image |
| `Image_2` | `Img_Item_10` | `_` | Image |
| `Image_2` | `Img_Item_11` | `_` | Image |
| `Image_2` | `Img_Item_12` | `_` | Image |
| `Image_3` | `Txt_Image_01` | `Image` | TMP_Text |
| `Image_3` | `Txt_Image_02` | `Image` | TMP_Text |
| `Image_4` | `Img_LackMemoryImg_01` | `LackMemoryImg` | Image |
| `Image_4` | `Img_Deactimg_02` | `Deactimg` | Image |
| `Image_5` | `Img_LackMemoryImg_03` | `LackMemoryImg` | Image |
| `Image_5` | `Img_Deactimg_04` | `Deactimg` | Image |
| `Img` | `Img_ReqItemBox1_3_01` | `ReqItemBox1_3` | Image |
| `Img` | `Img_ReqItemBox_3_02` | `ReqItemBox_3` | Image |
| `Img` | `Img_ReqItemBox_03` | `ReqItemBox` | Image |
| `Img` | `Img_ReqItemBox1_1_04` | `ReqItemBox1_1` | Image |
| `Img` | `Img_ReqItemBox_4_07` | `ReqItemBox_4` | Image |
| `Img` | `Img_ReqItemBox_1_06` | `ReqItemBox_1` | Image |
| `Img` | `Img_ReqItemBox1_4_07` | `ReqItemBox1_4` | Image |
| `Img` | `Img_ReqItemBox1_2_08` | `ReqItemBox1_2` | Image |
| `Img` | `Img_ReqItemBox1_5_11` | `ReqItemBox1_5` | Image |
| `Img` | `Img_ReqItemBox_2_12` | `ReqItemBox_2` | Image |
| `Img` | `Img_GlowMask_11` | `GlowMask` | Image |
| `Img` | `Img_GlowMask_12` | `GlowMask` | Image |
| `Img` | `Img_GlowMask_28` | `GlowMask` | Image |
| `Img` | `Img_GlowMask_29` | `GlowMask` | Image |
| `Img` | `Img_GlowMask_30` | `GlowMask` | Image |
| `MaxLvlTxt` | `Txt_ReqGroup_01` | `ReqGroup` | TMP_Text |
| `MaxLvlTxt` | `Txt_ReqMinionGroup_04` | `ReqMinionGroup` | TMP_Text |
| `MaxLvlTxt` | `Txt_ReqEquipGroup_03` | `ReqEquipGroup` | TMP_Text |
| `MemoryAmountTxt` | `Txt_GotMEM_01` | `GotMEM` | TMP_Text |
| `MemoryAmountTxt` | `Txt_MemoryDisplayGroup_02` | `MemoryDisplayGroup` | TMP_Text |
| `MemoryIcon` | `Btn_GotMEM_01` | `GotMEM` | Image,Button |
| `MemoryIcon` | `Btn_MemoryAmountTxt_02` | `MemoryAmountTxt` | Image,Button |
| `NeedMinionTxt0` | `Txt_NeedMinionTxtGroup_06` | `NeedMinionTxtGroup` | TMP_Text |
| `NeedMinionTxt0` | `Txt_NeedMinionTxtGroup_07` | `NeedMinionTxtGroup` | TMP_Text |
| `NeedMinionTxt0` | `Txt_NeedMinionTxtGroup_08` | `NeedMinionTxtGroup` | TMP_Text |
| `NeedMinionTxt0` | `Txt_NeedMinionTxtGroup_09` | `NeedMinionTxtGroup` | TMP_Text |
| `NeedMinionTxt0` | `Txt_NeedMinionTxtGroup_10` | `NeedMinionTxtGroup` | TMP_Text |
| `NeedMinionTxt1` | `Txt_NeedMinionTxtGroup_01` | `NeedMinionTxtGroup` | TMP_Text |
| `NeedMinionTxt1` | `Txt_NeedMinionTxtGroup_02` | `NeedMinionTxtGroup` | TMP_Text |
| `NeedMinionTxt1` | `Txt_NeedMinionTxtGroup_03` | `NeedMinionTxtGroup` | TMP_Text |
| `NeedMinionTxt1` | `Txt_NeedMinionTxtGroup_04` | `NeedMinionTxtGroup` | TMP_Text |
| `NeedMinionTxt1` | `Txt_NeedMinionTxtGroup_05` | `NeedMinionTxtGroup` | TMP_Text |
| `NeedMinionTxtGroup` | `Panel_ReqMinionBox_3_01` | `ReqMinionBox_3` | — |
| `NeedMinionTxtGroup` | `Panel_ReqMinionBox_02` | `ReqMinionBox` | — |
| `NeedMinionTxtGroup` | `Panel_ReqMinionBox_1_03` | `ReqMinionBox_1` | — |
| `NeedMinionTxtGroup` | `Panel_ReqMinionBox_2_04` | `ReqMinionBox_2` | — |
| `NeedMinionTxtGroup` | `Panel_ReqMinionBox_4_05` | `ReqMinionBox_4` | — |
| `RankCheckMark` | `Img_ReqMinionBox_4_01` | `ReqMinionBox_4` | Image |
| `RankCheckMark` | `Img_ReqMinionBox_1_02` | `ReqMinionBox_1` | Image |
| `RankCheckMark` | `Img_ReqMinionBox_2_03` | `ReqMinionBox_2` | Image |
| `RankCheckMark` | `Img_ReqMinionBox_04` | `ReqMinionBox` | Image |
| `RankCheckMark` | `Img_ReqMinionBox_3_05` | `ReqMinionBox_3` | Image |
| `RankTxt` | `Txt_ReqMinionBox_2_01` | `ReqMinionBox_2` | TMP_Text |
| `RankTxt` | `Txt_ReqMinionBox_1_02` | `ReqMinionBox_1` | TMP_Text |
| `RankTxt` | `Txt_ReqMinionBox_3_03` | `ReqMinionBox_3` | TMP_Text |
| `RankTxt` | `Txt_ReqMinionBox_4_04` | `ReqMinionBox_4` | TMP_Text |
| `RankTxt` | `Txt_ReqMinionBox_05` | `ReqMinionBox` | TMP_Text |
| `ReqItemList` | `Panel_ReqItemGroup_01` | `ReqItemGroup` | — |
| `ReqItemList` | `Panel_ReqEquipGroup_02` | `ReqEquipGroup` | — |
| `SYNCTxt` | `Txt_ReqMinionBox_4_01` | `ReqMinionBox_4` | TMP_Text |
| `SYNCTxt` | `Txt_ReqMinionBox_2_02` | `ReqMinionBox_2` | TMP_Text |
| `SYNCTxt` | `Txt_ReqMinionBox_3_05` | `ReqMinionBox_3` | TMP_Text |
| `SYNCTxt` | `Txt_ReqMinionBox_1_04` | `ReqMinionBox_1` | TMP_Text |
| `SYNCTxt` | `Txt_ReqMinionBox_07` | `ReqMinionBox` | TMP_Text |
| `Text` | `Txt_MemorizedIcon_01` | `MemorizedIcon` | TMP_Text |
| `Text` | `Txt_MasterIcon_02` | `MasterIcon` | TMP_Text |
| `_` | `Txt_MemorizeBtn_01` | `MemorizeBtn` | TMP_Text |
| `_` | `Mask_BackBtn_02` | `BackBtn` | Image,Mask |
| `_` | `Txt_NotInPaceCraftBtn_03` | `NotInPaceCraftBtn` | TMP_Text |
| `_` | `Mask_MemorizeBtn_04` | `MemorizeBtn` | Image,Mask |
| `_` | `Txt_CraftBtn_05` | `CraftBtn` | TMP_Text |
| `_` | `Mask_NextLvlBtn_06` | `NextLvlBtn` | Image,Mask |
| `_` | `Txt_NextLvlBtn_07` | `NextLvlBtn` | TMP_Text |
| `_` | `Mask_CantCraftBtn_08` | `CantCraftBtn` | Image,Mask |
| `_` | `Mask_NotInPaceCraftBtn_09` | `NotInPaceCraftBtn` | Image,Mask |
| `_` | `Mask_CraftBtn_10` | `CraftBtn` | Image,Mask |
| `_` | `Txt_CantCraftBtn_11` | `CantCraftBtn` | TMP_Text |
| `_` | `Txt_BackBtn_12` | `BackBtn` | TMP_Text |
| `_` | `Img_GlowMask_13` | `GlowMask` | Image |
| `_` | `Img_GlowMask_14` | `GlowMask` | Image |
| `_` | `Img_GlowMask_15` | `GlowMask` | Image |
| `_` | `Img_GlowMask_16` | `GlowMask` | Image |
| `_` | `Img_GlowMask_17` | `GlowMask` | Image |
| `_` | `Img_GlowMask_18` | `GlowMask` | Image |
| `_` | `Img_GlowMask_19` | `GlowMask` | Image |
| `_` | `Img_GlowMask_20` | `GlowMask` | Image |
| `_` | `Img_GlowMask_21` | `GlowMask` | Image |
| `_` | `Img_GlowMask_22` | `GlowMask` | Image |
| `_` | `Img_GlowMask_23` | `GlowMask` | Image |
| `_` | `Img_GlowMask_24` | `GlowMask` | Image |
| `_` | `Img_GlowMask_25` | `GlowMask` | Image |
| `_` | `Img_GlowMask_26` | `GlowMask` | Image |
| `_` | `Img_GlowMask_27` | `GlowMask` | Image |
| `__1` | `Img_ClassGroup_01` | `ClassGroup` | Image |
| `__1` | `Img_ReqItemGroup_02` | `ReqItemGroup` | Image |
| `__1` | `Img_ReqMinionGroup_03` | `ReqMinionGroup` | Image |
| `__1` | `Txt_NotInPaceCraftBtn_04` | `NotInPaceCraftBtn` | TMP_Text |
| `__1` | `Img_UnlimitedPotenGroup_05` | `UnlimitedPotenGroup` | Image |
| `__1` | `Img_ReqEquipGroup_06` | `ReqEquipGroup` | Image |
| `txt` | `Txt_BlockImg_01` | `BlockImg` | TMP_Text |
| `txt` | `Txt_BlockImg_02` | `BlockImg` | TMP_Text |
| `txt` | `Txt_BlockImg_03` | `BlockImg` | TMP_Text |
| `txt` | `Txt_BlockImg_04` | `BlockImg` | TMP_Text |
| `txt` | `Txt_BlockImg_05` | `BlockImg` | TMP_Text |

## `Assets/@Resources/Prefab/UI/Popup/UI_TamePopup.prefab`

- 바인드 스크립트: 없음 (enum 보호 대상 없음)
- 변경된 오브젝트: 131개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `BoxLine_1` | `Panel_LoadingBox2_03` | `LoadingBox2` | — |
| `BoxLine_1` | `Panel_LoadingBox1_07` | `LoadingBox1` | — |
| `BoxLine_2` | `Panel_LoadingBox1_01` | `LoadingBox1` | — |
| `BoxLine_2` | `Panel_LoadingBox2_02` | `LoadingBox2` | — |
| `BoxLine_3` | `Panel_LoadingBox1_06` | `LoadingBox1` | — |
| `BoxLine_3` | `Panel_LoadingBox2_07` | `LoadingBox2` | — |
| `BoxLine_4` | `Panel_LoadingBox1_04` | `LoadingBox1` | — |
| `BoxLine_4` | `Panel_LoadingBox2_05` | `LoadingBox2` | — |
| `BoxLine_5` | `Panel_LoadingBox1_03` | `LoadingBox1` | — |
| `BoxLine_5` | `Panel_LoadingBox2_04` | `LoadingBox2` | — |
| `BoxLine_6` | `Panel_LoadingBox1_05` | `LoadingBox1` | — |
| `BoxLine_6` | `Panel_LoadingBox2_06` | `LoadingBox2` | — |
| `BoxLine_7` | `Panel_LoadingBox2_01` | `LoadingBox2` | — |
| `BoxLine_7` | `Panel_LoadingBox1_02` | `LoadingBox1` | — |
| `Frame` | `Panel_TamePopup_01` | `TamePopup` | — |
| `Frame` | `Img_HappyMandrakeBox_02` | `HappyMandrakeBox` | Image |
| `Frame` | `Img_TameConfirmPopups_03` | `TameConfirmPopups` | Image |
| `Frame_1` | `Img_Frame_01` | `Frame` | Image |
| `Frame_1` | `Img_TameConfirmPopups_02` | `TameConfirmPopups` | Image |
| `GameObject` | `Txt_MinimonInfoBtn_01` | `MinimonInfoBtn` | TMP_Text |
| `GameObject` | `Txt_Frame_1_02` | `Frame_1` | TMP_Text |
| `GameObject` | `Txt_Title_03` | `Title` | TMP_Text |
| `GameObject` | `Txt_ENG_TameConfirmPopup_06` | `ENG_TameConfirmPopup` | TMP_Text |
| `GameObject` | `Txt_TameConfirmPopup_UseMand_05` | `TameConfirmPopup_UseMandrakeBtn` | TMP_Text |
| `GameObject` | `Txt_KOR_TameConfirmPopup_06` | `KOR_TameConfirmPopup` | TMP_Text |
| `GameObject_1` | `Txt_Frame_1_01` | `Frame_1` | TMP_Text |
| `GameObject_1` | `Txt_KOR_TameConfirmPopup_02` | `KOR_TameConfirmPopup` | TMP_Text |
| `GameObject_1` | `Txt_Image_1_03` | `Image_1` | TMP_Text |
| `GameObject_1` | `Txt_ENG_TameConfirmPopup_04` | `ENG_TameConfirmPopup` | TMP_Text |
| `Image` | `Img_MinimonInfoBtn_01` | `MinimonInfoBtn` | Image |
| `Image` | `Img_BoxLine_1_05` | `BoxLine_1` | Image |
| `Image` | `Img_BoxLine_3_03` | `BoxLine_3` | Image |
| `Image` | `Img_BoxLine_6_04` | `BoxLine_6` | Image |
| `Image` | `Img_BoxLine_6_05` | `BoxLine_6` | Image |
| `Image` | `Img_BoxLine_4_06` | `BoxLine_4` | Image |
| `Image` | `Img_BoxLine_2_09` | `BoxLine_2` | Image |
| `Image` | `Img_BoxLine_7_10` | `BoxLine_7` | Image |
| `Image` | `Img_BoxLine_3_09` | `BoxLine_3` | Image |
| `Image` | `Img_BoxLine_1_10` | `BoxLine_1` | Image |
| `Image` | `Img_BoxLine_2_11` | `BoxLine_2` | Image |
| `Image` | `Img_BoxLine_5_12` | `BoxLine_5` | Image |
| `Image` | `Img_BoxLine_7_13` | `BoxLine_7` | Image |
| `Image` | `Img_BoxLine_4_14` | `BoxLine_4` | Image |
| `Image` | `Img_BoxLine_5_15` | `BoxLine_5` | Image |
| `Image_1` | `Btn_Frame_01` | `Frame` | Image,Button |
| `Image_1` | `Img_BoxLine_7_02` | `BoxLine_7` | Image |
| `Image_1` | `Img_BoxLine_5_06` | `BoxLine_5` | Image |
| `Image_1` | `Img_BoxLine_3_06` | `BoxLine_3` | Image |
| `Image_1` | `Img_BoxLine_5_11` | `BoxLine_5` | Image |
| `Image_1` | `Img_BoxLine_3_08` | `BoxLine_3` | Image |
| `Image_1` | `Img_BoxLine_6_07` | `BoxLine_6` | Image |
| `Image_1` | `Img_BoxLine_1_12` | `BoxLine_1` | Image |
| `Image_1` | `Img_BoxLine_2_15` | `BoxLine_2` | Image |
| `Image_1` | `Img_BoxLine_7_14` | `BoxLine_7` | Image |
| `Image_1` | `Img_BoxLine_4_13` | `BoxLine_4` | Image |
| `Image_1` | `Img_BoxLine_2_16` | `BoxLine_2` | Image |
| `Image_1` | `Img_BoxLine_1_15` | `BoxLine_1` | Image |
| `Image_1` | `Img_BoxLine_4_16` | `BoxLine_4` | Image |
| `Image_1` | `Img_BoxLine_6_15` | `BoxLine_6` | Image |
| `Image_2` | `Img_BoxLine_5_01` | `BoxLine_5` | Image |
| `Image_2` | `Img_BoxLine_1_07` | `BoxLine_1` | Image |
| `Image_2` | `Img_BoxLine_7_03` | `BoxLine_7` | Image |
| `Image_2` | `Img_BoxLine_3_07` | `BoxLine_3` | Image |
| `Image_2` | `Img_BoxLine_3_05` | `BoxLine_3` | Image |
| `Image_2` | `Img_BoxLine_6_10` | `BoxLine_6` | Image |
| `Image_2` | `Img_BoxLine_7_09` | `BoxLine_7` | Image |
| `Image_2` | `Img_BoxLine_6_11` | `BoxLine_6` | Image |
| `Image_2` | `Img_BoxLine_4_09` | `BoxLine_4` | Image |
| `Image_2` | `Img_BoxLine_4_10` | `BoxLine_4` | Image |
| `Image_2` | `Img_BoxLine_5_13` | `BoxLine_5` | Image |
| `Image_2` | `Img_BoxLine_1_14` | `BoxLine_1` | Image |
| `Image_2` | `Img_BoxLine_2_17` | `BoxLine_2` | Image |
| `Image_2` | `Img_BoxLine_2_18` | `BoxLine_2` | Image |
| `Image_3` | `Img_BoxLine_4_01` | `BoxLine_4` | Image |
| `Image_3` | `Img_BoxLine_1_02` | `BoxLine_1` | Image |
| `Image_3` | `Img_BoxLine_5_03` | `BoxLine_5` | Image |
| `Image_3` | `Img_BoxLine_1_04` | `BoxLine_1` | Image |
| `Image_3` | `Img_BoxLine_5_05` | `BoxLine_5` | Image |
| `Image_3` | `Img_BoxLine_2_06` | `BoxLine_2` | Image |
| `Image_3` | `Img_BoxLine_2_07` | `BoxLine_2` | Image |
| `Image_3` | `Img_BoxLine_7_08` | `BoxLine_7` | Image |
| `Image_3` | `Img_BoxLine_6_09` | `BoxLine_6` | Image |
| `Image_3` | `Img_BoxLine_3_10` | `BoxLine_3` | Image |
| `Image_3` | `Img_BoxLine_4_11` | `BoxLine_4` | Image |
| `Image_3` | `Img_BoxLine_7_12` | `BoxLine_7` | Image |
| `Image_3` | `Img_BoxLine_3_13` | `BoxLine_3` | Image |
| `Image_3` | `Img_BoxLine_6_14` | `BoxLine_6` | Image |
| `Image_4` | `Img_BoxLine_7_01` | `BoxLine_7` | Image |
| `Image_4` | `Img_BoxLine_4_02` | `BoxLine_4` | Image |
| `Image_4` | `Img_BoxLine_4_07` | `BoxLine_4` | Image |
| `Image_4` | `Img_BoxLine_3_04` | `BoxLine_3` | Image |
| `Image_4` | `Img_BoxLine_7_05` | `BoxLine_7` | Image |
| `Image_4` | `Img_BoxLine_1_08` | `BoxLine_1` | Image |
| `Image_4` | `Img_BoxLine_2_10` | `BoxLine_2` | Image |
| `Image_4` | `Img_BoxLine_5_08` | `BoxLine_5` | Image |
| `Image_4` | `Img_BoxLine_5_09` | `BoxLine_5` | Image |
| `Image_4` | `Img_BoxLine_3_12` | `BoxLine_3` | Image |
| `Image_4` | `Img_BoxLine_2_13` | `BoxLine_2` | Image |
| `Image_4` | `Img_BoxLine_6_12` | `BoxLine_6` | Image |
| `Image_4` | `Img_BoxLine_1_13` | `BoxLine_1` | Image |
| `Image_4` | `Img_BoxLine_6_16` | `BoxLine_6` | Image |
| `Image_5` | `Img_BoxLine_1_01` | `BoxLine_1` | Image |
| `Image_5` | `Img_BoxLine_4_04` | `BoxLine_4` | Image |
| `Image_5` | `Img_BoxLine_3_11` | `BoxLine_3` | Image |
| `Image_5` | `Img_BoxLine_4_08` | `BoxLine_4` | Image |
| `Image_5` | `Img_BoxLine_7_11` | `BoxLine_7` | Image |
| `Image_5` | `Img_BoxLine_5_14` | `BoxLine_5` | Image |
| `Image_5` | `Img_BoxLine_1_09` | `BoxLine_1` | Image |
| `Image_5` | `Img_BoxLine_2_08` | `BoxLine_2` | Image |
| `Image_5` | `Img_BoxLine_7_16` | `BoxLine_7` | Image |
| `Image_5` | `Img_BoxLine_2_19` | `BoxLine_2` | Image |
| `Image_5` | `Img_BoxLine_6_13` | `BoxLine_6` | Image |
| `Image_5` | `Img_BoxLine_5_16` | `BoxLine_5` | Image |
| `Image_5` | `Img_BoxLine_6_17` | `BoxLine_6` | Image |
| `Image_5` | `Img_BoxLine_3_16` | `BoxLine_3` | Image |
| `Image_6` | `Img_BoxLine_4_03` | `BoxLine_4` | Image |
| `Image_6` | `Img_BoxLine_3_02` | `BoxLine_3` | Image |
| `Image_6` | `Img_BoxLine_4_05` | `BoxLine_4` | Image |
| `Image_6` | `Img_BoxLine_6_06` | `BoxLine_6` | Image |
| `Image_6` | `Img_BoxLine_5_07` | `BoxLine_5` | Image |
| `Image_6` | `Img_BoxLine_1_06` | `BoxLine_1` | Image |
| `Image_6` | `Img_BoxLine_7_07` | `BoxLine_7` | Image |
| `Image_6` | `Img_BoxLine_6_08` | `BoxLine_6` | Image |
| `Image_6` | `Img_BoxLine_2_12` | `BoxLine_2` | Image |
| `Image_6` | `Img_BoxLine_5_10` | `BoxLine_5` | Image |
| `Image_6` | `Img_BoxLine_1_11` | `BoxLine_1` | Image |
| `Image_6` | `Img_BoxLine_2_14` | `BoxLine_2` | Image |
| `Image_6` | `Img_BoxLine_7_15` | `BoxLine_7` | Image |
| `Image_6` | `Img_BoxLine_3_14` | `BoxLine_3` | Image |
| `_` | `Img_Mask_01` | `Mask` | Image |
| `_` | `Img_GlowMask_02` | `GlowMask` | Image |

## `Assets/@Resources/Prefab/UI/Popup/UI_WarningPopup.prefab`

- 바인드 스크립트: 없음 (enum 보호 대상 없음)
- 변경된 오브젝트: 9개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `GameObject` | `Img_AttactedPopup_01` | `AttactedPopup` | Image |
| `GameObject` | `Img_TameSlotMaxWarningPopup_02` | `TameSlotMaxWarningPopup` | Image |
| `GameObject_1` | `Img_AttactedPopup_03` | `AttactedPopup` | Image |
| `GameObject_1` | `Img_TameSlotMaxWarningPopup_04` | `TameSlotMaxWarningPopup` | Image |
| `GameObject_2` | `Img_AttactedPopup_04` | `AttactedPopup` | Image |
| `GameObject_2` | `Img_TameSlotMaxWarningPopup_05` | `TameSlotMaxWarningPopup` | Image |
| `GameObject_3` | `Mask_AttactedPopup_01` | `AttactedPopup` | Image,Mask |
| `GameObject_3` | `Txt_GameObject_2_02` | `GameObject_2` | TMP_Text |
| `GameObject_3` | `Txt_GameObject_2_03` | `GameObject_2` | TMP_Text |

## `Assets/@Resources/Prefab/UI/Scene/UI_GameScene.prefab`

- 바인드 스크립트: `Assets/@Scripts/UI/Scene/UI_GameScene.cs` (enum 멤버 88개 보호)
- 변경된 오브젝트: 26개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `GlowDot_01` | `VFX_Quest_Done_Effect_01` | `Quest_Done_Effect` | ParticleSystem |
| `GlowDot_01` | `VFX_BestiaryBtnNewIcon_02` | `BestiaryBtnNewIcon` | ParticleSystem |
| `Icon` | `Img_ExitButton_01` | `ExitButton` | Image |
| `Icon` | `Img_GuildButton_3_02` | `GuildButton(3)` | Image |
| `Icon` | `Img_TravelButton_OnBattle_03` | `TravelButton(OnBattle)` | Image |
| `Icon` | `Img_GuildButton_4_04` | `GuildButton(4)` | Image |
| `Icon` | `Img_GuildButton_2_05` | `GuildButton(2)` | Image |
| `Icon` | `Img_GuildButton_1_06` | `GuildButton(1)` | Image |
| `Icon` | `Img_GuildButton_07` | `GuildButton` | Image |
| `Image` | `Img_Main_QuestFollow_01` | `Main_QuestFollow` | Image |
| `Image` | `Img_Main_QuestFollow_02` | `Main_QuestFollow` | Image |
| `Image` | `Img_Main_QuestFollow_03` | `Main_QuestFollow` | Image |
| `Image` | `Img_BTMButtonGroup_04` | `BTMButtonGroup` | Image |
| `Image` | `Img_Image_05` | `Image` | Image |
| `Image` | `Img_BestiaryBtnNewIcon_06` | `BestiaryBtnNewIcon` | Image |
| `QuestMarker` | `Img_ExitQuestVFX_01` | `ExitQuestVFX` | Image |
| `QuestMarker` | `Img_TravelButton_OnBattle_Qu_02` | `TravelButton(OnBattle)QuestVFX` | Image |
| `Text` | `Txt_ExitButton_01` | `ExitButton` | TMP_Text |
| `Text` | `Txt_GuildButton_4_02` | `GuildButton(4)` | TMP_Text |
| `Text` | `Txt_TravelButton_OnBattle_03` | `TravelButton(OnBattle)` | TMP_Text |
| `Text` | `Txt_GuildButton_1_04` | `GuildButton(1)` | TMP_Text |
| `Text` | `Txt_GuildButton_05` | `GuildButton` | TMP_Text |
| `Text` | `Txt_GuildButton_3_06` | `GuildButton(3)` | TMP_Text |
| `Text` | `Txt_GuildButton_2_07` | `GuildButton(2)` | TMP_Text |
| `TotalFrame` | `Img_FIXED_01` | `FIXED` | Image |
| `TotalFrame` | `Img_PlayerBar_02` | `PlayerBar` | Image |

## `Assets/@Resources/Prefab/UI/SubItem/UI_BattleBtnGroup.prefab`

- 바인드 스크립트: `Assets/@Scripts/UI/SubItem/UI_BattleBtnGroup.cs` (enum 멤버 23개 보호)
- 변경된 오브젝트: 8개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `Icon` | `Img_TempButton5_01` | `TempButton5` | Image |
| `Icon` | `Img_TempButton4_02` | `TempButton4` | Image |
| `QuestMarker` | `Img_Village1AlchemyVFX_Sub1_01` | `Village1AlchemyVFX_Sub1` | Image |
| `QuestMarker` | `Img_TravelButtonQuestVFX_02` | `TravelButtonQuestVFX` | Image |
| `QuestMarker` | `Img_Village1GuildVFX_03` | `Village1GuildVFX` | Image |
| `QuestMarker` | `Img_MonasteryVFX_Sub1_04` | `MonasteryVFX_Sub1` | Image |
| `Text` | `Txt_TempButton5_01` | `TempButton5` | TMP_Text |
| `Text` | `Txt_TempButton4_02` | `TempButton4` | TMP_Text |

## `Assets/@Resources/Prefab/UI/SubItem/UI_Village1BtnGroup.prefab`

- 바인드 스크립트: `Assets/@Scripts/UI/SubItem/UI_Village1BtnGroup.cs` (enum 멤버 103개 보호)
- 변경된 오브젝트: 20개

| Old Name | New Name | Parent | Components |
| --- | --- | --- | --- |
| `Icon` | `Img_Village1GuildTalkButton_01` | `Village1GuildTalkButton` | Image |
| `Icon` | `Img_TempButton2_02` | `TempButton2` | Image |
| `Icon` | `Img_TempButton1_03` | `TempButton1` | Image |
| `QuestMarker` | `Img_Village1MonasteryTalkVFX_01` | `Village1MonasteryTalkVFX` | Image |
| `QuestMarker` | `Img_Village1GuildQuestVFX_02` | `Village1GuildQuestVFX` | Image |
| `QuestMarker` | `Img_Village1MonasteryQuestVF_03` | `Village1MonasteryQuestVFX` | Image |
| `QuestMarker` | `Img_Village1AlchemyQuestVFX_04` | `Village1AlchemyQuestVFX` | Image |
| `QuestMarker` | `Img_Village1AlchemyVFX_Sub1_05` | `Village1AlchemyVFX_Sub1` | Image |
| `QuestMarker` | `Img_Village1AlchemyTalkVFX_06` | `Village1AlchemyTalkVFX` | Image |
| `QuestMarker` | `Img_Village1GuildTalkVFX_07` | `Village1GuildTalkVFX` | Image |
| `QuestMarker` | `Img_Village1TravelQuestVFX_08` | `Village1TravelQuestVFX` | Image |
| `QuestMarker` | `Img_MonasteryExitQuestVFX_09` | `MonasteryExitQuestVFX` | Image |
| `QuestMarker` | `Img_Village1AlchemyTalkVFX_S_10` | `Village1AlchemyTalkVFX_Sub1` | Image |
| `QuestMarker` | `Img_GuildExitQuestVFX_11` | `GuildExitQuestVFX` | Image |
| `QuestMarker` | `Img_Village1GuildVFX_12` | `Village1GuildVFX` | Image |
| `QuestMarker` | `Img_AlchemyExitQuestVFX_13` | `AlchemyExitQuestVFX` | Image |
| `QuestMarker` | `Img_Village1MonasteryTalkVFX_14` | `Village1MonasteryTalkVFX_Sub1` | Image |
| `QuestMarker` | `Img_MonasteryVFX_Sub1_15` | `MonasteryVFX_Sub1` | Image |
| `Text` | `Txt_TempButton2_01` | `TempButton2` | TMP_Text |
| `Text` | `Txt_TempButton1_02` | `TempButton1` | TMP_Text |

---

## 요약

- 전체 UI 프리펩: 41개
- 변경된 프리펩: 32개
- 변경된 오브젝트: 2178개
