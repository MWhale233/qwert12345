#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FresnelGenerator))]
public class FresnelGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FresnelGenerator generator = (FresnelGenerator)target;
        
        EditorGUI.BeginDisabledGroup(generator.IsCalculating);
        if (GUILayout.Button("Generate Volume"))
        {
            generator.GenerateVolume();
        }
        EditorGUI.EndDisabledGroup();

        if (generator.IsCalculating)
        {
            EditorGUILayout.HelpBox("Calculating...", MessageType.Info);
        }
    }
}
#endif