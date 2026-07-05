using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Encoder;
using UnityEditor.Recorder.Input;
using UnityEngine;
using static Define;

/// <summary>
/// 슬라이드 3(Unlock Anim) 수정안 검증용 자동 녹화.
/// Test/Record/여정 해금 연출 녹화 클릭 한 번으로:
///   플레이 진입 → GameScene 준비 대기 → Recorder(MP4, 30fps) 시작
///   → 여정 버튼 해금 트리거 → 연출+팝업 대기 → 팝업 닫기([!]+NEW 확인 구간)
///   → 녹화 종료 후 Documents 폴더에 저장 → 플레이 종료.
/// </summary>
public static class UnlockAnimRecorder
{
    const string PENDING_KEY = "UnlockAnimRecorder.Pending";
    static readonly string OUTPUT_DIR = Application.dataPath.Replace("/Assets", "") + "/Documents";

    [MenuItem("Test/Record/여정 해금 연출 녹화 (Documents 저장)")]
    static void Record()
    {
        if (!Application.isPlaying)
        {
            SessionState.SetBool(PENDING_KEY, true);
            EditorApplication.isPlaying = true;   // 플레이 진입 후 자동으로 캡처 시작
            return;
        }
        _ = RunCapture();
    }

    [InitializeOnLoadMethod]
    static void Hook()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(PENDING_KEY, false))
            {
                SessionState.SetBool(PENDING_KEY, false);
                _ = RunCapture();
            }
        };
    }

    static async Task RunCapture()
    {
        RecorderController controller = null;
        try
        {
            // 1) GameScene 준비 대기 (최대 30초)
            float deadline = Time.realtimeSinceStartup + 30f;
            while (Time.realtimeSinceStartup < deadline &&
                   (Managers.Game == null || Managers.Game.Player == null || !(Managers.UI?.SceneUI is UI_GameScene)))
                await Task.Delay(200);
            if (Managers.Game == null || Managers.Game.Player == null)
            {
                Debug.LogError("[UnlockAnimRecorder] GameScene 준비 실패 — GameScene에서 실행해 주세요.");
                return;
            }
            await Task.Delay(1500);   // 마을 버튼 그룹 초기 연출 대기

            // 2) Recorder 구성 (게임뷰 → MP4, 30fps)
            var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            var movie = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            movie.name = "UnlockAnim";
            movie.Enabled = true;
            movie.EncoderSettings = new CoreEncoderSettings
            {
                Codec = CoreEncoderSettings.OutputCodec.MP4,
                EncodingQuality = CoreEncoderSettings.VideoEncodingQuality.High,
            };
            movie.ImageInputSettings = new GameViewInputSettings();
            movie.OutputFile = $"{OUTPUT_DIR}/UnlockAnim_{DateTime.Now:MMdd_HHmmss}";
            controllerSettings.AddRecorderSettings(movie);
            controllerSettings.SetRecordModeToManual();
            controllerSettings.FrameRate = 30;
            controller = new RecorderController(controllerSettings);
            controller.PrepareRecording();
            controller.StartRecording();
            Debug.Log("[UnlockAnimRecorder] 녹화 시작");

            await Task.Delay(500);

            // 3) 여정 버튼 해금 트리거 (퀘스트 완료와 동일한 이벤트 경로)
            Managers.Quest.RaiseBtnUnlocked(UNLOCK_TRAVEL_BUTTON);

            // 4) 연출 대기: 0.5s(지연) + 2s(페이드+버스트) + 1s(글로우 플래시) + 팝업 노출 여유
            await Task.Delay(6500);

            // 5) 해금 팝업 닫기 → [!] Quest Mark + NEW 동시 표시 확인 구간
            Managers.UI.ClosePopupUI();
            await Task.Delay(2500);

            controller.StopRecording();
            controller = null;
            Debug.Log($"[UnlockAnimRecorder] 저장 완료: {movie.OutputFile}.mp4");

            await Task.Delay(500);
            EditorApplication.isPlaying = false;
        }
        catch (Exception e)
        {
            Debug.LogError("[UnlockAnimRecorder] 실패: " + e);
            controller?.StopRecording();
        }
    }
}
