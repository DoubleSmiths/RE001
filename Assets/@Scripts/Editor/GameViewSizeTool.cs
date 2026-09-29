using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 게임뷰 해상도를 코드로 고정한다 — unity-cli 로 폰 화면비 캡처·검증을 하려면 Screen.width/height 가
/// 실제 기기 크기여야 한다(스크린샷 옵션의 크기는 카메라만 다시 그리고 레이아웃은 그대로라 잘린다).
/// UnityEditor 내부 API(GameViewSizes)를 리플렉션으로 쓴다. 사용: GameViewSizeTool.Set(1080, 2340)
/// </summary>
public static class GameViewSizeTool
{
    const BindingFlags ANY = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    public static string Set(int width, int height)
    {
        Assembly asm = typeof(Editor).Assembly;
        Type sizesType = asm.GetType("UnityEditor.GameViewSizes");
        Type sizeType = asm.GetType("UnityEditor.GameViewSize");
        Type sizeKind = asm.GetType("UnityEditor.GameViewSizeType");
        Type viewType = asm.GetType("UnityEditor.GameView");

        object sizes = typeof(ScriptableSingleton<>).MakeGenericType(sizesType).GetProperty("instance").GetValue(null);
        object groupType = sizesType.GetProperty("currentGroupType", ANY).GetValue(sizes);
        object group = sizesType.GetMethod("GetGroup", ANY).Invoke(sizes, new[] { groupType });
        Type groupT = group.GetType();

        string label = $"RE001 {width}x{height}";
        int total = (int)groupT.GetMethod("GetTotalCount", ANY).Invoke(group, null);
        int index = -1;
        for (int i = 0; i < total; i++)
        {
            object s = groupT.GetMethod("GetGameViewSize", ANY).Invoke(group, new object[] { i });
            if ((int)sizeType.GetProperty("width", ANY).GetValue(s) == width &&
                (int)sizeType.GetProperty("height", ANY).GetValue(s) == height &&
                sizeType.GetProperty("sizeType", ANY).GetValue(s).ToString() == "FixedResolution")
            { index = i; break; }
        }
        if (index < 0)
        {
            object size = Activator.CreateInstance(sizeType, Enum.Parse(sizeKind, "FixedResolution"), width, height, label);
            groupT.GetMethod("AddCustomSize", ANY).Invoke(group, new[] { size });
            index = total;
        }

        EditorWindow view = EditorWindow.GetWindow(viewType, false, null, false);
        PropertyInfo selected = viewType.GetProperty("selectedSizeIndex", ANY);
        if (selected != null) selected.SetValue(view, index);
        else viewType.GetMethod("SizeSelectionCallback", ANY).Invoke(view, new object[] { index, null });
        view.Repaint();
        return $"GameView → {width}x{height} (index {index})";
    }
}
