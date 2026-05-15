using System;
using Data;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// 마물 도감의 몬스터 한 칸. UI_Bestiary(지역) 안에서 몬스터 수만큼 동적 생성된다.
public class UI_BestiaryBox : UI_Base
{
    #region Enum

    enum Images
    {
        BestiaryBoxSelectActive,
        BestiaryBoxFrame,
        BestiaryBoxMask,
        BestiaryBoxGlow,
        BeastiaryBoxMonsterExist,
        BestiaryBoxUnlock,
        BestiaryBoxUnlockGlow,
        BestiaryBoxNumberFrame,
        BestiaryBoxStar,
    }

    enum Texts
    {
        BestiaryBoxUnlockQustion,
        BestiaryBoxNumber,
    }

    enum Buttons
    {
        UI_BestiaryBox,
    }

    enum GameObjects
    {
        BestiaryBoxSelect,
    }

    #endregion


    int _dataId;
    Action<int> _onClick;

    void Awake() => Init();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        // gameObject.BindEvent(OnClickBox);

        return true;
    }

    // data: 몬스터 데이터 / number: 도감 표시 번호 / onClick: 클릭 콜백(dataId)
    public void SetInfo(CreatureData data, Action<int> onClick = null)
    {
        _dataId = data != null ? data.DataId : 0;
        _onClick = onClick;

        // 번호 텍스트
        var numTxt = GetText((int)Texts.BestiaryBoxNumber);
        if (numTxt != null)
        {
            numTxt.text = data.DataId.ToString("000");
            ApplyFont(numTxt, Define.FontType.UI);
        }

        // 선택 하이라이트는 기본 숨김
        GetImage((int)Images.BestiaryBoxSelectActive).gameObject.SetActive(false);

        if (data != null) // TODO 진행률이 10% 이상일 경우
        {
            // 발견한 몬스터: 아이콘 노출, 잠금 오버레이 제거
            GetImage((int)Images.BestiaryBoxUnlock).gameObject.SetActive(false);
            GetText((int)Texts.BestiaryBoxUnlockQustion).gameObject.SetActive(false);

            var icon = GetImage((int)Images.BeastiaryBoxMonsterExist);
            icon.gameObject.SetActive(true);
            if (!string.IsNullOrEmpty(data.IconLabel))
            {
                Sprite sprite = Managers.Resource.Load<Sprite>(data.IconLabel);
                if (sprite != null)
                {
                    icon.sprite = sprite;
                    icon.preserveAspect = true;
                }
                // 스프라이트를 못 찾으면 prefab 기본 스프라이트 유지
            }

            // 등급(Rarity) 글로우
            var glow = GetImage((int)Images.BestiaryBoxGlow);
            glow.gameObject.SetActive(true);
            glow.color = GetRarityColor(data.Rarity);

            GetImage((int)Images.BestiaryBoxStar).gameObject.SetActive(true);
        }
        else
        {
            // 미발견 몬스터: 잠금 + "?" 표시
            gameObject.GetComponent<Button>().interactable = false; // 해금 안됐으면 버튼 클릭 막기.
            GetImage((int)Images.BeastiaryBoxMonsterExist).gameObject.SetActive(false);
            GetImage((int)Images.BestiaryBoxStar).gameObject.SetActive(false);
            GetText((int)Texts.BestiaryBoxNumber).text = "-";


            GetImage((int)Images.BestiaryBoxGlow).color = new Color32(15, 15, 15, 255);

            GetImage((int)Images.BestiaryBoxUnlock).gameObject.SetActive(true);
            var q = GetText((int)Texts.BestiaryBoxUnlockQustion);
            q.gameObject.SetActive(true);
            ApplyFont(q, Define.FontType.UI);
        }
    }

    // Rarity 문자열 → 글로우 색 (CreatureData.Rarity: "Common"/"Rare"/"Boss" 등)
    Color GetRarityColor(string rarity)
    {
        switch (rarity)
        {
            case "Rare":   return Util.HexToColor("0064C8");
            case "Elite":  return Util.HexToColor("9600FF");
            case "Boss":   return Util.HexToColor("960E00");
            default:       return new Color32(255, 255, 255, 200); // Common
        }
    }

    void OnClickBox()
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
        transform.DOPunchScale(new Vector3(-0.2f, 0.15f, 1f), 0.3f, 10, 1f)
                 .SetEase(Ease.OutQuad)
                 .SetUpdate(true);

        _onClick?.Invoke(_dataId);
    }
}
