using TMPro;
using UnityEngine;
using static Define;

public class FontManager
{
    // 폰트 스펙:
    //   - Korean UI   : PF_Stardust  (한글 선택 시 메뉴/아이템/설명 등)
    //   - English UI  : DOSSaemmul   (영문 선택 시 동일 용도)
    //   - Dialogue    : Cafe24 Klassic (NPC 대화, 언어 무관)
    //   - Battle      : RiaSans-Bold  (슬라이드 47 전투 UI 고정)
    //   - 숫자 Text   : prefab 에서 직접 지정된 폰트 유지 (언어 무관). 여기서는 주입하지 않음.
    TMP_FontAsset _krFont;
    TMP_FontAsset _enFont;
    TMP_FontAsset _dialogueFont;
    TMP_FontAsset _battleFont;

    public void Init()
    {
        _krFont       = Managers.Resource.Load<TMP_FontAsset>("PF_Stardust SDF");
        _enFont       = Managers.Resource.Load<TMP_FontAsset>("DOSSaemmul SDF");
        _dialogueFont = Managers.Resource.Load<TMP_FontAsset>("Cafe24 SDF");
        _battleFont   = Managers.Resource.Load<TMP_FontAsset>("RiaSans-Bold SDF");
    }

    /// <summary>FontType과 현재 언어 설정에 맞는 폰트를 반환</summary>
    public TMP_FontAsset GetFont(FontType type)
    {
        return type switch
        {
            FontType.Dialogue => _dialogueFont,
            FontType.Battle   => _battleFont,
            FontType.UI       => Managers.Game.Language == Language.Korean ? _krFont : _enFont,
            _                 => _krFont,
        };
    }
}
