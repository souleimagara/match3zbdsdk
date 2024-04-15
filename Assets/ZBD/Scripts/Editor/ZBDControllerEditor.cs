using UnityEngine;
using UnityEditor;

namespace ZBD
{
    [CustomEditor(typeof(ZBDController))]
    public class ZBDControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DrawDefaultInspector();
            
            var booleanProperty = serializedObject.FindProperty("autostart"); 
            
            if (!booleanProperty.boolValue)
            {
                EditorGUILayout.HelpBox("You must init the Rewards SDK manually if autostart is not selected", MessageType.Info);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}