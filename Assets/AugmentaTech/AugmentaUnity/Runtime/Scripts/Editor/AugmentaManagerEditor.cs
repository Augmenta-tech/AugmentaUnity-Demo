using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Augmenta
{
    [CustomEditor(typeof(AugmentaManager))]
    public class AugmentaManagerEditor : Editor
    {
        SerializedProperty augmentaId;
        SerializedProperty inputPort;
        SerializedProperty protocolVersion;
        SerializedProperty pixelSize;
        SerializedProperty scaling;
        SerializedProperty flipX;
        SerializedProperty flipY;
        SerializedProperty augmentaObjectTimeOut;
        SerializedProperty desiredAugmentaObjectType;
        SerializedProperty desiredAugmentaObjectCount;
        SerializedProperty velocitySmoothing;
        SerializedProperty positionOffsetFromVelocity;
        SerializedProperty positionFromBoundingBox;
        SerializedProperty augmentaScenePrefab;
        SerializedProperty augmentaObjectPrefab;
        SerializedProperty customObjectPrefab;
        SerializedProperty customObjectPositionType;
        SerializedProperty customObjectRotationType;
        SerializedProperty customObjectScalingType;
        SerializedProperty mute;
        SerializedProperty showSceneDebug;
        SerializedProperty showObjectDebug;

        void OnEnable() {

            augmentaId = serializedObject.FindProperty("augmentaId");
            inputPort = serializedObject.FindProperty("_inputPort");
            protocolVersion = serializedObject.FindProperty("protocolVersion");
            pixelSize = serializedObject.FindProperty("pixelSize");
            scaling = serializedObject.FindProperty("scaling");
            flipX = serializedObject.FindProperty("flipX");
            flipY = serializedObject.FindProperty("flipY");
            augmentaObjectTimeOut = serializedObject.FindProperty("augmentaObjectTimeOut");
            desiredAugmentaObjectType = serializedObject.FindProperty("desiredAugmentaObjectType");
            desiredAugmentaObjectCount = serializedObject.FindProperty("desiredAugmentaObjectCount");
            velocitySmoothing = serializedObject.FindProperty("velocitySmoothing");
            positionOffsetFromVelocity = serializedObject.FindProperty("positionOffsetFromVelocity");
            positionFromBoundingBox = serializedObject.FindProperty("positionFromBoundingBox");
            augmentaScenePrefab = serializedObject.FindProperty("augmentaScenePrefab");
            augmentaObjectPrefab = serializedObject.FindProperty("augmentaObjectPrefab");
            customObjectPrefab = serializedObject.FindProperty("customObjectPrefab");
            customObjectPositionType = serializedObject.FindProperty("customObjectPositionType");
            customObjectRotationType = serializedObject.FindProperty("customObjectRotationType");
            customObjectScalingType = serializedObject.FindProperty("customObjectScalingType");
            mute = serializedObject.FindProperty("mute");
            showSceneDebug = serializedObject.FindProperty("_showSceneDebug");
            showObjectDebug = serializedObject.FindProperty("_showObjectDebug");
        }

        public override void OnInspectorGUI() {

            AugmentaManager augmentaManager = target as AugmentaManager;

            serializedObject.Update();

            EditorGUILayout.LabelField("AUGMENTA ID", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(augmentaId, new GUIContent("Augmenta ID"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("OSC SETTINGS", EditorStyles.boldLabel);
            //Input port change handling
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(inputPort, new GUIContent("Input Port"));
            if (EditorGUI.EndChangeCheck() && Application.isPlaying) {
                serializedObject.ApplyModifiedProperties(); 
                augmentaManager.CreateAugmentaOSCReceiver(); 
            }

            if (Application.isPlaying) {
                if (!augmentaManager.portBinded) {
                    EditorGUILayout.HelpBox("Port not binded", MessageType.Error);
                } else {
                    if (!augmentaManager.receivingData) {
                        EditorGUILayout.HelpBox("Waiting for data", MessageType.Info);
                    } else {
                        EditorGUILayout.HelpBox("Receiving data", MessageType.Info);
                    }
                }
            }

            EditorGUILayout.PropertyField(protocolVersion, new GUIContent("Protocol Version"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("AUGMENTA SCENE SETTINGS", EditorStyles.boldLabel);
            if (protocolVersion.enumValueIndex == 0) {
                EditorGUILayout.PropertyField(pixelSize, new GUIContent("Pixel Size"));
            }
            EditorGUILayout.PropertyField(scaling, new GUIContent("Scaling"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("AUGMENTA OBJECTS SETTINGS", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(flipX, new GUIContent("Flip X"));
            EditorGUILayout.PropertyField(flipY, new GUIContent("Flip Y"));
            EditorGUILayout.PropertyField(augmentaObjectTimeOut, new GUIContent("Augmenta Object TimeOut"));
            EditorGUILayout.PropertyField(desiredAugmentaObjectType, new GUIContent("Desired Augmenta Object Type"));
            if(desiredAugmentaObjectType.enumValueIndex > 0) {
                EditorGUILayout.PropertyField(desiredAugmentaObjectCount, new GUIContent("Desired Augmenta Object Count"));
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(velocitySmoothing, new GUIContent("Velocity Smoothing"));
            EditorGUILayout.PropertyField(positionOffsetFromVelocity, new GUIContent("Position Offset From Velocity"));

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(positionFromBoundingBox, new GUIContent("Position From Bounding Box", "Place the object according to the bounding box position, the centroid is used otherwise. Usually this should only be true if you represent the bounding box visually."));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("AUGMENTA PREFABS", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(augmentaScenePrefab, new GUIContent("Augmenta Scene Prefab"));
            EditorGUILayout.PropertyField(augmentaObjectPrefab, new GUIContent("Augmenta Object Prefab"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("CUSTOM OBJECT", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(customObjectPrefab, new GUIContent("Custom Object Prefab"));
            if(customObjectPrefab.objectReferenceValue != null) {
                EditorGUILayout.PropertyField(customObjectPositionType, new GUIContent("Custom Object Position"));
                EditorGUILayout.PropertyField(customObjectRotationType, new GUIContent("Custom Object Rotation"));
                EditorGUILayout.PropertyField(customObjectScalingType, new GUIContent("Custom Object Scaling"));
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("DEBUG", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(mute, new GUIContent("Mute"));

            //Show debug change handling
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(showSceneDebug, new GUIContent("Show Scene Debug"));
            EditorGUILayout.PropertyField(showObjectDebug, new GUIContent("Show Object Debug"));
            if (EditorGUI.EndChangeCheck() && Application.isPlaying) {
                serializedObject.ApplyModifiedProperties();
                augmentaManager.ShowDebug(showSceneDebug.boolValue, showObjectDebug.boolValue);
            }

            serializedObject.ApplyModifiedProperties();

        }
    }
}
