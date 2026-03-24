using TMPro;
using UnityEngine;
using static Define;

public class FontManager
{
    TMP_FontAsset _krFont;       // Cafe24 DOSSaemmul
    TMP_FontAsset _enFont;       // PF_Stardust
    TMP_FontAsset _dialogueFont; // Cafe24 Klassic (언어 무관 고정)

    public void Init()
    {
        _krFont       = Managers.Resource.Load<TMP_FontAsset>("Cafe24DOSSaemmul SDF");
        _enFont       = Managers.Resource.Load<TMP_FontAsset>("PFStardust SDF");
        _dialogueFont = Managers.Resource.Load<TMP_FontAsset>("Cafe24Klassic SDF");
    }

    /// <summary>FontType과 현재 언어 설정에 맞는 폰트를 반환</summary>
    public TMP_FontAsset GetFont(FontType type)
    {
        return type switch
        {
            FontType.Dialogue => _dialogueFont,
            FontType.UI       => Managers.Game.Language == Language.Korean ? _krFont : _enFont,
            _                 => _krFont,
        };
    }
}
