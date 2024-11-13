using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Augmenta
{
    [CustomEditor(typeof(AugmentaVideoOutputCamera))]
    public class AugmentaVideoOutputCameraEditor : Editor
    {
        SerializedProperty augmentaVideoOutput;
        SerializedProperty cameraType;
        SerializedProperty distanceToVideoOutput;

        void OnEnable() {

            augmentaVideoOutput = serializedObject.FindProperty("augmentaVideoOutput");
            cameraType = serializedObject.FindProperty("cameraType");
            distanceToVideoOutput = serializedObject.FindProperty("distanceToVideoOutput");
        }

        public override void OnInspectorGUI() {

            serializedObject.Update();

            EditorGUILayout.PropertyField(augmentaVideoOutput, new GUIContent("Augmenta Video Output"));
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(cameraType, new GUIContent("Camera Type"));
            EditorGUILayout.Space();

            switch (cameraType.enumValueIndex) {

                case 0: //Orthographic
                    EditorGUILayout.PropertyField(distanceToVideoOutput, new GUIContent("Distance To Video Output"));
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("In Orthographic mode, the camera is always facing and centered on the Video Output.", MessageType.Info);
                    break;

                case 1: //Perspective
                    EditorGUILayout.PropertyField(distanceToVideoOutput, new GUIContent("Distance To Video Output"));
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("In Perspective mode, the camera is always facing and centered on the Video Output.", MessageType.Info);
                    break;

                case 2: //Offcenter
                    EditorGUILayout.HelpBox("In OffCenter mode, you can move the camera freely but it will always be facing the Video Output.", MessageType.Info);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
