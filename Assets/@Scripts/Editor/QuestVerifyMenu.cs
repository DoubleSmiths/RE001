#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using Data;
using UnityEditor;
using UnityEngine;
using static Define;

// 미니몬퀘스트 기획서(part1 + part2 05.29) 기반 시나리오 검증 메뉴.
//
// 핵심 흐름:
//   A. [Reset] 세이브 삭제 → PlayMode 재진입 → 첫실행 상태로 시작
//   B. [Show All] 퀘스트 진행 없이 모든 컨텐츠를 즉시 해금/시드 → 모든 UI/연출 확인 가능
//   C. [Slide N] 각 슬라이드의 핵심 UI를 한 클릭으로 열기 (검증 체크리스트 동시 출력)
//
// 사용법:
//   1) PlayMode 진입 (▶)
//   2) Test/Quest Verify/A. Reset & Re-init  (필요 시)
//   3) Test/Quest Verify/B. Show All & Seed   ← 가장 자주 쓰임. 한 번에 모든 컨텐츠 해금
//   4) Test/Quest Verify/Slide ##  ← 슬라이드별 시연
public static class QuestVerifyMenu
{
    static bool Require()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[QuestVerify] PlayMode 진입 후 실행하세요. (메뉴 ▶ 버튼)");
            return false;
        }
        if (Managers.Game == null)
        {
            Debug.LogWarning("[QuestVerify] Managers 아직 준비 안 됨.");
            return false;
        }
        return true;
    }

    static UI_GameScene GameScene => Managers.UI?.SceneUI as UI_GameScene;

    // ─────────────────────────────────────────────────────────
    // A. Reset (세이브 삭제 → 첫 실행 상태)
    // ─────────────────────────────────────────────────────────
    [MenuItem("Test/Quest Verify/A. Reset & Re-init (세이브 삭제) %#r", false, 0)]
    static void ResetAndReinit()
    {
        string path = Application.persistentDataPath + "/SaveData.json";
        if (File.Exists(path)) File.Delete(path);
        PlayerPrefs.SetInt("ISFIRST", 1);
        PlayerPrefs.Save();
        Debug.Log($"[QuestVerify] 세이브 삭제 + ISFIRST=1.\n" +
                  $"  PATH: {path}\n" +
                  $"  ⚠ PlayMode를 ▶ Stop → ▶ Play 다시 누르면 첫 실행 상태로 재진입합니다.");
    }

    // ─────────────────────────────────────────────────────────
    // B. Show All & Seed — 모든 컨텐츠를 즉시 해금/시드.
    // 퀘스트가 짧아도 슬라이드별 모든 UI 상태를 확인 가능.
    // ─────────────────────────────────────────────────────────
    [MenuItem("Test/Quest Verify/B. Show All & Seed (모든 컨텐츠 해금) %#u", false, 1)]
    static void ShowAllAndSeed()
    {
        if (!Require()) return;
        var log = new StringBuilder();
        log.AppendLine("════ Show All & Seed ════");

        // 1) 재화/스탯 (슬6/슬7/슬16)
        Managers.Game.Gold        = 99999;
        Managers.Game.Crystal     = 9999;
        Managers.Game.StatusPoint = 10;
        Managers.Game.Elixir      = 5;
        Managers.Game.AdventurerRank = 5;
        log.AppendLine("● 슬6/슬7/슬16: 재화 Gold/Crystal/SP/Elixir 충전, AdvRank=5");

        // 2) 하단 6탭 전부 해금 + Travel/Alchemy 버튼 해금 (슬10/슬25~29/슬33)
        foreach (TabType tab in System.Enum.GetValues(typeof(TabType)))
            Managers.Game.UnlockBtmButton(tab);
        Managers.Game.AddUnlockedBtn(UNLOCK_TRAVEL_BUTTON);
        Managers.Game.AddUnlockedBtn(UNLOCK_ALCHEMY_BUTTON);
        Managers.Game.AddUnlockedBtn(UNLOCK_INVENTORY_BUTTON);
        Managers.Game.AddUnlockedBtn(UNLOCK_POTION_BUTTON);
        log.AppendLine("● 슬10: 하단 6탭 전부 해금 + Travel/Alchemy 진입버튼 해금");

        // 3) 인벤토리 (슬19/슬20/슬21)
        Managers.Game.AddItem(10001, 10);   // 젤리 퐁
        Managers.Game.AddItem(10002, 10);   // 폴짝 젤리
        Managers.Game.AddItem(20001, 3);    // 귀중품(통행증 등)
        log.AppendLine("● 슬19/슬20/슬21: 인벤토리 일반/귀중품 시드");

        // 4) 능력 시스템 (슬38~42)
        //    - 도감 4스킬 (7000/7001/7002/5001) 자동 학습
        //    - 능력 단계 다양화
        //    - 기억확장(5001) Lv.3 → MaxMemory=25
        UI_BestiaryPopup.SetSkillUnlocked(0, true); UI_BestiaryPopup.SetSkillGrade(0, 2); // 종초월교감 +2등급
        UI_BestiaryPopup.SetSkillUnlocked(1, true); UI_BestiaryPopup.SetSkillGrade(1, 1); // 조련사의 기술 +1
        UI_BestiaryPopup.SetSkillUnlocked(2, true); UI_BestiaryPopup.SetSkillGrade(2, 0); // 오다 주웠 +0
        UI_BestiaryPopup.SetSkillUnlocked(3, true); UI_BestiaryPopup.SetSkillGrade(3, 2); // 기억확장 +2
        Managers.Ability.Learn(5002); Managers.Ability.Memorize(5002); // 힘의 기억(점유3)
        Managers.Ability.Learn(5003); Managers.Ability.Memorize(5003); // 치명의 기억(점유2)
        Managers.Ability.Learn(5004);                                  // 강철의 기억(보유만)
        log.AppendLine("● 슬2/38~42: 도감4스킬 학습, 5002·5003 기억(장착), 5004 보유. MaxMem 보너스 적용");

        // 5) 미니몬 시드 (슬9~14, 슬18, 슬20, 슬23~27)
        SeedMinMon(2001, killCount: 100, caught: 4, atkG: 5, asG: 4, critG: 5); // 젤리 퐁: 기록100% + 공명도100% + AllSS 직전(AS 4)
        SeedMinMon(2002, killCount: 5,   caught: 1, atkG: 1, asG: 2, critG: 0); // 라비: 기록 5% (실루엣) + 공명도 25% + 1포획
        Managers.MinMon.SetCompanion(2001);
        log.AppendLine("● 슬9/슬10/슬14/슬18/슬20/슬23: 2001 풀시드+동행, 2002 부분시드");

        // 6) 관심 창고 시드 (슬23~27)
        Managers.MinMon.AddInterest(2002);
        log.AppendLine("● 슬26: 도감버튼 NEW 뱃지 표시용 관심창고 1슬롯 채움");

        // 7) 물약 체인 (슬37~42)
        Managers.Game.CurrentHPPotionDataId = POTION_ID_MIN + 2;       // HP 2단계
        Managers.Game.CurrentSpeedPotionDataId = SPEED_POTION_ID_MIN + 1; // Speed 1단계
        Managers.Game.SpdPotionRevealed = true;
        Managers.Game.SpdPotionPopupAnimPlayed = false; // 첫 진입 시 1회 해금연출 보기
        log.AppendLine("● 슬37~42: HP 2단계, Speed 해금 후 1단계 (popup anim 미재생)");

        // 8) 퀘스트 진행도 — 마지막 퀘스트(QuestID=6) 직전 상태로 둬 [의뢰완료보고]까지 UI 보존
        if (Managers.Quest != null)
            Managers.Quest.JumpToQuest(6);
        log.AppendLine("● 슬22~24/슬30/슬34: 퀘스트 ID=6 으로 점프 (마지막 의뢰 진행 중)");

        // 9) 저장 + UI 즉시 반영
        Managers.Game.SaveGame();
        GameScene?.TestUnlockAll();   // Village1 BtnGroup 재생성 + 하단탭 갱신
        var p = Managers.Game.Player;
        if (p != null) p.RefreshStats();

        log.AppendLine("════ 완료 ════");
        log.AppendLine("이제 게임에서 모든 탭/팝업/연출을 확인할 수 있습니다.");
        log.AppendLine("개별 슬라이드는 [Test/Quest Verify/Slide ##] 메뉴로 즉시 열 수 있습니다.");
        Debug.Log(log.ToString());
    }

    static void SeedMinMon(int dataId, int killCount, int caught, int atkG, int asG, int critG)
    {
        Managers.MinMon.DebugSetKillCount(dataId, killCount);
        var save = Managers.MinMon.GetSaveData(dataId);
        save.CaughtCount = caught;
        save.TameCount   = caught;
        Managers.MinMon.DebugSetGrade(dataId, 0, atkG);
        Managers.MinMon.DebugSetGrade(dataId, 1, asG);
        Managers.MinMon.DebugSetGrade(dataId, 2, critG);
        if (!Managers.Game.DiscoveredMonsterIds.Contains(dataId))
            Managers.Game.DiscoveredMonsterIds.Add(dataId);
        // 공명도 100% (caught>=4)일 때만 고유기술 자동 학습
        if (Managers.MinMon.GetSyncRate(dataId) >= 1f)
        {
            int lv = 1 + (Managers.MinMon.IsAllSS(dataId) ? 1 : 0);
            Managers.Ability.LearnMinionUniqueSkill(dataId, lv);
        }
    }

    // ─────────────────────────────────────────────────────────
    // C. 슬라이드별 시연 — 클릭 한 번에 해당 UI를 열고 기획서 체크리스트를 콘솔에 출력
    // ─────────────────────────────────────────────────────────

    // ─── part1 ────────────────────────────────────────────────
    [MenuItem("Test/Quest Verify/part1/Slide 6 - 재화 BAR 증감 (Gold +500)", false, 100)]
    static void S06_Currency() { if (!Require()) return; Managers.Game.Gold += 500; Spec("슬6", "Gold +500 → 초록 플래시 + Lerp + Outline FFF600. ##.####%는 EXP."); }

    [MenuItem("Test/Quest Verify/part1/Slide 7 - Player BAR (HP -30)", false, 101)]
    static void S07_PlayerBar()
    {
        if (!Require()) return;
        var p = Managers.Game.Player;
        if (p == null) { Debug.LogWarning("[Slide7] Battle 진입 후 다시 호출"); return; }
        p.OnDamaged(30f);
        Spec("슬7", "HP Bar 빨강 플래시 0.17s × 2, 직업='여행자', AdvRank='5' 표시 확인. EXP '##.####%' 포맷.");
    }

    [MenuItem("Test/Quest Verify/part1/Slide 9 - 환경설정 Popup %#1", false, 102)]
    static void S09_Settings() { if (!Require()) return; Managers.UI.ShowPopupUI<UI_SettingPopup>(); Spec("슬9", "BlackBG fade-in 0.3s + Move y350 OutBack 0.5s. 언어 토글 fx_glow on/off."); }

    [MenuItem("Test/Quest Verify/part1/Slide 11~14 - 상태정보 Popup %#2", false, 103)]
    static void S11_Character() { if (!Require()) return; Managers.UI.ShowPopupUI<UI_CharacterPopup>(); Spec("슬11/14", "Move From y-120 OutBack 0.5s. 능력치 14종(물공/마공/치명타/명중/공속/최대HP/방어/물저/마저/회피/회복/성장/획득). 치명타 100% 초과 표기 허용."); }

    [MenuItem("Test/Quest Verify/part1/Slide 15 - 증가율/증폭률 확장 UI", false, 104)]
    static void S15_RateAmp() { if (!Require()) return; Managers.UI.ShowPopupUI<UI_CharacterPopup>(); Spec("슬15", "물공/마공/최대HP/방어 박스 클릭 → 확장. UIWidthHeight 0.3s Linear To(60,46), 텍스트 Fade delay 0.1/0.2/0.3."); }

    [MenuItem("Test/Quest Verify/part1/Slide 16 - 능력점수 분배(SP=+5)", false, 105)]
    static void S16_StatPoint() { if (!Require()) return; Managers.Game.StatusPoint += 5; Managers.UI.ShowPopupUI<UI_CharacterPopup>(); Spec("슬16", "< > V 되돌리기 버튼. 변경 텍스트 색 00FF00. SP=0인데 > 클릭 시 B70000 + Shake 0.3s."); }

    [MenuItem("Test/Quest Verify/part1/Slide 19 - 소지품 Popup %#3", false, 106)]
    static void S19_Inventory() { if (!Require()) return; Managers.UI.ShowPopupUI<UI_InventoryPopup>().SetInfo(); Spec("슬19", "16칸 + 잠긴칸. Glow 색: 빈칸 FFFFFF40, 사용중 등급색, 잠금 0F0F0F. NEW 표시 = '새로운 종류' 획득 시."); }

    [MenuItem("Test/Quest Verify/part1/Slide 20 - 귀중품 (인벤토리 ImportantBag 패널)", false, 107)]
    static void S20_Requisite() { if (!Require()) return; Managers.UI.ShowPopupUI<UI_InventoryPopup>(); Spec("슬20", "✅ 귀중품 = 인벤토리 내 [ImportantBag] 패널(가방 버튼). Open Anim Scale 0.15 OutQuad FROM(0,1,0) 슬20 일치, RequisiteItemDic 연동, UI_ImportantSlot(shop_frame_glow 미사용), 획득순서 좌→우."); }

    [MenuItem("Test/Quest Verify/part1/Slide 21 - 아이템 상세 Popup", false, 108)]
    static void S21_ItemDetail() { if (!Require()) return; Managers.UI.ShowPopupUI<UI_ItemPopup>().SetInfo(10001); Spec("슬21", "Move From y-120 OutBack 0.5s. Layer4=등급색, Glow=등급색, FoundAtText / RarityText 표기."); }

    [MenuItem("Test/Quest Verify/part1/Slide 24 - Quest System 진행도 (Quest=3)", false, 109)]
    static void S24_QuestSystem() { if (!Require()) return; Managers.Quest.JumpToQuest(3); Spec("슬22~24", "Quest 3 (1급모험가퀘, 젤리 퐁 0/2 follow UI). 보유량 색: 0→FF0000, 진행→FEA400, 완료→00FF00."); }

    [MenuItem("Test/Quest Verify/part1/Slide 25~29 - 마을 MID UI (Village 재진입)", false, 110)]
    static void S25_VillageMid() { if (!Require()) return; GameScene?.EnterAreaWithFade(AreaType.Village1); Spec("슬25~29", "TYPE1/TYPE2 버튼 그룹. 1번 → 0.7s wait → 2번 순차 Active. 장소 Text Fade 0.4s/대기0.15s/Fade-in 0.5s."); }

    [MenuItem("Test/Quest Verify/part1/Slide 30 - 모험가협회 (Guild)", false, 111)]
    static void S30_Guild()
    {
        if (!Require()) return;
        var grp = Object.FindObjectOfType<UI_Village1BtnGroup>();
        if (grp == null) { Debug.LogWarning("[Slide30] Village1 진입 후 호출"); return; }
        typeof(UI_Village1BtnGroup).GetMethod("OnClickGuild", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.Invoke(grp, null);
        Spec("슬30", "Background_Guild + guild_npc_idle. [임무] 버튼이 메인 퀘 진행. Quest=1 일 때만 [대화] 진행.");
    }

    [MenuItem("Test/Quest Verify/part1/Slide 31~32 - 대화창 (dialogue popup)", false, 112)]
    static void S31_Dialogue()
    {
        if (!Require()) return;
        Managers.UI.ShowPopupUI<UI_DialoguePopup>();
        Spec("슬31~32", "STEP1 Gradient+Sprite fade-in → 0.5s 후 dia_box2 → 0.2s 후 캐릭터 Sprite + 텍스트 타이핑 → Arrow_down 활성.");
    }

    [MenuItem("Test/Quest Verify/part1/Slide 33 - 연금공방 진입 %#4", false, 113)]
    static void S33_Alchemy()
    {
        if (!Require()) return;
        var grp = Object.FindObjectOfType<UI_Village1BtnGroup>();
        if (grp == null) { Debug.LogWarning("[Slide33] Village1 진입 후 호출"); return; }
        typeof(UI_Village1BtnGroup).GetMethod("OnClickAlchemy", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.Invoke(grp, null);
        Spec("슬33", "Background_Alchemy + Alchemist_idle. CommissionText '제작의뢰' 버튼.");
    }

    [MenuItem("Test/Quest Verify/part1/Slide 34~36 - 제작의뢰 (CustomOrder)", false, 114)]
    static void S34_CustomOrder() { if (!Require()) return; Managers.UI.ShowPopupUI<UI_CustomOrderPopup>().SetInfo(); Spec("슬34~36", "[물약]/[기술] 버튼 클릭→F5DD6C색+Move To y-40. 잠긴건 SpriteNone+Lock(180/80alpha)."); }

    [MenuItem("Test/Quest Verify/part1/Slide 37~40 - 물약 상세 (HP Step2)", false, 115)]
    static void S37_PotionDetail()
    {
        if (!Require()) return;
        if (Object.FindObjectOfType<UI_CustomOrderPopup>() == null)
            Managers.UI.ShowPopupUI<UI_CustomOrderPopup>().SetInfo();
        EditorApplication.delayCall += () => EditorApplication.delayCall += () =>
            Managers.UI.ShowPopupUI<UI_PotionDetailPopup>().SetInfo(30002, fromCustomOrder: true);
        Spec("슬37~40", "[설명]↔[필요재료] 토글, [다음단계 정보] 점멸+능력치 색(▲=00FF00 ▼=FF0000), [제작] Shake1.1s + Punch0.3s + FX_splash_spark.");
    }

    [MenuItem("Test/Quest Verify/part1/Slide 41~42 - 물약창 (자동물약) %#5", false, 116)]
    static void S41_PotionPopup() { if (!Require()) return; Managers.UI.ShowPopupUI<UI_PotionPopup>().SetInfo(); Spec("슬41~42", "자동물약 토글, 사용 시 위→아래 Slider 쿨다운, Finish Anim(SpriteNone alpha80 fade 0.3s InBack)."); }

    [MenuItem("Test/Quest Verify/part1/Slide 43~46 - 여정 Popup (Travel)", false, 117)]
    static void S43_Travel() { if (!Require()) return; Managers.UI.ShowPopupUI<UI_TravelPopup>(); Spec("슬43~46", "대분류 → [마을]/[탐험] → 중/소분류 Layer Move From y-150 OutQuad 0.3s. 행선지 색 FFA300, 그 외 5E5E5E."); }

    // ─── part2 (도감·미니몬·능력) ─────────────────────────────────
    [MenuItem("Test/Quest Verify/part2/Slide 1~6 - 도감 (Bestiary) %#6", false, 200)]
    static void P2_S01_Bestiary() { if (!Require()) return; Managers.UI.ShowPopupUI<UI_BestiaryPopup>().SetInfo(); Spec("슬1~6", "Move From y-120 OutBack 0.5s. 4스킬 (종초월/조련사/오다주웠/기억확장) +n등급 표시. 잠긴건 Interactive X. 별 단계 Punch/Rotation z=20 Vib=2 Loop=-1 항시 재생."); }

    [MenuItem("Test/Quest Verify/part2/Slide 5 - 별등급 상승 연출(2001 → ★3)", false, 201)]
    static void P2_S05_GradeUp()
    {
        if (!Require()) return;
        var popup = Object.FindObjectOfType<UI_BestiaryPopup>() ?? Managers.UI.ShowPopupUI<UI_BestiaryPopup>();
        popup.SetInfo();
        EditorApplication.delayCall += () => EditorApplication.delayCall += () =>
            (Object.FindObjectOfType<UI_BestiaryPopup>())?.PlayGradeUp(2001, 3);
        Spec("슬5", "★3(보라) 풀시퀀스: 기술→미니몬→완성도별→숫자 0.1s 간격 Punch/Scale. Flash_USETHIS 적용. 미해금 기술 Lv0→Lv1이면 자물쇠/검정 SpriteNone Active false + NEW.");
    }

    [MenuItem("Test/Quest Verify/part2/Slide 7~14 - 미니몬 기록 Detail (2001)", false, 202)]
    static void P2_S07_Detail() { if (!Require()) return; Managers.UI.ShowPopupUI<UI_MonsterDetailPopup>().SetInfo(2001); Spec("슬7~14", "기록도 단계: 0(검정/???/Interactive X) → 60(색복귀/스탯) → 100(전리품 해금). 공명도: 0/50/100, 100%이상=고유기술 학습. AllSS=완벽획득(+2등급)."); }

    [MenuItem("Test/Quest Verify/part2/Slide 9 - 기록도 단계 Cycle (2001)", false, 203)]
    static void P2_S09_RecordCycle()
    {
        if (!Require()) return;
        int cur = UI_BestiaryPopup.GetRecordRate(2001);
        int next = cur <= 0 ? 60 : cur < 100 ? 100 : 0;
        UI_BestiaryPopup.SetRecordRate(2001, next);
        var p = Object.FindObjectOfType<UI_MonsterDetailPopup>();
        if (p != null) p.SetInfo(2001); else Managers.UI.ShowPopupUI<UI_MonsterDetailPopup>().SetInfo(2001);
        Spec("슬9", $"기록도 {cur}→{next}%. 100% 미만 EC9907 / 100% 00FF00.");
    }

    [MenuItem("Test/Quest Verify/part2/Slide 10 - 공명도 단계 Cycle (2001)", false, 204)]
    static void P2_S10_SyncCycle()
    {
        if (!Require()) return;
        int cur = UI_BestiaryPopup.GetSync(2001);
        int next = cur <= 0 ? 50 : cur < 100 ? 100 : 0;
        UI_BestiaryPopup.SetSync(2001, next);
        var p = Object.FindObjectOfType<UI_MonsterDetailPopup>();
        if (p != null) p.SetInfo(2001); else Managers.UI.ShowPopupUI<UI_MonsterDetailPopup>().SetInfo(2001);
        Spec("슬10", $"공명도 {cur}→{next}%. 100% 달성 시 능력 6001 영구 학습 (현재 학습={Managers.Ability.IsLearned(6001)}).");
    }

    [MenuItem("Test/Quest Verify/part2/Slide 12 - 나의 미니몬 버튼 (포획 유무)", false, 205)]
    static void P2_S12_OwnedBtn() { if (!Require()) return; Managers.UI.ShowPopupUI<UI_MonsterDetailPopup>().SetInfo(2001); Spec("슬12", "포획O = MyMinimon 버튼 활성 (shop_frame_glow 8E5E00). 포획X = T_gray_button_s + RequireMinimonText, Interactive X."); }

    [MenuItem("Test/Quest Verify/part2/Slide 13 - 동행 토글 (2001 follower)", false, 206)]
    static void P2_S13_Follower()
    {
        if (!Require()) return;
        int cur = Managers.Game.CompanionMinMonId;
        if (cur == 2001) Managers.MinMon.RemoveCompanion();
        else Managers.MinMon.SetCompanion(2001);
        Managers.UI.ShowPopupUI<UI_MonsterDetailPopup>().SetInfo(2001);
        Spec("슬13", $"Companion {cur} → {Managers.Game.CompanionMinMonId}. 동행 시 FollowingText 하단UI + Active Ef. (현재 Active Ef prefab 부재로 데이터만)");
    }

    [MenuItem("Test/Quest Verify/part2/Slide 15~18 - 나의 미니몬 Popup (2001)", false, 207)]
    static void P2_S15_MyMinimon() { if (!Require()) return; Managers.UI.ShowPopupUI<UI_MinionInfoPopup>().SetInfo(2001); Spec("슬15~18", "Move From y-120 OutBack 0.5s. 능력치 6등급 outline색(0=B900FF~5=000000 + Text_ShineGold). 5등급=UIMask FFF300 ON."); }

    [MenuItem("Test/Quest Verify/part2/Slide 18 - 능력치 5등급(S) AllSS (2001)", false, 208)]
    static void P2_S18_AllSS()
    {
        if (!Require()) return;
        Managers.MinMon.DebugSetGrade(2001, 0, 5);
        Managers.MinMon.DebugSetGrade(2001, 1, 5);
        Managers.MinMon.DebugSetGrade(2001, 2, 5);
        Managers.UI.ShowPopupUI<UI_MinionInfoPopup>().SetInfo(2001);
        Spec("슬18", $"AllSS={Managers.MinMon.IsAllSS(2001)}, 별={Managers.MinMon.GetStarGrade(2001)}. 6001 능력 Lv.{Managers.Ability.GetLevel(6001)} (공명100% & AllSS → Lv.2)");
    }

    [MenuItem("Test/Quest Verify/part2/Slide 20 - 첫 포획 NEW + 능력치 롤 (2002 Reset)", false, 209)]
    static void P2_S20_FirstCatch()
    {
        if (!Require()) return;
        var save = Managers.MinMon.GetSaveData(2002);
        save.CaughtCount = 0; save.TameCount = 0; save.AtkGrade = save.AtkSpeedGrade = save.CritGrade = -1;
        Managers.Game.InterestedMinMons.Remove(2002);
        Managers.MinMon.AddInterest(2002);
        Managers.MinMon.TryCatch(2002, forceSuccess: true);
        var info20 = Managers.UI.ShowPopupUI<UI_MinionInfoPopup>();
        info20.SetInfo(2002);
        info20.PlayCatchResult(Managers.MinMon.LastCatchWasFirst, Managers.MinMon.LastCatchImproved);
        Spec("슬20", $"첫 포획: NEW 표시 + 창 뒤 GlowRays(회전+페이드) + 공명도 0→25% lerp + 능력치 랜덤 롤(40/30/15/8/5/2%). 결과: ATK={save.AtkGrade} AS={save.AtkSpeedGrade} CRIT={save.CritGrade}");
    }

    // 슬21: 이미 보유한 미니몬을 또 잡았을 때 "능력치 향상될 때". 낮은 등급으로 세팅 후 향상될 때까지 재시도(결정적).
    [MenuItem("Test/Quest Verify/part2/Slide 21 - 재포획 능력치 향상 (2001)", false, 210)]
    static void P2_S21_ReCatchImproved()
    {
        if (!Require()) return;
        var mm = Managers.MinMon;
        var save = mm.GetSaveData(2001);
        // 향상될 때까지 재시도: 매번 낮은 등급+공명도 2회로 리셋 후 재포획
        bool ok = false;
        for (int i = 0; i < 30; i++)
        {
            mm.DebugSetGrade(2001, 0, 0); mm.DebugSetGrade(2001, 1, 0); mm.DebugSetGrade(2001, 2, 0);
            save.CaughtCount = 2; save.TameCount = Mathf.Max(save.TameCount, 2);
            mm.TryCatch(2001, forceSuccess: true);
            if (mm.LastCatchImproved) { ok = true; break; }
        }
        var info = Managers.UI.ShowPopupUI<UI_MinionInfoPopup>();
        info.SetInfo(2001);
        info.PlayCatchResult(mm.LastCatchWasFirst, mm.LastCatchImproved);
        Spec("슬21", $"재포획 향상={ok}. ATK {mm.LastAtkGradePrev}->{mm.GetAtkGrade(2001)}({(mm.LastAtkImproved?"↑":"-")}) AS {mm.LastASGradePrev}->{mm.GetAtkSpeedGrade(2001)}({(mm.LastASImproved?"↑":"-")}) CRIT {mm.LastCritGradePrev}->{mm.GetCritGrade(2001)}({(mm.LastCritImproved?"↑":"-")}). 향상된 스탯만 Flip+Punch+VFX+NEW, 1초뒤 ATK→AS→CRIT 순. 모든 anim 끝나야 닫기 가능.");
    }

    // 슬22: 이미 보유한 미니몬을 또 잡았을 때 "능력치 향상되지 않을 때". 5/5/5(최대)로 막아 무조건 향상없음.
    [MenuItem("Test/Quest Verify/part2/Slide 22 - 재포획 향상 없음 (2001)", false, 211)]
    static void P2_S22_ReCatchNoImprove()
    {
        if (!Require()) return;
        var mm = Managers.MinMon;
        mm.DebugSetGrade(2001, 0, 5); mm.DebugSetGrade(2001, 1, 5); mm.DebugSetGrade(2001, 2, 5);
        mm.TryCatch(2001, forceSuccess: true);
        var info = Managers.UI.ShowPopupUI<UI_MinionInfoPopup>();
        info.SetInfo(2001);
        info.PlayCatchResult(mm.LastCatchWasFirst, mm.LastCatchImproved);
        Spec("슬22", $"재포획 향상={mm.LastCatchImproved}(false 기대). NEW/VFX 미등장, 호출 1.5초뒤 MinimonGrowthFailText(\"성장하지 않았네요\") FadeIn → 0.5초뒤 FullGrowthInText FadeIn. 모든 anim 끝나야 닫기 가능.");
    }

    [MenuItem("Test/Quest Verify/part2/Slide 23 - 관심 알림 (Side Popup 성공)", false, 211)]
    static void P2_S23_Attract() { if (!Require()) return; Managers.UI.ShowAttractedPopup(2001, storageFull: false); Spec("슬23~24", "Move From x+150 OutExpo 0.5s → 3s 유지 → DoPlayBackwards. Layer 색: 파랑/핑크 (성공). 몬스터 머리 위 Emoj_Shy 동시 (현재 prefab 부재)."); }

    [MenuItem("Test/Quest Verify/part2/Slide 25 - 관심 창고가득 경고 (UI_WarningPopup 빨강)", false, 212)]
    static void P2_S25_WarningFull() { if (!Require()) return; Managers.UI.ShowWarningPopup(2002); Spec("슬25", "UI_WarningPopup: Layer1~3 전부 B70000, 우측에서 슬라이드인(rest 27,45) → 3초 → 슬라이드아웃. Text='CreatureName + MinimonInterestFullText'. (라이브 사냥 StorageFull 경로와 동일)"); }

    // 슬26: 저장칸 여유 있음 → 도감버튼 NEW + GlowDot_01 + Punch/Scale, count 흰색
    [MenuItem("Test/Quest Verify/part2/Slide 26 - 도감버튼 저장칸 여유(count 흰색)", false, 213)]
    static void P2_S26_BottomUI_Room()
    {
        if (!Require()) return;
        Managers.Game.InterestedMinMons.Clear();
        Managers.MinMon.AddInterest(2001);   // 1/2 → 여유
        Spec("슬26", $"저장칸 여유(1/{Managers.MinMon.InterestCapacity}): 도감버튼 NEW Feedback + slime_icon 중앙 GlowDot_01 + Punch/Scale(0.3,Vib10,Elas1), count='1' 흰색. (관심 보일 때마다 Anim 재생)");
    }

    // 슬27: 저장칸 가득 → 하단 UI 변경 (count 빨강 FF0000, 버튼 FA5959, 박스 D14A4A), 가득 후 추가 관심은 Anim 없음
    [MenuItem("Test/Quest Verify/part2/Slide 27 - 도감버튼 저장칸 가득(count 빨강)", false, 214)]
    static void P2_S27_BottomUI_Full()
    {
        if (!Require()) return;
        Managers.Game.InterestedMinMons.Clear();
        Managers.MinMon.AddInterest(2001);
        Managers.MinMon.AddInterest(2002);   // 2/2 → 가득
        Spec("슬27", $"저장칸 가득({Managers.MinMon.InterestedMinMons.Count}/{Managers.MinMon.InterestCapacity}): count 빨강(FF0000) + 버튼 FA5959 + 박스 D14A4A. 가득 후 추가 관심은 Anim 없이 경고팝업(슬25). HasRoom={Managers.MinMon.HasInterestRoom}");
    }

    // 슬28~29: 잡기 버튼 — 저장칸에 미니몬 있을 때 (포획 가능 창고 1+)
    [MenuItem("Test/Quest Verify/part2/Slide 28~29 - 잡기버튼 저장칸 있음(!/슬라임)", false, 214)]
    static void P2_S28_TameButton_Has()
    {
        if (!Require()) return;
        foreach (var p in Object.FindObjectsOfType<UI_BestiaryPopup>()) Managers.UI.ClosePopupUI();
        Managers.Game.InterestedMinMons.Clear();
        Managers.MinMon.AddInterest(2001);   // 저장칸 1마리
        Managers.UI.ShowPopupUI<UI_BestiaryPopup>().SetInfo();
        Spec("슬28~29 (있음)", "잡기버튼: 타이틀 '미니몬 잡기!' + 슬라임 표시 + Emoj_Exclamation + Interactive O. slide-in은 prefab DOTweenAnimation(Frame 0s/Slime·Player 0.4s/Amount 0.8s, Move isFrom OutBack autoPlay).");
    }

    // 슬28~29: 잡기 버튼 — 저장칸 비어있을 때 (Emoj_Question, slime 자식이라 GO active 유지 필요)
    [MenuItem("Test/Quest Verify/part2/Slide 28~29 - 잡기버튼 저장칸 없음(?/물음표)", false, 215)]
    static void P2_S28_TameButton_Empty()
    {
        if (!Require()) return;
        foreach (var p in Object.FindObjectsOfType<UI_BestiaryPopup>()) Managers.UI.ClosePopupUI();
        Managers.Game.InterestedMinMons.Clear();   // 저장칸 0
        Managers.UI.ShowPopupUI<UI_BestiaryPopup>().SetInfo();
        Spec("슬28~29 (없음)", "잡기버튼: 타이틀 '미니몬 잡기?' + 슬라임 Image 숨김(GO는 active) + 슬라임 위치에 Emoj_Question + Interactive X.");
    }

    // 공통: 2001을 창고에 넣고 TamePopup 진입
    static UI_TamePopup OpenTame()
    {
        if (!Managers.Game.InterestedMinMons.Contains(2001)) Managers.MinMon.AddInterest(2001);
        var p = Managers.UI.ShowPopupUI<UI_TamePopup>(); p.SetInfo(2001); return p;
    }

    // 슬30~34: 잡기팝업 진입 — Popup Anim→Opening 깜빡임→Loading Type1/2→Setup 슬라이드인(Grayscale)→Emoj_Exclamation→대사→버튼 순차
    [MenuItem("Test/Quest Verify/part2/Slide 30~34 - 잡기팝업 진입(UI/로딩/Setup)", false, 215)]
    static void P2_S30_Enter()
    {
        if (!Require()) return;
        UI_TamePopup.DebugForceCatch = 0;
        OpenTame();
        Spec("슬30~34", "Popup Anim(y-120 OutBack)→Opening 깜빡임(DBDBDA↔7B7B7B Yoyo6)→Loading Type1/2 랜덤→Setup(플레이어/미니몬 화면밖→지정위치 1.5s, Grayscale 도착후 컬러)→미니몬 Emoj_Exclamation→대사 타이핑→[길들인다]→0.5s→[가랏]→0.5s→[나의미니몬] 순차 등장 후 상호작용.");
    }

    // 슬35~36: 길들이기 성공 — 팝업 진입 후 [길들인다!] 클릭 시 강제 성공
    [MenuItem("Test/Quest Verify/part2/Slide 35~36 - 길들이기 성공(강제)", false, 216)]
    static void P2_S35_LureSuccess()
    {
        if (!Require()) return;
        UI_TamePopup.DebugForceCatch = 1;   // 다음 Lure 결과=성공
        OpenTame();
        Spec("슬35~36 성공", "팝업 뜨면 [길들인다!] 클릭 → Lure Anim(Y Yoyo4)→Emoj_Lure→Shake→효과(Emoj_Excited→Dots)→성공: Caught Anim(Move Y Yoyo 0.18×4)→Emoj_TwoHearts→Emoj_Happy→GlowRays [나의 미니몬] 팝업.");
    }

    // 슬35~36: 길들이기 실패 — 팝업 진입 후 [길들인다!] 클릭 시 강제 실패
    [MenuItem("Test/Quest Verify/part2/Slide 35~36 - 길들이기 실패(강제)", false, 217)]
    static void P2_S35_LureFail()
    {
        if (!Require()) return;
        UI_TamePopup.DebugForceCatch = 2;   // 다음 Lure 결과=실패
        OpenTame();
        Spec("슬35~36 실패", "팝업 뜨면 [길들인다!] 클릭 → ...→실패: Emoj_Sweat + 미니몬 좌우반전(0.8s) + 플레이어 Emoj_Sweat2 + 도망 대사 → 0.3s 뒤 Runaway Anim(Move InBack 0.5s X 오른쪽 화면밖) → Popup Close.");
    }

    // 슬37: 만드라고라 확정 포획 — 만드라고라 보유 보장 후 진입, [가랏! 만드라고라!] 클릭
    [MenuItem("Test/Quest Verify/part2/Slide 37 - 만드라고라 확정포획", false, 218)]
    static void P2_S37_Mandrake()
    {
        if (!Require()) return;
        if (Managers.Game.MandrakeCount < 1) Managers.Game.MandrakeCount = 3;
        UI_TamePopup.DebugForceCatch = 0;
        OpenTame();
        Spec("슬37", $"만드라고라 보유={Managers.Game.MandrakeCount}. 팝업 뜨면 [가랏! 만드라고라!] 클릭 → 순서2 대사→1s→포물선 Happy Mandrake(0.67s)→도착 scale1.5/alpha0(0.33s)→Emoj_Exclamation+Jump(Y Yoyo 0.15×2)→Eat Mandrake(Scale Yoyo 0.05×24 TO1.1)→Emoj_Shy+순서3 대사→1.2s→[잡기 성공]과 동일.");
    }

    [MenuItem("Test/Quest Verify/part2/Slide 38~42 - 기술책 (능력 목록) %#7", false, 216)]
    static void P2_S38_SkillBook() { if (!Require()) return; Managers.UI.ShowPopupUI<UI_SkillBookPopup>().SetInfo(); Spec("슬38~42", $"기억력 {Managers.Ability.UsedMemory}/{Managers.Ability.MaxMemory}. 슬롯 7칸 + 필터(전체/재능/기억중/미기억/잠금). 색: 0/Red, used<max/Yellow, used=max/Lime. 재능(MemoryCapacity=0)은 항시 적용·망각 불가."); }

    [MenuItem("Test/Quest Verify/part2/Slide 40 - 기억력 색 변화 (Memory full)", false, 217)]
    static void P2_S40_MemoryFull()
    {
        if (!Require()) return;
        // 5002(3) + 5003(2) + 5004(4) = 9, + 6001(2) = 11 → MaxMem 보너스로 max 채우기 시도
        Managers.Ability.Learn(5002); Managers.Ability.Memorize(5002);
        Managers.Ability.Learn(5003); Managers.Ability.Memorize(5003);
        Managers.Ability.Learn(5004); Managers.Ability.Memorize(5004);
        if (Managers.Ability.IsLearned(6001)) Managers.Ability.Memorize(6001);
        Managers.UI.ShowPopupUI<UI_SkillBookPopup>().SetInfo();
        Spec("슬40", $"기억력 채움 시도. Used={Managers.Ability.UsedMemory} Max={Managers.Ability.MaxMemory}. Used==Max → Lime색.");
    }

    // 슬44: 능력 종류 2/3/4가지 — 능력 아이콘 왼쪽에 역할 아이콘+BtmText 겹침 배치 검증.
    [MenuItem("Test/Quest Verify/part2/Slide 44 - 종류 2가지 (이중의 기억 5014)", false, 218)]
    static void P2_S44_Two() { if (!Require()) return; Managers.Ability.Learn(5014); Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(5014); Spec("슬44(2종)", "능력 아이콘 왼쪽 [기술][마법] 가로 1줄 중앙. 아이콘 크게+텍스트 겹침."); }

    [MenuItem("Test/Quest Verify/part2/Slide 44 - 종류 3가지 (만능의 기억 5013)", false, 219)]
    static void P2_S44_Three() { if (!Require()) return; Managers.Ability.Learn(5013); Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(5013); Spec("슬44(3종)", "[기술][방어] 윗줄 + [지원] 아랫줄 가운데."); }

    [MenuItem("Test/Quest Verify/part2/Slide 44 - 종류 4가지 (전능의 기억 5015)", false, 220)]
    static void P2_S44_Four() { if (!Require()) return; Managers.Ability.Learn(5015); Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(5015); Spec("슬44(4종)", "[기술][마법] 윗줄 + [방어][지원] 아랫줄 = 2x2."); }

    // 슬46: 설명 텍스트 양에 따라 정보창 세로 사이즈 자동 변화 — 짧음(5002, 1줄) vs 김(5008, 5줄) 비교.
    [MenuItem("Test/Quest Verify/part2/Slide 46 - 긴 설명 auto-size (5008)", false, 221)]
    static void P2_S46_AutoSizeLong() { if (!Require()) return; Managers.Ability.Learn(5008); Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(5008); Spec("슬46(긴설명)", "설명 5줄 → 팝업 세로 확장. 상단(이름/아이콘) 고정, 출처/버튼은 아래로 밀림(겹침 X)."); }

    [MenuItem("Test/Quest Verify/part2/Slide 46 - 짧은 설명 auto-size (5002)", false, 222)]
    static void P2_S46_AutoSizeShort() { if (!Require()) return; Managers.Ability.Learn(5002); Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(5002); Spec("슬46(짧은설명)", "설명 1줄 → 팝업 기본(짧은) 세로. 5008과 번갈아 열어 높이 차이 확인."); }

    // 슬47: 능력 정보창 #5 하단 UI 전체정보 — 출처 5종(전직/미니몬/장비/NPC재료/잠재력) 헤더 TEXT·색 + 박스 HLG 가운데정렬.
    [MenuItem("Test/Quest Verify/part2/Slide 47 - 출처: 전직 (5004)", false, 223)]
    static void P2_S47_Class() { if (!Require()) return; Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(5004); Spec("슬47(전직)", "상단 '직업 기술' #3DC4FF + 하단 직업명 '여행자' #F3FF00."); }

    [MenuItem("Test/Quest Verify/part2/Slide 47 - 출처: 미니몬 (6001)", false, 224)]
    static void P2_S47_Minion() { if (!Require()) return; Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(6001); Spec("슬47(미니몬)", "상단 '미니몬 필요' #ECCC6C + 미니몬 박스 가운데 정렬."); }

    [MenuItem("Test/Quest Verify/part2/Slide 47 - 출처: 장비 (5002)", false, 225)]
    static void P2_S47_Equip() { if (!Require()) return; Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(5002); Spec("슬47(장비)", "상단 '장비 필요' #902EC8 + 장비 박스 가운데 정렬."); }

    [MenuItem("Test/Quest Verify/part2/Slide 47 - 출처: NPC재료 (5003)", false, 226)]
    static void P2_S47_Npc() { if (!Require()) return; Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(5003); Spec("슬47(NPC재료)", "상단 '필요 재료' #8CEC6C + 재료 박스 가운데 정렬."); }

    [MenuItem("Test/Quest Verify/part2/Slide 47 - 출처: 잠재력 (5001)", false, 227)]
    static void P2_S47_Unlimited() { if (!Require()) return; Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(5001); Spec("슬47(잠재력)", "상단 '무한한 잠재력' #EC706C + 하단 설명 #C8C8C8 (3줄)."); }

    // 슬48: 능력 정보창 #6 하단 UI 필요 미니몬 — 기록도 색/체크 ICON 2개(기록100%·올SS)/최상등급/잠금.
    [MenuItem("Test/Quest Verify/part2/Slide 48 - 필요 미니몬: 완성 (6001)", false, 228)]
    static void P2_S48_MinionFull() { if (!Require()) return; Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(6001); Spec("슬48(완성)", "기록도 #00FF00 + 체크 2개 초록(0AFF00) + '최상등급' 초록. 미니몬 이미지=도감 IconLabel(미프리로드면 흰색)."); }

    [MenuItem("Test/Quest Verify/part2/Slide 48 - 필요 미니몬: 부분 (6002)", false, 229)]
    static void P2_S48_MinionPartial() { if (!Require()) return; Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(6002); Spec("슬48(부분)", "기록도<100 red/orange + 체크 2개 회색(BEBEBE) + '최상등급' 회색(909090). 1단계도 동일 문구."); }

    // 슬49: 능력 정보창 #7 하단 UI 필요 장비 — 장비 ICON + 체크 ICON 2개(강화최대·보유&내구최대) + 최대강화/최대내구도.
    [MenuItem("Test/Quest Verify/part2/Slide 49 - 필요 장비: 미보유 (5002)", false, 230)]
    static void P2_S49_EquipNone() { if (!Require()) return; Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(5002); Spec("슬49(미보유)", "장비 ICON(Sword01) + 체크 2개 회색(BEBEBE) + '최대 강화'/'최대 내구도' 회색(909090)."); }

    [MenuItem("Test/Quest Verify/part2/Slide 49 - 필요 장비: 풀강+풀내구 (5002)", false, 231)]
    static void P2_S49_EquipMax()
    {
        if (!Require()) return;
        var list = Managers.Game.OwnedEquipments;
        var eq = list.Find(e => e.DataId == 30001);
        if (eq == null) { eq = new Equipment(); eq.DataId = 30001; list.Add(eq); }
        eq.EnhanceLevel = 9; eq.Durability = 100;
        Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(5002);
        Spec("슬49(풀강풀내구)", "장비 30001 보유+강화9+내구100 → 체크 2개 초록(0AFF00) + '최대 강화'/'최대 내구도' 초록(#00FF00).");
    }

    // 슬50: 능력 정보창 #8 하단 UI 필요 재료 — 아이템 ICON + 보유/필요 수량 색(0 red/부족 orange/충분 lime) + 최대치 '- 최다 등급 -'.
    [MenuItem("Test/Quest Verify/part2/Slide 50 - 필요 재료 (5003)", false, 232)]
    static void P2_S50_Material()
    {
        if (!Require()) return;
        // 최다등급 테스트가 5003 레벨을 max로 올려놨을 수 있으므로, 재료 박스가 보이도록 레벨을 낮춤(미만 = 박스 표시).
        if (Managers.Ability.IsLearned(5003)) Managers.Ability.SetLevel(5003, 1);
        Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(5003);
        Spec("슬50(재료)", "아이템 ICON(Slime_Green_Drop) + 보유/필요 수량(0 red/부족 orange/충분 #00FF00) + interactive O + 연금공방 장소.");
    }

    [MenuItem("Test/Quest Verify/part2/Slide 50 - 필요 재료: 최대 등급 (5003)", false, 233)]
    static void P2_S50_MaterialMax()
    {
        if (!Require()) return;
        if (!Managers.Ability.IsLearned(5003)) Managers.Ability.Learn(5003);
        int maxLv = Managers.Data.SkillDic.TryGetValue(5003, out var sd) ? System.Math.Max(1, sd.MaxLevel) : 1;
        Managers.Ability.SetLevel(5003, maxLv);
        Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(5003);
        Spec("슬50(최대등급)", "5003 최대레벨 → '- 최대 등급 -'(Ability_MaxLevel_Text) 표시 + 재료 박스 숨김.");
    }

    // 슬51: 능력 정보창 최하단 3버튼 — 버튼1(NPC장소,좌·글로우000000)/버튼2(다음등급,우·8E5E00)/버튼3(기억,중앙·0036A6).
    [MenuItem("Test/Quest Verify/part2/Slide 51 - 최하단 3버튼 (5003)", false, 234)]
    static void P2_S51_Buttons()
    {
        if (!Require()) return;
        if (!Managers.Ability.IsLearned(5003)) Managers.Ability.Learn(5003);
        Managers.Ability.SetLevel(5003, 1);   // 다음등급 존재(버튼2 표시)
        Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(5003);
        Spec("슬51(3버튼)", "좌:연금공방(글로우 검정) / 우:다음등급 정보(금색 8E5E00) / 중앙:기억하기(파랑 0036A6). '다음등급 정보' 클릭 시 등급 '1 ▶ 2'·수치 '+▶+'·버튼 '뒤로'로 변경.");
    }

    // 슬52: 버튼3 기억 상태 — 기억하기(파랑·O) / 기억중(파랑·O→잊기) / 기억력 부족(검정·X).
    [MenuItem("Test/Quest Verify/part2/Slide 52 - 기억중 (5003)", false, 235)]
    static void P2_S52_Memorized()
    {
        if (!Require()) return;
        if (!Managers.Ability.IsLearned(5003)) Managers.Ability.Learn(5003);
        Managers.Ability.SetLevel(5003, 1);
        if (!Managers.Ability.IsEquipped(5003)) Managers.Ability.Memorize(5003);
        var p = Managers.UI.ShowPopupUI<UI_SkillDetailPopup>(); p.SetInfo(5003);
        Spec("슬52(기억중)", "버튼3 '기억중'(파랑 0036A6) + 게이지 'used/max'(점유 -표기 없음, lime) + 아이콘 Active_Ef + 이름 Frame Material_Equip.");
    }

    [MenuItem("Test/Quest Verify/part2/Slide 52 - 기억력 부족", false, 236)]
    static void P2_S52_LackMemory()
    {
        if (!Require()) return;
        // 기억력을 거의 채워 free < cap 인 미기억 능력을 띄움.
        foreach (var id in new[] { 5002, 5004, 5005, 5007, 5008 }) { if (!Managers.Ability.IsLearned(id)) Managers.Ability.Learn(id); Managers.Ability.Memorize(id); }
        int target = -1;
        foreach (var kv in Managers.Data.SkillDic)
        {
            int id = kv.Key;
            if (Managers.Ability.IsLearned(id) && !Managers.Ability.IsTalent(id) && !Managers.Ability.IsEquipped(id) && Managers.Ability.FreeMemory < kv.Value.MemoryCapacity) { target = id; break; }
        }
        if (target < 0) target = 5007;
        Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(target);
        Spec("슬52(부족)", $"버튼3 '기억력 부족'(검정 000000·Interactive X) + 게이지 red(used(-cap)/max) + LackMemoryImg. (대상 {target})");
    }

    // ─────────────────────────────────────────────────────────
    // 빠른 진입 (전투/마을)
    // ─────────────────────────────────────────────────────────
    [MenuItem("Test/Quest Verify/Jump/Battle 진입 (전투씬) %#b", false, 300)]
    static void GoBattle() { if (!Require()) return; Managers.UI.CloseAllPopupUI(); GameScene?.EnterAreaWithFade(AreaType.Battle); }

    [MenuItem("Test/Quest Verify/Jump/Village1 진입 (마을) %#v", false, 301)]
    static void GoVillage() { if (!Require()) return; Managers.UI.CloseAllPopupUI(); GameScene?.EnterAreaWithFade(AreaType.Village1); }

    [MenuItem("Test/Quest Verify/Jump/Quest = 1 (시작)", false, 310)]
    static void Q1() { if (!Require()) return; Managers.Quest?.JumpToQuest(1); }

    [MenuItem("Test/Quest Verify/Jump/Quest = 6 (마지막)", false, 311)]
    static void Q6() { if (!Require()) return; Managers.Quest?.JumpToQuest(6); }

    // ─────────────────────────────────────────────────────────
    // 헬퍼
    // ─────────────────────────────────────────────────────────
    static void Spec(string slide, string spec)
    {
        Debug.Log($"<b>[기획서 {slide}]</b> {spec}");
    }
}
#endif
