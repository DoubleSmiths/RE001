using DG.Tweening;

using System.Collections;

using System.Collections.Generic;

using UnityEngine;



public class UI_Toast : UI_Base
{

    #region Enum

    enum Images
    {
        StageTitle,
        StageTitle_1,
        StageTitle_2,
    }

    enum Texts
    {
        StageTitle_Txt,
    }

    public void OnEnable()
    {
        // Tm 오픈 연출
        //OpenAnimation();
        //PopupOpenAnimation(gameObject);

        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() => gameObject.GetComponent<DOTweenAnimation>()?.DORestart());
        sequence.AppendInterval(1.1f);
        sequence.AppendCallback(() => Fade());

    }

    #endregion

    private void Awake()
    {
        Init();
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        #region Object Bind
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        #endregion

        Refresh();

        return true;
    }

    public void SetInfo(string msg)
    {
        // 메시지 변경
        transform.localScale = Vector3.one;
        GetText((int)Texts.StageTitle_Txt).text = msg;
        Refresh();
    }

    void Refresh()
    {

    }

    void Fade()
    {
        GetImage((int)Images.StageTitle).DOFade(0, 1.5f);
        GetImage((int)Images.StageTitle_1).DOFade(0, 1.5f);
        GetImage((int)Images.StageTitle_2).DOFade(0, 1.5f);
        GetText((int)Texts.StageTitle_Txt).DOFade(0, 1.5f);
    }

}

