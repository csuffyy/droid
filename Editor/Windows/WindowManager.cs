﻿#if UNITY_EDITOR
using System;
using UnityEditor;

namespace Neodroid.Editor.Windows {
  public class WindowManager : EditorWindow {
    static Type[] _desired_dock_next_toos = {
        typeof(RenderTextureConfiguratorWindow),
        typeof(CameraSynchronisationWindow),
        typeof(DebugWindow),
        typeof(SegmentationWindow),
        typeof(EnvironmentsWindow),
        typeof(TaskWindow),
        typeof(DemonstrationWindow),
        typeof(SimulationWindow)
    };

    [MenuItem(EditorWindowMenuPath._WindowMenuPath + "ShowAllWindows")]
    [MenuItem(EditorWindowMenuPath._ToolMenuPath + "ShowAllWindows")]
    public static void ShowWindow() {
      //Show existing window instance. If one doesn't exist, make one.
      GetWindow<RenderTextureConfiguratorWindow>(_desired_dock_next_toos);
      GetWindow<CameraSynchronisationWindow>(_desired_dock_next_toos);
      GetWindow<DebugWindow>(_desired_dock_next_toos);
      GetWindow<SegmentationWindow>(_desired_dock_next_toos);
      GetWindow<EnvironmentsWindow>(_desired_dock_next_toos);
      GetWindow<TaskWindow>(_desired_dock_next_toos);
      GetWindow<DemonstrationWindow>(_desired_dock_next_toos);
      GetWindow<SimulationWindow>(_desired_dock_next_toos);
    }
  }
}
#endif