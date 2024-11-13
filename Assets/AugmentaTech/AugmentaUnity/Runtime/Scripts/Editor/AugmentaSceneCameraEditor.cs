using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Augmenta
{
    [CustomEditor(typeof(AugmentaSceneCamera))]
    public class AugmentaSceneCameraEditor : Editor
    {
        SerializedProperty augmentaManager;
        SerializedProperty cameraType;
        SerializedProperty distanceToScene;

        void OnEnable() {

            augmentaManager = serializedObject.FindProperty("augmentaManager");
            cameraType = serializedObject.FindProperty("cameraType");
            distanceToScene = serializedObject.FindProperty("distanceToScene");
        }

        public override void OnInspectorGUI() {

            serializedObject.Update();

            EditorGUILayout.PropertyField(augmentaManager, new GUIContent("Augmenta Manager"));
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(cameraType, new GUIContent("Camera Type"));
            EditorGUILayout.Space();

            switch (cameraType.enumValueIndex) {

                case 0: //Orthographic
                    EditorGUILayout.PropertyField(distanceToScene, new GUIContent("Distance To Scene"));
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("In Orthographic mode, the camera is always facing and centered on the Augmenta Scene.", MessageType.Info);
                    break;

                case 1: //Perspective
                    EditorGUILayout.PropertyField(distanceToScene, new GUIContent("Distance To Scene"));
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("In Perspective mode, the camera is always facing and centered on the Augmenta Scene.", MessageType.Info);
                    break;

                case 2: //Offcenter
                    EditorGUILayout.HelpBox("In OffCenter mode, you can move the camera freely but it will always be facing the Augmenta Scene.", MessageType.Info);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
