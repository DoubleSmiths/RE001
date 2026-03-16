using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementManager
{
    private List<Data.AchievementData> achievements
    {
        get { return Managers.Game.Achievements; }
        set { Managers.Game.Achievements = value;}
    }
    public event Action<Data.AchievementData> OnAchievementCompleted;  // 업적 완료 이벤트

    // 업적 추가
    public void Init()
    {
        achievements = Managers.Data.AchievementDataDic.Values.ToList();
    }

    // 업적 완료 처리
    public void CompleteAchievement(int dataId)
    {
        AchievementData achievement = achievements.Find(a => a.AchievementID == dataId);
        if (achievement != null && achievement.IsCompleted == false)
        {
            achievement.IsCompleted = true;
            OnAchievementCompleted?.Invoke(achievement);  // 업적 완료 이벤트 호출
            //Managers.UI.ShowToast($"업적달성 : {achievement.DescriptionTextID}");
            Managers.Game.SaveGame();
        }
    }

    // 보상받기 완료 처리
    public void RewardedAchievement(int dataId)
    {
        AchievementData achievement = achievements.Find(a => a.AchievementID == dataId);
        if (achievement != null && achievement.IsRewarded == false)
        {
            achievement.IsRewarded = true;
            achievement.IsCompleted = true;
            Managers.Game.SaveGame();
        }
    }


    public void Attendance()
    {
        List<AchievementData> list = achievements.Where(data => data.MissionTarget == Define.MissionTarget.Login).ToList();

        // 출석 업적 보상 확인
        foreach (AchievementData achievement in list)
        {
            if (!achievement.IsCompleted && achievement.MissionTargetValue == Managers.Time.AttendanceDay)
            {
                CompleteAchievement(achievement.AchievementID);
            }
        }
    }
   
    public List<AchievementData> GetProceedingAchievment()
    {
        List<AchievementData> resultList = new List<AchievementData>();

        //1. achievements 리스트에서 achievments.MissionTarget이 같은 애들끼리 나눈다. 즉 MissionTarget의 갯수만큼 리스트를 만든다.
        foreach(Define.MissionTarget missionTarget in Enum.GetValues(typeof(Define.MissionTarget)))
        {
            List<AchievementData> list = achievements.Where(data => data.MissionTarget == missionTarget).ToList();
            //2. list중에 현재 진행중인 애들을 ADD
            for(int i = 0; i < list.Count; i++)
            {
                if (list[i].IsCompleted == false)
                {
                    resultList.Add(list[i]);
                    break;
                }
                else 
                {
                    if (list[i].IsRewarded == false)
                    {
                        resultList.Add(list[i]);
                        break;
                    }

                    if(i == list.Count - 1)
                        resultList.Add(list[i]);
                }
            }
        }

        return resultList;
    }

    public int GetProgressValue(Define.MissionTarget missionTarget)
    {
        switch (missionTarget)
        {
            case Define.MissionTarget.DailyComplete:
            case Define.MissionTarget.WeeklyComplete:
                return 0;
            
            case Define.MissionTarget.StageEnter:
                return Managers.Game.DicMission[missionTarget].Progress;
            
            case Define.MissionTarget.StageClear:
                return Managers.Game.GetMaxStageClearIndex();
            
            case Define.MissionTarget.EquipmentLevelUp:
                return Managers.Game.DicMission[missionTarget].Progress;
            
            case Define.MissionTarget.CommonGachaOpen:
                return Managers.Game.CommonGachaOpenCount;
            
            case Define.MissionTarget.AdvancedGachaOpen:
                return Managers.Game.AdvancedGachaOpenCount;
            
            case Define.MissionTarget.OfflineRewardGet:
                return Managers.Game.OfflineRewardCount;
            
            case Define.MissionTarget.FastOfflineRewardGet:
                return Managers.Game.FastRewardCount;
            
            case Define.MissionTarget.ShopProductBuy:
                return 0;
            
            case Define.MissionTarget.Login:
                return Managers.Time.AttendanceDay;
            
            case Define.MissionTarget.EquipmentMerge:
                return Managers.Game.DicMission[missionTarget].Progress;
            
            case Define.MissionTarget.MonsterAttack:
                return 0;
            
            case Define.MissionTarget.MonsterKill:
                return Managers.Game.TotalMonsterKillCount;

            case Define.MissionTarget.EliteMonsterAttack:
                return 0;

            case Define.MissionTarget.EliteMonsterKill:
                return Managers.Game.TotalEliteKillCount;

            case Define.MissionTarget.BossKill:
                return Managers.Game.TotalBossKillCount;
         
            case Define.MissionTarget.DailyShopBuy:
                return 0;
         
            case Define.MissionTarget.GachaOpen:
                return Managers.Game.DicMission[missionTarget].Progress;
         
            case Define.MissionTarget.ADWatchIng:
                return Managers.Game.DicMission[missionTarget].Progress;
        }
        return 0;
    }

    public AchievementData GetNextAchievment(int dataId)
    {
        AchievementData achievement = achievements.Find(a => a.AchievementID == dataId +1);
        if (achievement != null && achievement.IsRewarded == false)
        {
            return achievement;
        }
        return null;
    }
   
    public void StageClear()
    {
        int MaxStageClearIndex = Managers.Game.GetMaxStageClearIndex();

        List<AchievementData> list = achievements.Where(data => data.MissionTarget == Define.MissionTarget.StageClear).ToList();
        foreach (AchievementData achievement in list)
        {
            if (!achievement.IsCompleted && achievement.MissionTargetValue == MaxStageClearIndex)
            {
                CompleteAchievement(achievement.AchievementID);
            }
        }
    }

    public void CommonOpen()
    {
        List<AchievementData> list = achievements.Where(data => data.MissionTarget == Define.MissionTarget.StageClear).ToList();

        foreach (AchievementData achievement in achievements)
        {
            if (!achievement.IsCompleted && achievement.MissionTargetValue == Managers.Game.CommonGachaOpenCount)
            {
                CompleteAchievement(achievement.AchievementID);
            }
        }
    }
    public void AdvancedOpen()
    {
        List<AchievementData> list = achievements.Where(data => data.MissionTarget == Define.MissionTarget.AdvancedGachaOpen).ToList();

        foreach (AchievementData achievement in list)
        {
            if (!achievement.IsCompleted && achievement.MissionTargetValue == Managers.Game.AdvancedGachaOpenCount)
            {
                CompleteAchievement(achievement.AchievementID);
            }
        }
    }
    public void OfflineReward()
    {
        List<AchievementData> list = achievements.Where(data => data.MissionTarget == Define.MissionTarget.OfflineRewardGet).ToList();

        foreach (AchievementData achievement in list)
        {
            if (!achievement.IsCompleted && achievement.MissionTargetValue == Managers.Game.OfflineRewardCount)
            {
                CompleteAchievement(achievement.AchievementID);
            }
        }
    }
    public void FastReward()
    {
        List<AchievementData> list = achievements.Where(data => data.MissionTarget == Define.MissionTarget.FastOfflineRewardGet).ToList();
       
        foreach (AchievementData achievement in list)
        {
            if (!achievement.IsCompleted && achievement.MissionTargetValue == Managers.Game.FastRewardCount)
            {
                CompleteAchievement(achievement.AchievementID);
            }
        }
    }
    public void MonsterKill()
    {
        List<AchievementData> list = achievements.Where(data => data.MissionTarget == Define.MissionTarget.MonsterKill).ToList();

        foreach (AchievementData achievement in list)
        {
            if (!achievement.IsCompleted && achievement.MissionTargetValue == Managers.Game.TotalMonsterKillCount)
                CompleteAchievement(achievement.AchievementID);
        }
    }
    public void EliteKill()
    {
        List<AchievementData> list = achievements.Where(data => data.MissionTarget == Define.MissionTarget.EliteMonsterKill).ToList();

        foreach (AchievementData achievement in list)
        {
            if (!achievement.IsCompleted && achievement.MissionTargetValue == Managers.Game.TotalEliteKillCount)
                CompleteAchievement(achievement.AchievementID);
        }
    }
    public void BossKill()
    {
        List<AchievementData> list = achievements.Where(data => data.MissionTarget == Define.MissionTarget.BossKill).ToList();

        foreach (AchievementData achievement in list)
        {
            if (!achievement.IsCompleted && achievement.MissionTargetValue == Managers.Game.TotalBossKillCount)
                CompleteAchievement(achievement.AchievementID);
        }
    }
}
