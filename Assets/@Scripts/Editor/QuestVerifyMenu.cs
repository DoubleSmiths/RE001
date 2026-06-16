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

    [MenuItem("Test/Quest Verify/part2/Slide 25 - 관심 창고가득 경고 (Side Popup 빨강)", false, 212)]
    static void P2_S25_WarningFull() { if (!Require()) return; Managers.UI.ShowAttractedPopup(2002, storageFull: true); Spec("슬25", "Layer1~3 전부 B70000. 'CreatureName + MinimonInterestFullText'."); }

    [MenuItem("Test/Quest Verify/part2/Slide 26 - 도감 버튼 NEW + 카운트(슬18, 2002 관심 추가)", false, 213)]
    static void P2_S26_BottomUI()
    {
        if (!Require()) return;
        if (!Managers.Game.InterestedMinMons.Contains(2002))
            Managers.MinMon.AddInterest(2002);
        Spec("슬26~27", "도감 버튼 우상단에 slime_icon + 저장수 텍스트 + GlowDot_01. Punch/Scale 0.3 To(0.3,0.3) Vib10. 가득 시 색 D14A4A/FA5959.");
    }

    [MenuItem("Test/Quest Verify/part2/Slide 28~29 - 잡기 버튼 (slide-in 등장)", false, 214)]
    static void P2_S28_TameButton() { if (!Require()) return; Managers.UI.ShowPopupUI<UI_BestiaryPopup>().SetInfo(); Spec("슬28~29", "도감 열린 직후 0.4s 후 Frame 좌→/Slime 좌→/Title 우→ Move OutBack 0.5s, 0.8s 후 Emoj Active. 저장칸 0=Emoj_Question + Interactive X / 1+=Emoj_Exclamation + Interactive O."); }

    [MenuItem("Test/Quest Verify/part2/Slide 30~37 - 잡기 미니게임 (UI_TamePopup, 2001)", false, 215)]
    static void P2_S30_TameMinigame()
    {
        if (!Require()) return;
        if (!Managers.Game.InterestedMinMons.Contains(2001))
            Managers.MinMon.AddInterest(2001);
        Managers.UI.ShowPopupUI<UI_TamePopup>().SetInfo(2001);
        Spec("슬30~37", "Popup Anim→Loading Type1/2 랜덤 1.2s→Setup(Grayscale 1.5s)→Emoj_Exclamation→대사 타이핑→버튼 0.5s간격. Lure: Move Y Yoyo4 0.125s→Emoj_Lure→Shake→효과有/無 분기→결과(성공:Caught4Yoyo+Emoj_TwoHearts→MyMinimon / 실패:Sweat+Runaway). Mandrake: 포물선 0.67s→Jump 0.15Yoyo2→Eat 0.05Yoyo24→확정 성공.");
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
