using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Encoder;
using UnityEditor.Recorder.Input;
using UnityEngine;

/// <summary>
/// 완성 결과물 시연 녹화. 메뉴 한 번으로:
///   플레이 진입 → GameScene 준비 대기 → Recorder(MP4, 30fps) 시작
///   → 지역별 몬스터 등장(각자 전용 애니메이터/색조) → 장비 성장 시연
///   → 녹화 종료 후 Documents 저장 → 플레이 종료.
/// 구성은 UnlockAnimRecorder와 동일한 방식(Manual 모드 + GameView 입력).
/// </summary>
public static class ShowcaseRecorder
{
    const string PENDING_KEY = "ShowcaseRecorder.Pending";
    static readonly string OUTPUT_DIR = Application.dataPath.Replace("/Assets", "") + "/Documents";

    [MenuItem("Test/Record/완성 결과물 시연 녹화 (Documents 저장)")]
    static void Record()
    {
        if (!Application.isPlaying)
        {
            SessionState.SetBool(PENDING_KEY, true);
            EditorApplication.isPlaying = true;
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
        int origLevel = -1;
        try
        {
            // 플레이어 생성만 기다리면 안 된다. 데이터 딕셔너리는 PreLoad(900여 개) 완료 후에야
            // Managers.Data.Init()에서 채워지므로, CreatureDic이 찰 때까지 기다려야 몬스터가 스폰된다.
            float deadline = Time.realtimeSinceStartup + 120f;
            while (Time.realtimeSinceStartup < deadline &&
                   (Managers.Game == null || Managers.Game.Player == null ||
                    Managers.Data == null || Managers.Data.CreatureDic.Count == 0))
                await Task.Delay(200);
            if (Managers.Game?.Player == null || Managers.Data.CreatureDic.Count == 0)
            {
                Debug.LogError("[Showcase] 준비 실패 — 데이터 로드가 끝나지 않았습니다");
                return;
            }
            await Task.Delay(1200);

            var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            var movie = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            movie.name = "Showcase";
            movie.Enabled = true;
            movie.EncoderSettings = new CoreEncoderSettings
            {
                Codec = CoreEncoderSettings.OutputCodec.MP4,
                EncodingQuality = CoreEncoderSettings.VideoEncodingQuality.High,
            };
            movie.ImageInputSettings = new GameViewInputSettings();
            movie.OutputFile = $"{OUTPUT_DIR}/Showcase_{DateTime.Now:MMdd_HHmmss}";
            controllerSettings.AddRecorderSettings(movie);
            controllerSettings.SetRecordModeToManual();
            controllerSettings.FrameRate = 30;
            controller = new RecorderController(controllerSettings);
            controller.PrepareRecording();
            controller.StartRecording();
            Debug.Log("[Showcase] 녹화 시작");
            await Task.Delay(800);

            var player = Managers.Game.Player;
            origLevel = Managers.Game.Level;

            // 1) 실제 전투 시연 — 지역을 바꿔가며 전투 진입.
            //    마을 씬에 강제 스폰한 몬스터는 게임 카메라에 잡히지 않으므로
            //    BattleSpawner가 정상 스폰하는 전투 지역으로 들어가 실제 전투 화면을 담는다.
            var scene = Managers.UI.SceneUI as UI_GameScene;
            // 신들바람 숲만 사용한다. 동굴/성소 전투 구역은 배경 아트가 아직 없어 검은 화면이 나온다.
            Managers.Game.CurrentTravelAreaId = 21100;
            scene?.EnterAreaWithFade(Define.AreaType.Battle);
            // 페이드(약 1s) + 첫 몬스터 스폰(2s) + 우측 x+12에서 걸어오는 시간 + 교전 반복
            await Task.Delay(22000);
            scene?.EnterAreaWithFade(Define.AreaType.Village1);
            await Task.Delay(2000);

            // 2) 성장 시연 — 레벨 구간을 올리며 스탯이 따라 오르는 것을 보여준다
            foreach (int lv in new[] { 1, 50, 150, 300 })
            {
                Managers.Game.Level = lv;
                player.RefreshStats();
                await Task.Delay(1400);
            }

            controller.StopRecording();
            controller = null;
            Debug.Log($"[Showcase] 저장 완료: {movie.OutputFile}.mp4");
            await Task.Delay(500);
        }
        catch (Exception e)
        {
            Debug.LogError("[Showcase] 실패: " + e);
            controller?.StopRecording();
        }
        finally
        {
            // 시연으로 바꾼 레벨은 원복 (세이브 오염 방지)
            if (origLevel > 0 && Managers.Game != null)
            {
                Managers.Game.Level = origLevel;
                Managers.Game.Player?.RefreshStats();
            }
            EditorApplication.isPlaying = false;
        }
    }
}
