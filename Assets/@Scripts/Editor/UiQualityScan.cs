using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
/// UI 품질 스캔 — 팝업 22종을 한국어·영어로 열어 보며 눈으로는 놓치기 쉬운 세 가지를 찾는다.
///
///   A. 폰트 섞임 : 글자·숫자가 주 폰트에 없어 기본 폰트(폴백)로 그려진다 → 한 단어 안에 두 서체.
///                 (전투 폰트가 93자 정적 아틀라스라 'Feral Fox' 의 F 만 다른 서체였다. 장식 기호는 제외)
///   B. 넘침 후보 : 글자 범위가 부모 칸보다 넓다. 앵커 역할의 작은 부모(폭 10 미만)는 건너뛴다.
///                 후보일 뿐이라 캡처로 확인한다(예: 'ABILITY DETAILS' 가 제목판을 넘었다).
///   C. 구독 누수 : 팝업을 세 번 열고 닫는 동안 매니저 이벤트 구독 수가 늘어난다.
///                 (여정 창이 열 때마다 언어 변경 구독 50개씩 쌓였다 — 비활성 패널 아래 만든 버튼)
///
/// 플레이 모드 GameScene 에서 실행. A·C 가 하나라도 있으면 에러로, B 는 목록만 남긴다.
/// 결과: persistentDataPath/ui_scan_result.txt (콘솔은 다른 로그에 밀린다).
/// </summary>
public static class UiQualityScan
{
    const int CYCLES = 3;
    const float MIN_PARENT_WIDTH = 10f;
    const float OVERFLOW_TOLERANCE = 2f;

    [MenuItem("Test/Integration/UI 품질 스캔 (폰트 섞임·넘침·구독 누수)", false, 7)]
    static void Run()
    {
        // ⚠️ DisplayDialog 금지 — unity-cli 로 메뉴를 부르면 아무도 확인을 누르지 않아 에디터가 멈춘다(FullSystemAudit 참고).
        if (!Application.isPlaying || Managers.Game?.Player == null)
        {
            Debug.LogError("[UI 스캔] 플레이 모드 GameScene 에서 실행하세요.");
            return;
        }
        Managers.Game.Player.StartCoroutine(CoRun());
    }

    static List<(string label, Action open)> Popups()
    {
        var equip = Managers.Game.OwnedEquipments.FirstOrDefault(e => e.IsEquipped) ?? Managers.Game.OwnedEquipments.FirstOrDefault();
        var shopItem = Managers.Data.ShopItemDic.Values.FirstOrDefault();
        var list = new List<(string, Action)>
        {
            ("hud", () => { }),
            ("bag", () => Managers.UI.ShowPopupUI<UI_InventoryPopup>()),
            ("bestiary", () => Managers.UI.ShowPopupUI<UI_BestiaryPopup>().SetInfo()),
            ("char", () => Managers.UI.ShowPopupUI<UI_CharacterPopup>()),
            ("class", () => Managers.UI.ShowPopupUI<UI_ClassPopup>().SetInfo()),
            ("equip", () => Managers.UI.ShowPopupUI<UI_EquipmentPopup>()),
            ("minion", () => Managers.UI.ShowPopupUI<UI_MinionInfoPopup>().SetInfo(2001)),
            ("monster", () => Managers.UI.ShowPopupUI<UI_MonsterDetailPopup>().SetInfo(2001)),
            ("potion", () => Managers.UI.ShowPopupUI<UI_PotionPopup>()),
            ("skill", () => Managers.UI.ShowPopupUI<UI_SkillDetailPopup>().SetInfo(5104)),
            ("skillbook", () => Managers.UI.ShowPopupUI<UI_SkillBookPopup>().SetInfo()),
            ("travel", () => Managers.UI.ShowPopupUI<UI_TravelPopup>().SetInfo()),
            ("shop", () => Managers.UI.ShowPopupUI<UI_ShopPopup>().SetInfo(false)),
            ("order", () => Managers.UI.ShowPopupUI<UI_CustomOrderPopup>().SetInfo()),
            ("potiondetail", () => Managers.UI.ShowPopupUI<UI_PotionDetailPopup>().SetInfo(Managers.Game.CurrentHPPotionDataId, true)),
            ("item", () => Managers.UI.ShowPopupUI<UI_ItemPopup>().SetInfo(11001)),
            ("setting", () => Managers.UI.ShowPopupUI<UI_SettingPopup>()),
            ("credits", () => Managers.UI.ShowPopupUI<UI_CreditsPopup>()),
            ("unlock", () => Managers.UI.ShowPopupUI<UI_UnlockButtonPopup>()),
        };
        if (equip != null)
        {
            list.Add(("forge", () => Managers.UI.ShowPopupUI<UI_ForgePopup>().SetInfo(equip)));
            list.Add(("enhance", () => Managers.UI.ShowPopupUI<UI_EnhacePopup>().SetInfo(equip, 0)));
            list.Add(("equipdetail", () => Managers.UI.ShowPopupUI<UI_EquipDetailPopup>().SetInfo(equip)));
        }
        if (shopItem != null)
            list.Add(("shopconfirm", () => Managers.UI.ShowPopupUI<UI_ShopConfirmPopup>().SetInfo(shopItem, false)));
        return list;
    }

    static IEnumerator CoRun()
    {
        float t0 = Time.realtimeSinceStartup;
        var fonts = new List<string>();
        var overflow = new List<string>();
        var leaks = new List<string>();
        var original = Managers.Game.Language;

        foreach (var lang in new[] { Define.Language.Korean, Define.Language.English })
        {
            Managers.Game.Language = lang;
            var seen = new HashSet<string>();
            foreach (var (label, open) in Popups())
            {
                var before = EventCounts();
                for (int k = 0; k < CYCLES; k++)
                {
                    Managers.UI.CloseAllPopupUI();
                    yield return new WaitForSecondsRealtime(0.4f);
                    bool opened = true;
                    try { open(); }
                    catch (Exception e) { opened = false; leaks.Add($"{lang}/{label} 열기 실패: {e.Message}"); }
                    yield return new WaitForSecondsRealtime(k == 0 ? 1.5f : 0.4f);
                    if (opened && k == 0) Scan($"{lang}/{label}", seen, fonts, overflow);
                }
                Managers.UI.CloseAllPopupUI();
                yield return new WaitForSecondsRealtime(0.4f);
                foreach (var kv in EventCounts())
                    if (before.TryGetValue(kv.Key, out int b) && kv.Value > b)
                        leaks.Add($"{lang}/{label}: {kv.Key} {b} → {kv.Value}");
            }
        }
        Managers.Game.Language = original;

        var sb = new StringBuilder("═══ UI 품질 스캔 (팝업 × 한/영) ═══\n");
        sb.AppendLine($"A. 폰트 섞임 {fonts.Count}건"); foreach (var s in fonts) sb.AppendLine("  ✗ " + s);
        sb.AppendLine($"C. 구독 누수 {leaks.Count}건"); foreach (var s in leaks) sb.AppendLine("  ✗ " + s);
        sb.AppendLine($"B. 넘침 후보 {overflow.Count}건 (캡처로 확인)"); foreach (var s in overflow) sb.AppendLine("  · " + s);
        sb.AppendLine($"소요 {Time.realtimeSinceStartup - t0:F1}s");
        string outPath = Path.Combine(Application.persistentDataPath, "ui_scan_result.txt");
        try { File.WriteAllText(outPath, sb.ToString()); } catch { }
        sb.AppendLine($"(결과 파일: {outPath})");
        if (fonts.Count + leaks.Count == 0) Debug.Log(sb.ToString());
        else Debug.LogError(sb.ToString());
    }

    static void Scan(string label, HashSet<string> seen, List<string> fonts, List<string> overflow)
    {
        foreach (var t in UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
        {
            if (t == null || !t.gameObject.activeInHierarchy || string.IsNullOrWhiteSpace(t.text) || t.font == null || t.color.a < 0.05f)
                continue;
            // 팝업 뒤에 비치는 HUD 를 팝업마다 다시 세지 않는다 — HUD 는 'hud' 단계에서 한 번만 본다.
            if (!label.EndsWith("/hud") && t.GetComponentInParent<UI_Popup>() == null) continue;
            t.ForceMeshUpdate();
            var info = t.textInfo;
            if (info.characterCount == 0) continue;
            var owner = t.GetComponentInParent<UI_Base>();
            string id = $"{(owner ? owner.GetType().Name : "-")}/{t.name}";
            string where = $"{label} | {id}";
            string sample = t.text.Replace("\n", " ");
            if (sample.Length > 40) sample = sample.Substring(0, 40);

            // A. 글자·숫자가 폴백으로 — 장식 기호(○◆【】·— 등)는 단어 밖이라 제외
            var mixed = new StringBuilder();
            string fbName = null;
            for (int i = 0; i < info.characterCount; i++)
            {
                var ci = info.characterInfo[i];
                if (ci.fontAsset == null || ci.fontAsset == t.font || !char.IsLetterOrDigit(ci.character)) continue;
                mixed.Append(ci.character);
                fbName = ci.fontAsset.name;
            }
            if (mixed.Length > 0 && seen.Add("A" + id + mixed))
                fonts.Add($"{where} 주={t.font.name} 폴백={fbName} [{mixed}] \"{sample}\"");

            // B. 넘침 — 그려진 글자 범위(월드) vs 부모 칸(월드)
            var parent = t.rectTransform.parent as RectTransform;
            if (parent == null || parent.rect.width < MIN_PARENT_WIDTH) continue;
            var c = new Vector3[4];
            parent.GetWorldCorners(c);
            var b = t.textBounds;
            float l = t.rectTransform.TransformPoint(new Vector3(b.min.x, 0, 0)).x;
            float r = t.rectTransform.TransformPoint(new Vector3(b.max.x, 0, 0)).x;
            float scale = parent.lossyScale.x > 0 ? parent.lossyScale.x : 1f;
            float over = Mathf.Max(c[0].x - l, r - c[2].x) / scale;
            if (over > OVERFLOW_TOLERANCE && seen.Add("B" + id))
                overflow.Add($"{where} 넘침 {over:F0} / 칸 {parent.rect.width:F0} \"{sample}\"");
        }
    }

    // 매니저·플레이어의 이벤트(델리게이트 필드)별 구독 수
    static Dictionary<string, int> EventCounts()
    {
        const System.Reflection.BindingFlags BF = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public
                                                | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly;
        var d = new Dictionary<string, int>();
        var owners = new (string, object)[]
        {
            ("Game", Managers.Game), ("Quest", Managers.Quest), ("SubQuest", Managers.SubQuest), ("MinMon", Managers.MinMon),
            ("Ability", Managers.Ability), ("UI", Managers.UI), ("Combo", Managers.Combo), ("Player", Managers.Game?.Player),
        };
        foreach (var (name, o) in owners)
        {
            if (o == null) continue;
            for (var type = o.GetType(); type != null && type != typeof(object) && type != typeof(MonoBehaviour); type = type.BaseType)
                foreach (var f in type.GetFields(BF))
                    if (typeof(Delegate).IsAssignableFrom(f.FieldType))
                        d[$"{name}.{f.Name}"] = (f.GetValue(o) as Delegate)?.GetInvocationList().Length ?? 0;
        }
        return d;
    }
}
