using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using static Define;

// 플레이 후 남은 백업이 정상 종료한 다음에도 복구 안내를 띄우는 문제를 막는다.
[InitializeOnLoad]
public static class SceneBackupArchive
{
    static SceneBackupArchive()
    {
        EditorApplication.quitting += OnEditorQuitting;
    }

    static void OnEditorQuitting()
    {
        // 종료가 확정된 시점에만 보관한다. 강제 종료/크래시의 복구 경로는 유지한다.
        try
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string source = Path.GetFullPath(Path.Combine(projectRoot, EDITOR_SCENE_BACKUP_FOLDER));
            if (!Directory.Exists(source)) return;
            string archiveRoot = Path.GetFullPath(Path.Combine(projectRoot, EDITOR_SCENE_ARCHIVE_FOLDER));
            string destination = Path.GetFullPath(Path.Combine(archiveRoot, Guid.NewGuid().ToString("N")));
            ValidatePath(projectRoot, source);
            ValidatePath(projectRoot, destination);
            Directory.CreateDirectory(archiveRoot);
            Directory.Move(source, destination);
            Debug.Log($"[SceneBackupArchive] 정상 종료 백업 보관: {destination}");
        }
        catch (Exception exception)
        {
            // 보관 실패 시 원래 백업을 남겨 복구할 수 있게 한다.
            Debug.LogWarning($"[SceneBackupArchive] 백업을 유지합니다: {exception.Message}");
        }
    }

    static void ValidatePath(string projectRoot, string path)
    {
        if (!path.StartsWith(projectRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new IOException("프로젝트 외부 백업 경로입니다.");
        for (var directory = new DirectoryInfo(path); directory != null; directory = directory.Parent)
        {
            if (directory.Exists && (directory.Attributes & FileAttributes.ReparsePoint) != 0)
                throw new IOException("링크된 백업 경로는 이동하지 않습니다.");
            if (string.Equals(directory.FullName, projectRoot, StringComparison.OrdinalIgnoreCase)) break;
        }
    }
}
