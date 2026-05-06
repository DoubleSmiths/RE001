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

        return true;
    }

    public void SetInfo(MonsterController monster)
    {
        if (Managers.Data.CreatureDic.TryGetValue(monster.DataId, out var data))
            GetText((int)Texts.MonsterNameDisplay).text = Util.Localize(data.Name);

        ApplyRarity(monster.Rarity);
        UpdateHp(monster.Hp, monster.MaxHp);
        PlayOpenAnimation(monster.Rarity);

        monster.OnHpChanged += (cur, max) =>
        {
            UpdateHp(cur, max);
            if (cur <= 0)
                PlayCloseAnimation();
        };
    }

    // 슬라이드 66/67 — 몬스터 사망 또는 Player 사망 시 HP Bar는 DOPlayBackwards로 사라짐.
    // prefab의 DOTweenAnimation(Move OutBack From x0 y0 z0)를 역방향 재생 → 원위치(0,0,0)로 되돌아감.
    public void PlayCloseAnimation()
    {
        var anim = GetComponent<DOTweenAnimation>();
        if (anim != null)
        {
            anim.DOPlayBackwards();
            DG.Tweening.DOVirtual.DelayedCall(anim.duration, () => Managers.Resource.Destroy(gameObject));
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
