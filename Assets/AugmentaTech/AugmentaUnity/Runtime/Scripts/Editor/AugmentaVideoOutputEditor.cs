using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Augmenta
{
    [CustomEditor(typeof(AugmentaVideoOutput))]
    public class AugmentaVideoOutputEditor : Editor
    {
        SerializedProperty augmentaManager;
        SerializedProperty augmentaVideoOutputCamera;

        SerializedProperty cameraMode;
        SerializedProperty paddingColor;
        SerializedProperty renderVideoOutputCameraToTexture;

        SerializedProperty autoOutputSizeInPixels;
        SerializedProperty autoOutputSizeInMeters;
        SerializedProperty autoOutputOffset;

        SerializedProperty videoOutputSizeInPixels;
        SerializedProperty videoOutputSizeInMeters;
        SerializedProperty videoOutputOffset;

        void OnEnable() {

            augmentaManager = serializedObject.FindProperty("augmentaManager");
            augmentaVideoOutputCamera = serializedObject.FindProperty("camera");

            cameraMode = serializedObject.FindProperty("_cameraMode");
            paddingColor = serializedObject.FindProperty("paddingColor");
            renderVideoOutputCameraToTexture = serializedObject.FindProperty("renderVideoOutputCameraToTexture");

            autoOutputSizeInPixels = serializedObject.FindProperty("autoOutputSizeInPixels");
            autoOutputSizeInMeters = serializedObject.FindProperty("autoOutputSizeInMeters");
            autoOutputOffset = serializedObject.FindProperty("autoOutputOffset");

            videoOutputSizeInPixels = serializedObject.FindProperty("_videoOutputSizeInPixels");
            videoOutputSizeInMeters = serializedObject.FindProperty("_videoOutputSizeInMeters");
            videoOutputOffset = serializedObject.FindProperty("_videoOutputOffset");
        }

        public override void OnInspectorGUI() {

            AugmentaVideoOutput augmentaVideoOutput = target as AugmentaVideoOutput;

            serializedObject.Update();

            EditorGUILayout.LabelField("AUGMENTA COMPONENTS", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(augmentaManager, new GUIContent("Augmenta Manager"));
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(cameraMode, new GUIContent("Camera Mode", "None: no camera used with this video output.\nVideoOutput: the script will look for an AugmentaVideoOutputCamera in its hierarchy.\nExternal: any camera from the scene can be used. Note that the specified camera will be assigned a render texture target."));
            if (EditorGUI.EndChangeCheck() && Application.isPlaying) {
                serializedObject.ApplyModifiedProperties();
                augmentaVideoOutput.Initialize();
            }
            EditorGUILayout.Space();

            if (cameraMode.enumValueIndex == 1) {
                EditorGUILayout.PropertyField(renderVideoOutputCameraToTexture, new GUIContent("Render VideoOutput Camera To Texture"));
                EditorGUILayout.Space();
            } else if (cameraMode.enumValueIndex == 2) {
                EditorGUILayout.PropertyField(augmentaVideoOutputCamera, new GUIContent("Output Camera"));
                EditorGUILayout.PropertyField(paddingColor, new GUIContent("Texture Padding Color"));
                EditorGUILayout.Space();
            }

            EditorGUILayout.LabelField("VIDEO OUTPUT SETTINGS", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(autoOutputSizeInPixels, new GUIContent("Auto Size Output in Pixels", "Use data from Fusion to determine the output size in pixels."));

            if (!autoOutputSizeInPixels.boolValue) {
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(videoOutputSizeInPixels, new GUIContent("Output Size in Pixels"));
				if (EditorGUI.EndChangeCheck() && Application.isPlaying) {
                    serializedObject.ApplyModifiedProperties();
                    augmentaVideoOutput.RefreshVideoTexture();
				}

				EditorGUILayout.Space();
            }

            EditorGUILayout.PropertyField(autoOutputSizeInMeters, new GUIContent("Auto Size Output in Meters", "Use data from Fusion to determine the output size in meters."));

            if (!autoOutputSizeInMeters.boolValue) {
                EditorGUILayout.PropertyField(videoOutputSizeInMeters, new GUIContent("Output Size in Meters"));

                EditorGUILayout.Space();
            }

            EditorGUILayout.PropertyField(autoOutputOffset, new GUIContent("Auto Output Offset", "Use data from Fusion to determine the output offset."));

            if (!autoOutputOffset.boolValue) {
                EditorGUILayout.PropertyField(videoOutputOffset, new GUIContent("Output Offset"));
            }

            serializedObject.ApplyModifiedProperties();

        }
    }
}
