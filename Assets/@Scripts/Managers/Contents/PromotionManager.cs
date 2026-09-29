using System;
using UnityEngine;
using static Define;

// v2 시련(승급 시험, ADR0037, 설계서 4.11) — 레벨 PROMO_LEVEL_STEP 마다 한 단계씩(최대 PROMO_MAX_TIER).
//  화면 이름은 '시련' — 퀘스트의 'N급 모험가 승급'(접수원, AdventurerRank)과 헷갈리지 않게 '승급'을 쓰지 않는다.
//  시험 = 요구 레벨에 맞는 스테이지(ExamNode)의 보스를 PROMO_BOSS_MULT 배로 키운 '시험관'을 PROMO_TIME_LIMIT 초 안에 쓰러뜨리기.
//   싸우는 곳은 사냥하던 스테이지 그대로다(쉬운 1-1 에서 치르는 우회를 막으려고 능력치만 단계에 묶었다).
//  통과하면 공격·체력이 영구히 PROMO_ATK_MULT·PROMO_HP_MULT 배씩(단계만큼 거듭제곱) + 크리스탈. 떨어져도 벌은 없고 곧바로 다시 할 수 있다.
//  입장은 미션 창 던전 탭(요일 던전 아래), 전투는 UI_BattleBtnGroup.CoPromotion(스테이지 보스와 같은 CoFightBoss).
//  세이브는 GameData.PromotionTier.
public class PromotionManager
{
    public event Action OnChanged;

    static float B(string key, float fallback) => Managers.Data.Balance(key, fallback);

    public int   Tier     => Mathf.Min(Managers.Game.PromotionTier, MaxTier);   // 손댄 세이브·최대 단계 하향에도 배수가 폭주하지 않게
    public int   MaxTier  => Mathf.Max(1, (int)B(BalanceKey.PROMO_MAX_TIER, 10f));
    public int   NextTier => Tier + 1;
    public bool  IsMax    => Tier >= MaxTier;
    public int   RequiredLevel(int tier) => Mathf.Max(1, (int)B(BalanceKey.PROMO_LEVEL_STEP, 30f)) * tier;
    public bool  CanChallenge => !IsMax && Managers.Game.Level >= RequiredLevel(NextTier);
    public bool  IsChallengeReady => CanChallenge && !IsPending && !IsRunning;   // 던전 탭 '!'·미션 버튼 알림 점(예약·진행 중엔 끈다)
    public int   ExamNode  => Managers.Game.StageForLevel(RequiredLevel(NextTier));
    public float TimeLimit => B(BalanceKey.PROMO_TIME_LIMIT, 45f);
    public float BossMult  => B(BalanceKey.PROMO_BOSS_MULT, 1.5f);
    public float AtkMult(int tier) => Mathf.Pow(B(BalanceKey.PROMO_ATK_MULT, 1.1f), tier);
    public float HpMult(int tier)  => Mathf.Pow(B(BalanceKey.PROMO_HP_MULT, 1.1f), tier);

    public bool IsPending { get; private set; }
    public bool IsRunning { get; private set; }

    // RefreshStats 가 훈련·유물 다음에 부른다(곱이라 순서는 상관없다)
    public void ApplyTo(PlayerController p)
    {
        if (p == null || Managers.Game == null) return;
        float atk = AtkMult(Tier);
        p.PhysicalAtk *= atk;
        p.MagicAtk    *= atk;
        p.MaxHp       *= HpMult(Tier);
    }

    // 입장 예약 — 전투 화면 전환은 부른 쪽(UI_MissionPopup)이 하고, 새 전투 그룹이 Begin 으로 시작한다(요일 던전과 같은 흐름).
    public bool TryEnter()
    {
        if (!CanChallenge || IsPending || IsRunning) return false;
        if (Managers.Dungeon != null && (Managers.Dungeon.IsPending || Managers.Dungeon.IsRunning)) return false;
        if (Managers.Abyss != null && (Managers.Abyss.IsPending || Managers.Abyss.IsRunning)) return false;
        int node = Managers.Dungeon?.PickNode() ?? 0;   // 사냥하던 스테이지(요일 던전과 같은 자리)
        if (node <= 0) return false;
        IsPending = true;
        Managers.Game.CurrentTravelAreaId = node;   // 스폰 지역은 이 값으로 정해진다 — 전환보다 먼저
        OnChanged?.Invoke();
        return true;
    }

    public bool Begin()
    {
        if (!IsPending) return false;
        IsPending = false;
        IsRunning = true;
        return true;
    }

    // 시험 끝 — 통과면 단계 +1(스탯 곧바로 반영) + 크리스탈. 귀환·쓰러짐도 실패로 끝낸다(여러 번 불려도 한 번).
    public void Finish(bool passed)
    {
        if (!IsRunning) return;
        IsRunning = false;
        var g = Managers.Game;
        if (passed)
        {
            g.PromotionTier = Tier + 1;
            int crystals = Mathf.RoundToInt(B(BalanceKey.PROMO_CRYSTAL, 50f) * Tier);
            if (crystals > 0) g.Crystal += crystals;
            var p = g.Player;   // == 로 파괴 여부까지
            if (p != null) p.RefreshStats(fullHeal: false);
            Managers.UI.ShowToast(string.Format(Util.Localize("PromotionPassFormat"), Tier, AtkMult(Tier)));
        }
        else
            Managers.UI.ShowToast(Util.Localize("PromotionFailText"));
        OnChanged?.Invoke();
    }
}
