using UnityEngine;

// 맞은 순간 몸 전체가 하얗게 번쩍인다(타격감, ADR0033). 예전엔 0.3초에 걸쳐 빨갛게 물들었다가 0.3초에 걸쳐 빠져서,
//  연타하면 몬스터가 내내 검붉은 덩어리로 보였다. 본체 위에 같은 스프라이트를 흰 실루엣으로 한 겹 더 그려 알파만 뺀다.
//  재질은 보스 테두리와 같은 RE001/SpriteSilhouette(어드레서블 'BossOutline') — 모양만 살리고 색은 렌더러 색 하나로 칠한다.
//  애니메이터가 스프라이트를 매 프레임 바꾸므로 LateUpdate 에서 따라간다. 전투 정지 중에도 끝나도록 실시간으로 뺀다.
public class HitFlash : MonoBehaviour
{
    SpriteRenderer _body;
    SpriteRenderer _flash;
    float _left, _duration;

    public static void Play(GameObject target, float seconds)
    {
        if (target == null) return;
        var hf = target.GetComponent<HitFlash>();
        if (hf == null) hf = target.AddComponent<HitFlash>();
        hf.Flash(seconds);
    }

    void Awake()
    {
        _body = GetComponent<SpriteRenderer>();
        var mat = Managers.Resource.Load<Material>("BossOutline");
        if (_body == null || mat == null) { enabled = false; return; }

        var go = new GameObject("HitFlash");
        go.transform.SetParent(transform, false);
        _flash = go.AddComponent<SpriteRenderer>();
        _flash.sharedMaterial = mat;
        _flash.sortingLayerID = _body.sortingLayerID;
        _flash.sortingOrder   = _body.sortingOrder + 1;
        _flash.enabled = false;
    }

    public void Flash(float seconds)
    {
        if (_flash == null) return;
        _duration = _left = Mathf.Max(0.01f, seconds);
        _flash.enabled = true;
        Sync();
    }

    void LateUpdate()
    {
        if (_flash == null || !_flash.enabled) return;
        _left -= Time.unscaledDeltaTime;
        if (_left <= 0f) { _flash.enabled = false; return; }
        Sync();
    }

    void Sync()
    {
        _flash.sprite = _body.sprite;
        _flash.flipX  = _body.flipX;
        _flash.color  = new Color(1f, 1f, 1f, _body.color.a * Mathf.Clamp01(_left / _duration));
    }

    void OnDisable()
    {
        if (_flash != null) _flash.enabled = false;
    }
}
