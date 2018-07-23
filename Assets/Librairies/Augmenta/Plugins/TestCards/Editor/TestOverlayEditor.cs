using UnityEditor;
using UnityEngine;

namespace TestCards
{
    [CustomEditor(typeof(TestOverlay))]
    public class TestOverlayEditor : Editor
    {
        SerializedProperty _mode;
        SerializedProperty _color;
        SerializedProperty _scale;
        SerializedProperty _shader;

        void OnEnable()
        {
            _mode = serializedObject.FindProperty("_mode");
            _color = serializedObject.FindProperty("_color");
            _scale = serializedObject.FindProperty("_scale");
            _shader = serializedObject.FindProperty("_shader");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_mode);
            EditorGUILayout.PropertyField(_shader);

            if (_mode.intValue == (int)TestOverlay.Mode.Fill)
                EditorGUILayout.PropertyField(_color);

            if (_mode.intValue == (int)TestOverlay.Mode.Checker)
                EditorGUILayout.PropertyField(_scale);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
