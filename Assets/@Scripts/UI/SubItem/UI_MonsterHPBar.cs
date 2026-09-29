using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UI_MonsterHPBar : UI_Base
{
    #region Enum

    enum Images
    {
        Frame,
        MonsterHPBar,
        MonsterHPBarGuage,
    }

    enum Texts
    {
        MonsterNameDisplay,
    }

    enum Buttons
    {
    }

    enum GameObjects
    {
    }

    #endregion

    // 슬라이드 59 — 희귀도별 Scale / Color / Material
    static readonly Color COLOR_COMMON = Color.white;
    static readonly Color COLOR_RARE   = new Color32(0xFF, 0xD4, 0x00, 0xFF);   // FFD400
    static readonly Color COLOR_BOSS   = new Color32(0xFF, 0x43, 0x00, 0xFF);   // FF4300

    const float SCALE_COMMON = 1.0f;
    const float SCALE_RARE   = 1.15f;
    const float SCALE_BOSS   = 1.3f;

    const float OPEN_ANIM_DURATION = 0.5f;

    private void Awake() { Init(); }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        RefreshTexts();

        return true;
    }

    // 풀링되는 HP 바다 — 언어 구독은 켜져 있는 동안만 둔다. Init 에서 걸고 OnDestroy 에서만 풀면 풀에 잠든 바가
    //  구독을 쥔 채 쌓였다(UI 품질 스캔 '구독 누수': 플레이어가 쓰러져 새 바가 만들어질 때마다 +1).
    void OnEnable()
    {
        if (Managers.Game != null) Managers.Game.OnLanguageChanged += RefreshTexts;
    }

    void OnDisable()
    {
        if (Managers.Game != null) Managers.Game.OnLanguageChanged -= RefreshTexts;
        Unwatch();
    }

    // enum Texts: MonsterNameDisplay — 동적 (CreatureData.Name SetInfo에서 set), Battle 폰트만.
    void RefreshTexts()
    {
        var name = GetText((int)Texts.MonsterNameDisplay);
        if (name != null) ApplyFont(name, Define.FontType.Battle);
    }

    // 지켜보는 몬스터 — 바가 풀로 돌아가 다른 몬스터에 다시 쓰일 때 예전 몬스터의 HP 변화가 새 바를 움직이지 않게 끊는다.
    MonsterController _monster;
    bool _closing;
    int  _useToken;

    public void SetInfo(MonsterController monster)
    {
        Unwatch();
        _useToken++;
        _closing = false;
        if (Managers.Data.CreatureDic.TryGetValue(monster.DataId, out var data))
            GetText((int)Texts.MonsterNameDisplay).text = Util.Localize(data.Name);

        ApplyRarity(monster.Rarity);
        UpdateHp(monster.Hp, monster.MaxHp);
        PlayOpenAnimation(monster.Rarity);

        _monster = monster;
        monster.OnHpChanged += OnMonsterHpChanged;
    }

    void OnMonsterHpChanged(float cur, float max)
    {
        UpdateHp(cur, max);
        if (cur <= 0)
            PlayCloseAnimation();
    }

    void Unwatch()
    {
        if (_monster != null) _monster.OnHpChanged -= OnMonsterHpChanged;
        _monster = null;
    }

    // 슬라이드 66/67 — 몬스터 사망 또는 Player 사망 시 HP Bar는 DOPlayBackwards로 사라짐.
    // prefab의 DOTweenAnimation(Move OutBack From x0 y0 z0)를 역방향 재생 → 원위치(0,0,0)로 되돌아감.
    //  닫기는 한 번만(몬스터와 플레이어가 같이 쓰러지면 두 번 불렸다). 늦게 도착한 반납 예약은 그 사이 다른 몬스터에
    //  다시 쓰인 바를 치우지 않는다(_useToken).
    public void PlayCloseAnimation()
    {
        if (_closing) return;
        _closing = true;
        Unwatch();
        int token = _useToken;
        var anim = GetComponent<DOTweenAnimation>();
        if (anim != null)
        {
            anim.DOPlayBackwards();
            DG.Tweening.DOVirtual.DelayedCall(anim.duration, () =>
            {
                if (this != null && token == _useToken) Managers.Resource.Destroy(gameObject);
            });
        }
        else
        {
            Managers.Resource.Destroy(gameObject);
        }
    }

    void ApplyRarity(CreatureRarity rarity)
    {
        Image frame = GetImage((int)Images.Frame);
        if (frame == null) return;

        // Color
        switch (rarity)
        {
            case CreatureRarity.Rare: frame.color = COLOR_RARE; break;
            case CreatureRarity.Boss: frame.color = COLOR_BOSS; break;
            default:                  frame.color = COLOR_COMMON; break;
        }

        // Material — Path_Image_Knight (희귀) / Path_Image_BeastLord (우두머리)
        // Common은 기본 UI material 그대로 (null로 두면 Image 기본 material 사용)
        string matName = rarity switch
        {
            CreatureRarity.Rare => "Path_Image_Knight",
            CreatureRarity.Boss => "Path_Image_BeastLord",
            _                   => null,
        };
        if (matName != null)
        {
            Material mat = Managers.Resource.Load<Material>(matName);
            if (mat != null) frame.material = mat;
        }
        else
        {
            frame.material = null;   // 기본 UI material로 복귀
        }
    }

    void PlayOpenAnimation(CreatureRarity rarity)
    {
        float targetScale = rarity switch
        {
            CreatureRarity.Rare => SCALE_RARE,
            CreatureRarity.Boss => SCALE_BOSS,
            _                   => SCALE_COMMON,
        };

        // 슬라이드 59 — Move (Duration 0.5 OutBack From x0 y0 z0)는 prefab의 DOTweenAnimation이 처리.
        // 희귀도 scale은 별개 — DOTweenAnimation이 position을 animate하므로 충돌 없음.
        transform.localScale = Vector3.one * targetScale;

        var anim = GetComponent<DOTweenAnimation>();
        if (anim != null) anim.DORestart();
    }

    void UpdateHp(float cur, float max)
    {
        GetImage((int)Images.MonsterHPBarGuage).fillAmount = max > 0 ? cur / max : 0f;
    }
}
