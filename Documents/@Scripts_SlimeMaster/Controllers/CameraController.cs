using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region 변수
    public Transform _playerTransform;
    public float Height { get; set; } = 0;
    public float Width { get; set; } = 0;

    [SerializeField]
    float _tickvalue = 5;
    [SerializeField]
    float _adjust = 0.5f;
    bool _isShake = false;

    #endregion
   
    void Start()
    {
        SetCameraSize();
    }

    void SetCameraSize()
    {
        Camera.main.orthographicSize = 21f;
        Height = Camera.main.orthographicSize;
        Width = Height * Screen.width / Screen.height;
    }

    void LateUpdate()
    {
        if (_playerTransform != null && Managers.Game.CurrentMap != null && _isShake == false)
            LimitCameraArea();
    }

    void LimitCameraArea()
    {
        transform.position = new Vector3(_playerTransform.position.x, _playerTransform.position.y, -10f);

        float limitX = Managers.Game.CurrentMap.MapSize.x * 0.5f - Width;
        float clampX = Mathf.Clamp(transform.position.x, -limitX, limitX);

        float limitY = Managers.Game.CurrentMap.MapSize.y * 0.5f - Height;
        float clampY = Mathf.Clamp(transform.position.y, -limitY, limitY);

        transform.position = new Vector3(clampX, clampY, -10f);
    }

    Vector3 camPos;
    public void Shake()
    {
        if(_isShake == false)
            StartCoroutine(CoShake(0.25f));
    }

    

    IEnumerator CoShake(float duration)
    {
        float halfDuration = duration / 2;
        float elapsed = 0f;
        float tick = Random.Range(-10f, 10f);
        _isShake = true;
        while (elapsed < duration)
        {
            if (Managers.UI.GetPopupCount() > 0)
                break;
            
            elapsed += Time.deltaTime / halfDuration;

            tick += Time.deltaTime * _tickvalue;
            transform.position += new Vector3(
                Mathf.PerlinNoise(tick, 0) - .5f,
                Mathf.PerlinNoise(0, tick) - .5f,
                0f) * _adjust * Mathf.PingPong(elapsed, halfDuration);
            yield return null;
        }
        //yield return new WaitForSeconds(0.1f);
        _isShake = false;
    }
}
