﻿#if UNITY_EDITOR
using Neodroid.Editor.Windows;
using Neodroid.Runtime.Utilities.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Neodroid.Editor.ScriptableObjects {
  public static class CreateNeodroidTask {
    [MenuItem(EditorScriptableObjectMenuPath._ScriptableObjectMenuPath + "NeodroidTask")]
    public static void CreateNeodroidTaskAsset() {
      var asset = ScriptableObject.CreateInstance<NeodroidTask>();

      AssetDatabase.CreateAsset(asset, EditorWindowMenuPath._NewAssetPath + "Assets/NewNeodroidTask.asset");
      AssetDatabase.SaveAssets();

      EditorUtility.FocusProjectWindow();

      Selection.activeObject = asset;
    }
  }
}
#endif