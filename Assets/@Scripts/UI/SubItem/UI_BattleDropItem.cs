using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

/// <summary>
/// 슬라이드 49: 전투 중 몬스터 드롭 아이템 슬롯.
/// 프리펩 구조:
///   UI_BattleDropItem (root, Image + DOTweenAnimation 스케일 1.2 Yoyo 2회 OutQuad)
///     ├─ UI_BattleDropItemIcon    (Image, 아이템 아이콘)
///     ├─ UI_BattleDropItemFram    (Image, 프레임)
///     ├─ UI_BattleDropItemCount   (TMP_Text, "해당 장소에서만 얻은 개수")
///     ├─ DMGBoostEffect1          (ParticleSystem, Exceptional+ 전용 — SetActive 토글)
///     └─ (기타 GlowDot 장식)
/// </summary>
public class UI_BattleDropItem : UI_Base
{
    enum Images
    {
        UI_BattleDropItemIcon,
        UI_BattleDropItemGrow,   // 등급 색 글로우 배경
    }

    enum Texts
    {
        UI_BattleDropItemCount,
    }

    enum GameObjects
    {
        DMGBoostEffect1,   // Exceptional+ 등급 파티클
    }

    public int        DataId     { get; private set; }
    public int        LocalCount { get; private set; }
    public EquipGrade Grade      { get; private set; }

    DOTweenAnimation _scaleAnim;
    GameObject       _gradeParticleGO;
    ParticleSystem   _gradeParticle;

    public override bool Init()
    {
        if (!base.Init()) return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindObject(typeof(GameObjects));

        _scaleAnim       = GetComponent<DOTweenAnimation>();
        _gradeParticleGO = GetObject((int)GameObjects.DMGBoostEffect1);
        if (_gradeParticleGO != null)
            _gradeParticle = _gradeParticleGO.GetComponent<ParticleSystem>();

        RefreshTexts();
        if (Managers.Game != null) Managers.Game.OnLanguageChanged += RefreshTexts;

        return true;
    }

    public override void OnDestroy()
    {
        if (Managers.Game != null) Managers.Game.OnLanguageChanged -= RefreshTexts;
        base.OnDestroy();
    }

    // enum Texts: UI_BattleDropItemCount — 동적 숫자 (RefreshCount에서 set), Battle 폰트만.
    void RefreshTexts()
    {
        var count = GetText((int)Texts.UI_BattleDropItemCount);
        if (count != null) ApplyFont(count, Define.FontType.Battle);
    }

    // 슬롯 최초 생성 — 아이콘/등급/카운트 세팅 + 초기 DOTween 연출.
    public void SetInfo(int dataId, int initialCount = 1)
    {
        if (!_init) Init();

        DataId     = dataId;
        LocalCount = Mathf.Max(1, initialCount);

        if (Managers.Data.ItemDic != null &&
            Managers.Data.ItemDic.TryGetValue(dataId, out var data))
        {
            Grade = data.Grade;
            SetIcon(data.SpriteName);
        }

        RefreshCount();
        ApplyGradeVisuals();
        PlayScaleAnim();
    }

    // 중복 획득 — 카운트만 변경 + DOTween 재생.
    public void AddCount(int by = 1)
    {
        LocalCount += Mathf.Max(1, by);
        RefreshCount();
        PlayScaleAnim();
    }

    void RefreshCount()
    {
        var tmp = GetText((int)Texts.UI_BattleDropItemCount);
        if (tmp != null) tmp.text = LocalCount.ToString();
    }

    void SetIcon(string spriteName)
    {
        var img = GetImage((int)Images.UI_BattleDropItemIcon);
        if (img == null || string.IsNullOrEmpty(spriteName)) return;
        var sprite = Managers.Resource.Load<Sprite>(spriteName);
        if (sprite != null) img.sprite = sprite;
    }

    void ApplyGradeVisuals()
    {
        Color gradeColor = Util.GetGradeColor(Grade);

        // Grow 배경 색 — 등급 색, 원래 알파 보존.
        var grow = GetImage((int)Images.UI_BattleDropItemGrow);
        if (grow != null)
        {
            Color c = gradeColor;
            c.a = grow.color.a;
            grow.color = c;
        }

        // Exceptional+ 파티클 토글
        bool highGrade = Grade >= EquipGrade.Exceptional;
        if (_gradeParticleGO != null) _gradeParticleGO.SetActive(highGrade);
        if (!highGrade || _gradeParticle == null) return;

        var main = _gradeParticle.main;
        main.startColor = new ParticleSystem.MinMaxGradient(gradeColor);
        _gradeParticle.Clear(true);
        _gradeParticle.Play(true);
    }

    void PlayScaleAnim()
    {
        // prefab 스케일 연출: 0.15s × 2 Yoyo, OutQuad, to 1.2 (autoPlay=1 이지만 수동 재생 시 DORestart)
        _scaleAnim?.DORestart();
    }
}
