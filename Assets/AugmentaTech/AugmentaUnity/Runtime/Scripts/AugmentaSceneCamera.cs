using Augmenta;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A camera that can will always adapt its field of view to perfectly match the Augmenta scene.
/// It can be orthographic, perspective or offCenter.
/// If it is orthographic or perspective, it should be centered on the Augmenta scene for the field of view to match the scene perfectly.
/// </summary>

namespace Augmenta
{
    public class AugmentaSceneCamera : AugmentaCamera
    {
        public AugmentaManager augmentaManager;

        public float distanceToScene = 10.0f;

        private Vector3 _botLeftCorner;
        private Vector3 _botRightCorner;
        private Vector3 _topLeftCorner;
        private Vector3 _topRightCorner;

        private Vector3 _sceneUp;
        private Vector3 _sceneForward;

        // Update is called once per frame
        void Update() {
            //Don't update if there is no Augmenta scene
            if (!augmentaManager.augmentaScene)
                return;

            //Don't update if Augmenta scene size is 0
            if (augmentaManager.augmentaScene.width <= 0 || augmentaManager.augmentaScene.height <= 0)
                return;

            UpdateAugmentaSceneCorners();
            OrientCamera();

            switch (cameraType) {

                case CameraType.Orthographic:
                    CenterCamera();
                    ComputeOrthoCamera(augmentaManager.augmentaScene.width * augmentaManager.scaling.x, augmentaManager.augmentaScene.height * augmentaManager.scaling.y);
                    break;

                case CameraType.Perspective:
                    CenterCamera();
                    ComputePerspectiveCamera(augmentaManager.augmentaScene.width * augmentaManager.scaling.x, augmentaManager.augmentaScene.height * augmentaManager.scaling.y, distanceToScene);
                    break;

                case CameraType.OffCenter:
                    ComputeOffCenterCamera(_botLeftCorner, _botRightCorner, _topLeftCorner, _topRightCorner);
                    break;

            }
        }

        /// <summary>
        /// Update the positions of the Augmenta scene corners
        /// </summary>
        void UpdateAugmentaSceneCorners() {
            _botLeftCorner = augmentaManager.augmentaScene.debugObject.transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0));
            _botRightCorner = augmentaManager.augmentaScene.debugObject.transform.TransformPoint(new Vector3(0.5f, -0.5f, 0));
            _topLeftCorner = augmentaManager.augmentaScene.debugObject.transform.TransformPoint(new Vector3(-0.5f, 0.5f, 0));
            _topRightCorner = augmentaManager.augmentaScene.debugObject.transform.TransformPoint(new Vector3(0.5f, 0.5f, 0));
        }

        void CenterCamera() {

            camera.transform.position = augmentaManager.augmentaScene.debugObject.transform.TransformPoint(Vector3.zero) - _sceneForward * distanceToScene;
        }

        void OrientCamera() {

            _sceneUp = (_topLeftCorner - _botLeftCorner).normalized;
            _sceneForward = Vector3.Cross((_topRightCorner - _topLeftCorner).normalized, _sceneUp).normalized;

            camera.transform.rotation = Quaternion.LookRotation(_sceneForward, _sceneUp);
        }
    }
}
