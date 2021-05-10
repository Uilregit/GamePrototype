using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoomSetup))]
public class LevelScriptEditor : Editor
{

    public bool showLevels = true;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();

        DrawDefaultInspector();

        RoomSetup levels = (RoomSetup)target;

        GUIStyle rowStyle = new GUIStyle();
        rowStyle.fixedHeight = 25;
        GUIStyle enumStyle = new GUIStyle("popup");
        rowStyle.fixedWidth = 175;

        EditorGUILayout.BeginHorizontal(rowStyle);
        for (int j = 6; j >= 0; j--)
            levels.level1[j] = (RoomSetup.BoardType)EditorGUILayout.EnumPopup(levels.level1[j], enumStyle);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(rowStyle);
        for (int j = 6; j >= 0; j--)
            levels.level2[j] = (RoomSetup.BoardType)EditorGUILayout.EnumPopup(levels.level2[j], enumStyle);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(rowStyle);
        for (int j = 6; j >= 0; j--)
            levels.level3[j] = (RoomSetup.BoardType)EditorGUILayout.EnumPopup(levels.level3[j], enumStyle);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(rowStyle);
        for (int j = 6; j >= 0; j--)
            levels.level4[j] = (RoomSetup.BoardType)EditorGUILayout.EnumPopup(levels.level4[j], enumStyle);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(rowStyle);
        for (int j = 6; j >= 0; j--)
            levels.level5[j] = (RoomSetup.BoardType)EditorGUILayout.EnumPopup(levels.level5[j], enumStyle);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(rowStyle);
        for (int j = 6; j >= 0; j--)
            levels.level6[j] = (RoomSetup.BoardType)EditorGUILayout.EnumPopup(levels.level6[j], enumStyle);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(rowStyle);
        for (int j = 6; j >= 0; j--)
            levels.level7[j] = (RoomSetup.BoardType)EditorGUILayout.EnumPopup(levels.level7[j], enumStyle);
        EditorGUILayout.EndHorizontal();


        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
            EditorUtility.SetDirty(levels);
    }
}

