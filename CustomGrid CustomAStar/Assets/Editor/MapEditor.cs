using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(Map))]
public class MapEditor : Editor
{
    private Map map;
    string loadFile;
    string saveFile;


    private bool setPathPosToggle = false;


    private void OnEnable()
    {
        map = target as Map;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();

        if (GUILayout.Button("Create Map"))
        {
            map.CreateMap();
        }

        if (GUILayout.Button("Reset Map"))
        {
            map.ResetMap();
        }

        EditorGUILayout.Space(20);

        if (GUILayout.Button("Get all possible paths"))
        {
            map.GetAllPossiblePaths();
        }

        if (EditorGUILayout.Toggle("Should Render Possible Paths", map.shouldRenderPossiblePaths))
            map.shouldRenderPossiblePaths = true;
        else
            map.shouldRenderPossiblePaths = false;

        if (EditorGUILayout.Toggle("Should Render Grid", map.shouldRenderGrid))
            map.shouldRenderGrid = true;
        else
            map.shouldRenderGrid = false;

        if (EditorGUILayout.Toggle("should Render PathFinding Debug Tools", map.shouldRenderPathFindingDebugTools))
            map.shouldRenderPathFindingDebugTools = true;
        else
            map.shouldRenderPathFindingDebugTools = false;

        

        EditorGUILayout.Space(20);


        saveFile = EditorGUILayout.TextField("Name Of File To Save", saveFile);

        if (GUILayout.Button("Save Map"))
        {
            map.Save("/" + saveFile + ".txt");
        }

        EditorGUILayout.Space(10);

        loadFile = EditorGUILayout.TextField("Name Of File To Load", loadFile);

        if (GUILayout.Button("Load Map"))
        {
            map.Load("/" + loadFile + ".txt");
        }

    }


    void OnSceneGUI()
    {
        Event guiEvent = Event.current;

        Vector3 mousePosition = Event.current.mousePosition;
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
        {
            map.SetValue(ray.origin);
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
        {
            if (setPathPosToggle)
                map.SetStartPos(ray.origin);
            else
                map.SetEndPos(ray.origin);

            setPathPosToggle = !setPathPosToggle;
        }

        if (guiEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
    }

}


