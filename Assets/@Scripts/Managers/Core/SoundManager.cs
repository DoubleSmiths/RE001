using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SoundManager
{
    private AudioSource[] _audioSources = new AudioSource[(int)Define.Sound.Max];
    private Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();

    private GameObject _soundRoot = null;

    // 배경음(BGM) 크로스페이드 — RE001_Share의 BGM Player(목표 볼륨 0.4, SmoothStep 페이드) 상당.
    const float BGM_VOLUME = 0.4f;
    const float BGM_FADE_OUT = 0.35f;
    const float BGM_FADE_IN  = 0.6f;
    private string _currentBgmKey = null;

    public void Init()
    {
        if (_soundRoot == null)
        {
            _soundRoot = GameObject.Find("@SoundRoot");
            if (_soundRoot == null)
            {
                _soundRoot = new GameObject { name = "@SoundRoot" };
                UnityEngine.Object.DontDestroyOnLoad(_soundRoot);

                string[] soundTypeNames = System.Enum.GetNames(typeof(Define.Sound));
                for (int count = 0; count < soundTypeNames.Length - 1; count++)
                {
                    GameObject go = new GameObject { name = soundTypeNames[count] };
                    _audioSources[count] = go.AddComponent<AudioSource>();
                    go.transform.parent = _soundRoot.transform;
                }

                _audioSources[(int)Define.Sound.Bgm].loop = true;
                _audioSources[(int)Define.Sound.SubBgm].loop = true;
            }
        }
    }

    public void Clear()
    {
        foreach (AudioSource audioSource in _audioSources)
            audioSource.Stop();
        _audioClips.Clear();
        _currentBgmKey = null;   // 멈춘 곡을 '재생 중'으로 기억하면 다음 씬의 같은 곡 요청이 무시된다
    }

    public void Play(Define.Sound type)
    {
        AudioSource audioSource = _audioSources[(int)type];
        audioSource.Play();
    }

    public void Play(Define.Sound type, string key, float pitch = 1.0f)
    {
        AudioSource audioSource = _audioSources[(int)type];

        if (type == Define.Sound.Bgm || type == Define.Sound.SubBgm)
        {
            LoadAudioClip(key, (audioClip) =>
            {
                if (audioClip == null) return;
                if (audioSource.isPlaying)
                    audioSource.Stop();

                audioSource.clip = audioClip;
                if (Managers.Game != null && Managers.Game.IsBGSoundOn)
                    audioSource.Play();
            });
        }
        else
        {
            LoadAudioClip(key, (audioClip) =>
            {
                if (audioClip == null) return;
                audioSource.pitch = pitch;
                if (Managers.Game != null && Managers.Game.IsSFXOn)
                    audioSource.PlayOneShot(audioClip);
            });
        }
    }

    public void Play(Define.Sound type, AudioClip audioClip, float pitch = 1.0f)
    {
        AudioSource audioSource = _audioSources[(int)type];

        if (audioClip == null) return;

        if (type == Define.Sound.Bgm || type == Define.Sound.SubBgm)
        {
            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.clip = audioClip;
            if (Managers.Game != null && Managers.Game.IsBGSoundOn)
                audioSource.Play();
        }
        else
        {
            audioSource.pitch = pitch;
            if (Managers.Game != null && Managers.Game.IsSFXOn)
                audioSource.PlayOneShot(audioClip);
        }
    }

    public void Stop(Define.Sound type)
    {
        AudioSource audioSource = _audioSources[(int)type];
        audioSource.Stop();
    }

    // ── 배경음(BGM) 크로스페이드 재생 ──
    // 같은 곡이면 무시. 현재 곡을 페이드아웃 후 새 곡을 페이드인(목표 볼륨 0.4).
    // DOTween.SetUpdate(true)로 timeScale=0(팝업/일시정지) 중에도 페이드 진행.
    public void PlayBgm(string key, bool restartIfSame = false)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (!restartIfSame && key == _currentBgmKey) return;
        _currentBgmKey = key;

        AudioSource src = _audioSources[(int)Define.Sound.Bgm];
        if (src == null) return;

        LoadAudioClip(key, (clip) =>
        {
            if (clip == null) return;
            DOTween.Kill(src);

            if (src.isPlaying && src.clip != null)
            {
                // 현재 곡 페이드아웃 → 교체 → 페이드인
                DOTween.To(() => src.volume, v => src.volume = v, 0f, BGM_FADE_OUT)
                       .SetTarget(src).SetUpdate(true)
                       .OnComplete(() => StartBgmFadeIn(src, clip));
            }
            else
            {
                StartBgmFadeIn(src, clip);
            }
        });
    }

    private void StartBgmFadeIn(AudioSource src, AudioClip clip)
    {
        src.clip   = clip;
        src.loop   = true;
        src.volume = 0f;
        if (Managers.Game != null && Managers.Game.IsBGSoundOn)
            src.Play();

        DOTween.To(() => src.volume, v => src.volume = v, BGM_VOLUME, BGM_FADE_IN)
               .SetTarget(src).SetUpdate(true);
    }

    // 설정에서 배경음을 켜고 끌 때 재생 중인 곡에 바로 반영한다.
    //  (예전엔 켜짐 여부를 곡을 **새로 틀 때만** 봐서, 끄기를 눌러도 지금 곡이 계속 나오고
    //   다시 켜도 다음 지역 이동 전까지 조용했다)
    public void ApplyBgmEnabled(bool on)
    {
        AudioSource bgm = _audioSources[(int)Define.Sound.Bgm];
        AudioSource sub = _audioSources[(int)Define.Sound.SubBgm];
        if (!on)
        {
            if (bgm != null) { DOTween.Kill(bgm); bgm.Stop(); }
            if (sub != null) sub.Stop();
            return;
        }
        if (bgm != null && bgm.clip != null && !bgm.isPlaying) StartBgmFadeIn(bgm, bgm.clip);
        if (sub != null && sub.clip != null && !sub.isPlaying) sub.Play();
    }

    // 임시 BGM(잡기 등) — 현재 곡을 저장하고 새 곡 재생. PopBgm으로 원래 곡 복귀.
    private string _savedBgmKey = null;
    public void PushBgm(string key)
    {
        _savedBgmKey = _currentBgmKey;
        PlayBgm(key);
    }
    public void PopBgm()
    {
        string restore = _savedBgmKey;
        _savedBgmKey = null;
        if (!string.IsNullOrEmpty(restore))
            PlayBgm(restore);
        else
            StopBgm();
    }

    public void StopBgm(bool fade = true)
    {
        AudioSource src = _audioSources[(int)Define.Sound.Bgm];
        if (src == null) return;
        _currentBgmKey = null;
        DOTween.Kill(src);
        if (fade)
            DOTween.To(() => src.volume, v => src.volume = v, 0f, BGM_FADE_OUT)
                   .SetTarget(src).SetUpdate(true).OnComplete(() => src.Stop());
        else
            src.Stop();
    }

    // SFX 단축 헬퍼 — 키 = 어드레서블 클립명(파일명). SoundAddressableRegistrar가 등록한 주소.
    public void PlaySFX(string key, float pitch = 1.0f) => Play(Define.Sound.Effect, key, pitch);

    public void PlayButtonClick()
    {
        Play(Define.Sound.Effect, "SFX_BtnSelect");
    }

    public void PlayPopupClose()
    {
        Play(Define.Sound.Effect, "SFX_Popup");
    }
    private void LoadAudioClip(string key, Action<AudioClip> callback)
    {
        AudioClip audioClip = null;
        if (_audioClips.TryGetValue(key, out audioClip))
        {
            callback?.Invoke(audioClip);
            return;
        }

        audioClip = Managers.Resource.Load<AudioClip>(key);

        if (!_audioClips.ContainsKey(key))
            _audioClips.Add(key, audioClip);

        callback?.Invoke(audioClip);

        //Managers.Resource.LoadAsync<AudioClip>(key, (audioClip) =>
        //{
        //    if (!_audioClips.ContainsKey(key))
        //        _audioClips.Add(key, audioClip);
        //    callback?.Invoke(audioClip);
        //});
    }
}
