using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// 슬40(능력#4 강화능력): 개별 강화 버프 아이콘. UI_GameScene의 BuffDisplayer 노드에 추가된다.
//  - 생성 시: Flash_Anim의 DOTweenAnimation(등장 플래시) 실행.
//  - ③지속시간형(timed=true): 지속시간 비율에 따라 Duri_Anim.fillAmount가 0→1로 차오르고,
//                              지속%에 따라 점멸 속도가 빨라지며, 지속이 끝나면 FadeOut된다.
//  - ②인게임지속(timed=false): 정적 아이콘(게이지/점멸 없음, 외부 해제 시 즉시 제거).
//  크기는 GridLayout이 정하므로 여기서 조절하지 않는다.
public class UI_Buff : UI_Base
{
    #region Enum

    enum Images
    {
        Frame,
        Buff_Icon,
        Duri_Anim,
        Flash_Anim,
    }

    enum Texts
    {
    }

    enum Buttons
    {
    }

    enum GameObjects
    {
    }

    #endregion

    CanvasGroup _cg;
    DOTweenAnimation _flashAnim;   // Flash_Anim의 등장 플래시(authored)
    int _key; float _total; bool _timed; float _startTime; bool _removing;
    System.Action<int> _onExpired;
    public int Key => _key;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        _cg = gameObject.GetOrAddComponent<CanvasGroup>();
        // 점멸은 코드(아이콘 알파)로 제어하므로 Animator는 끈다(Flash는 authored DOTween 사용).
        var anim = GetComponent<Animator>();
        if (anim != null) anim.enabled = false;

        var flash = GetImage((int)Images.Flash_Anim);
        if (flash != null) _flashAnim = flash.GetComponent<DOTweenAnimation>();
        return true;
    }

    public void SetInfo(PlayerController.BuffInfo b, System.Action<int> onExpired)
    {
        Init();
        _onExpired = onExpired;
        Apply(b);
    }

    // 신규 적용 / 재발동 갱신 공통
    public void Apply(PlayerController.BuffInfo b)
    {
        _key = b.Key; _total = b.Duration; _timed = b.Timed; _startTime = Time.time; _removing = false;
        if (_cg != null) _cg.alpha = 1f;

        var icon = GetImage((int)Images.Buff_Icon);
        if (icon != null)
        {
            if (!string.IsNullOrEmpty(b.IconLabel))   // 비면 프리팹 기본 아이콘 유지
            {
                var sp = Managers.Resource.Load<Sprite>(b.IconLabel);
                if (sp != null) icon.sprite = sp;
            }
            var c = icon.color; c.a = 1f; icon.color = c;
        }

        var duri = GetImage((int)Images.Duri_Anim);
        if (duri != null)
        {
            duri.fillAmount = 0f;                  // 0에서 시작 → 지속시간 동안 1로 차오름
            duri.gameObject.SetActive(_timed);     // ②인게임지속은 게이지 없음
        }

        // 생성/재발동 시 등장 플래시(authored DOTweenAnimation) 실행. tween 생성 후 재생되도록 다음 프레임에.
        if (_flashAnim != null) StartCoroutine(CoPlayFlash());
    }

    IEnumerator CoPlayFlash()
    {
        yield return null;   // DOTweenAnimation.Start(autoGenerate)가 tween을 만든 뒤
        if (_flashAnim != null) _flashAnim.DORestart();
    }

    // 외부 해제(전투 종료 / 지속 만료) — FadeOut 후 제거
    public void Remove()
    {
        if (_removing) return;
        _removing = true;
        StartCoroutine(CoFadeOutAndDestroy());
    }

    IEnumerator CoFadeOutAndDestroy()
    {
        float t = 0f; const float dur = 0.4f;
        float from = _cg != null ? _cg.alpha : 1f;
        while (t < dur && _cg != null)
        {
            t += Time.deltaTime;
            _cg.alpha = Mathf.Lerp(from, 0f, t / dur);
            yield return null;
        }
        if (gameObject != null) Object.Destroy(gameObject);
    }

    private float _blinkPhase;

    void Update()
    {
        if (!_timed || _removing) return;

        float elapsed = Time.time - _startTime;
        float frac = _total > 0f ? Mathf.Clamp01(elapsed / _total) : 1f;   // 경과 비율 0→1

        var duri = GetImage((int)Images.Duri_Anim);
        if (duri != null) duri.fillAmount = frac;   // 지속시간 동안 0→1로 차오름

        var icon = GetImage((int)Images.Buff_Icon);
        if (icon != null)
        {
            float a;
            if (frac < 0.3f)
            {
                _blinkPhase = 0f;   // 점멸 phase 초기화
                a = 1f;   // 30% 진행 전: 점멸 없이 선명하게
            }
            else
            {
                // 30%~100% 구간에서만 점멸. 끝에 가까울수록 약간 빨라지되 완만하게.
                float t = Mathf.Clamp01((frac - 0.3f) / 0.7f);

                float speed = Mathf.Lerp(0.2f, 2.5f, t);

                _blinkPhase += Time.deltaTime * speed;
                float blink = 0.5f + 0.5f * Mathf.Sin(_blinkPhase * Mathf.PI * 2f);
                a = Mathf.Lerp(0.35f, 1f, blink);
            }
            var c = icon.color; c.a = a; icon.color = c;
        }

        if (frac >= 1f)   // 지속 종료 → FadeOut
        {
            _onExpired?.Invoke(_key);
            Remove();
        }
    }
}
