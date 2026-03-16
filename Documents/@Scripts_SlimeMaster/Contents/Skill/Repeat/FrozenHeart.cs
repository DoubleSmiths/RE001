using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class FrozenHeart : RepeatSkill
{
    [SerializeField]
    public GameObject[] Spinner = new GameObject[6];
    [SerializeField]
    public GameObject[] SpinnerFinal = new GameObject[6];
   
    private void Awake()
    {
        SkillType = Define.SkillType.FrozenHeart;
        gameObject.SetActive(false);
        SetActiveSpinner(false);
    }

    public override void ActivateSkill()
    {
        gameObject.SetActive(true);
        SetActiveSpinner(true);
        ActiveSpinner();
    }

    public override void OnChangedSkillData()
    {
        SetActiveSpinner(true);
        SetFrozenHeart();
    }

    public void SetActiveSpinner(bool isActive)
    {
        if (Level == 6)
        {
            foreach (GameObject spinner in SpinnerFinal)
                spinner.SetActive(isActive);

            foreach (GameObject spinner in Spinner)
                spinner.SetActive(false);
        }
        else
        {
            foreach (GameObject spinner in Spinner)
                spinner.SetActive(isActive);

            foreach (GameObject spinner in SpinnerFinal)
                spinner.SetActive(false);
        }
    }

    void SetFrozenHeart()
    {
        transform.localPosition = Vector3.zero;
        if (Level == 6)
        {
            for (int i = 0; i < SpinnerFinal.Length; i++)
            {
                if (i < SkillData.NumProjectiles)
                {
                    SpinnerFinal[i].SetActive(true);
                    float degree = 360f / SkillData.NumProjectiles * i;
                    SpinnerFinal[i].transform.localPosition = Quaternion.Euler(0f, 0f, degree) * Vector3.up * SkillData.ProjRange;
                    SpinnerFinal[i].transform.localScale = Vector3.one * SkillData.ScaleMultiplier;
                }
                else
                {
                    SpinnerFinal[i].SetActive(false);
                }
            }
        }
        else
        {
            for (int i = 0; i < Spinner.Length; i++)
            {
                if (i < SkillData.NumProjectiles)
                {
                    Spinner[i].SetActive(true);
                    float degree = 360f / SkillData.NumProjectiles * i;
                    Spinner[i].transform.localPosition = Quaternion.Euler(0f, 0f, degree) * Vector3.up * SkillData.ProjRange;
                    Spinner[i].transform.localScale = Vector3.one * SkillData.ScaleMultiplier;
                }
                else
                {
                    Spinner[i].SetActive(false);
                }
            }
        }


    }
    void ActiveSpinner()
    {
        SetFrozenHeart();
        Sequence EnableSequence = DOTween.Sequence();
        gameObject.SetActive(true);

        float speed = SkillData.RoatateSpeed * SkillData.Duration;
        Tween scale = transform.DOScale(1, 0.2f);// ???? Ŀ??
        Tween rotate = transform.DORotate(new Vector3(0, 0, speed), SkillData.Duration, RotateMode.FastBeyond360).SetEase(Ease.Linear);

        Tween scale2 = transform.DOScale(0, 1f);// ???? ?????
        Tween rotate2 = transform.DORotate(new Vector3(0, 0, speed * 1), 1f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear);
        //Tween fade = gameObject.SetActive(false);
        EnableSequence.Append(scale).Join(rotate)//.AppendInterval(duration)
            .Append(scale2).Join(rotate2)
            .InsertCallback(SkillData.Duration , () => gameObject.SetActive(false))
            .AppendInterval(SkillData.CoolTime)
            .AppendCallback(() => ActiveSpinner());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CreatureController creature = collision.transform.GetComponent<CreatureController>();
        if (creature.IsValid() == false)
            return;

        if (creature?.IsMonster() == true)
            creature.OnDamaged(Managers.Game.Player, this);
    }

    protected override void DoSkillJob()
    {
    }
}
