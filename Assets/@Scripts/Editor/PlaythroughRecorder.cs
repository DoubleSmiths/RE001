using System;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Encoder;
using UnityEditor.Recorder.Input;
using UnityEngine;

/// <summary>
/// 손으로 플레이하는 동안 화면을 녹화한다.
///
/// ShowcaseRecorder는 스스로 정해진 시연을 돌리고 끝나면 저장한다 — "완성본 보여주기"용이다.
/// 이쪽은 **시작과 종료를 갈라 놓았다**. 사이에 무엇을 하든(퀘스트 진행, 전투, 팝업 조작)
/// 그대로 담기므로, 실제 플레이스루를 증빙으로 남길 수 있다.
///
/// 저장: Documents/Playthrough_MMdd_HHmmss.mp4 (30fps, GameView)
/// </summary>
public static class PlaythroughRecorder
{
    static readonly string OUTPUT_DIR = Application.dataPath.Replace("/Assets", "") + "/Documents";

    static RecorderController _controller;
    static string _output;

    public static bool IsRecording => _controller != null && _controller.IsRecording();
    public static string OutputPath => _output;

    [MenuItem("Test/Record/플레이스루 녹화 시작")]
    public static string StartRecording()
    {
        if (!Application.isPlaying)
            return "[녹화] 플레이 모드에서 실행하세요.";
        if (IsRecording)
            return $"[녹화] 이미 녹화 중: {_output}.mp4";

        var settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        var movie = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        movie.name = "Playthrough";
        movie.Enabled = true;
        movie.EncoderSettings = new CoreEncoderSettings
        {
            Codec = CoreEncoderSettings.OutputCodec.MP4,
            EncodingQuality = CoreEncoderSettings.VideoEncodingQuality.High,
        };
        movie.ImageInputSettings = new GameViewInputSettings();

        _output = $"{OUTPUT_DIR}/Playthrough_{DateTime.Now:MMdd_HHmmss}";
        movie.OutputFile = _output;

        settings.AddRecorderSettings(movie);
        settings.SetRecordModeToManual();
        settings.FrameRate = 30;

        _controller = new RecorderController(settings);
        _controller.PrepareRecording();
        _controller.StartRecording();

        Debug.Log($"[녹화] 시작 → {_output}.mp4");
        return $"[녹화] 시작 → {_output}.mp4";
    }

    [MenuItem("Test/Record/플레이스루 녹화 종료")]
    public static string StopRecording()
    {
        if (!IsRecording) return "[녹화] 녹화 중이 아닙니다.";

        _controller.StopRecording();
        _controller = null;
        Debug.Log($"[녹화] 저장 완료 → {_output}.mp4");
        return $"[녹화] 저장 완료 → {_output}.mp4";
    }
}
