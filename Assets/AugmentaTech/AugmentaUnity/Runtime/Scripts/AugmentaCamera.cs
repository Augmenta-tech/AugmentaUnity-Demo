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
    [RequireComponent(typeof(Camera))]
    public class AugmentaCamera : MonoBehaviour
    {
        public enum CameraType { Orthographic, Perspective, OffCenter };
        public CameraType cameraType = CameraType.Perspective;

        public new Camera camera {
            get { if (!_camera) _camera = GetComponent<Camera>();
                return _camera;
            }
		}

        private Camera _camera;

		#region Camera Update Functions

		/// <summary>
		/// Compute the size and aspect for an orthographic camera
		/// </summary>
		protected void ComputeOrthoCamera(float width, float height) {

            camera.orthographic = true;
            camera.aspect = width / height;
            camera.orthographicSize = height * 0.5f;

            camera.ResetProjectionMatrix();
        }

        /// <summary>
        /// Compute the size and aspect for a perspective camera
        /// </summary>
        protected void ComputePerspectiveCamera(float width, float height, float distance) {

            camera.orthographic = false;
            camera.ResetProjectionMatrix();

            camera.fieldOfView = 2.0f * Mathf.Rad2Deg * Mathf.Atan2(height * 0.5f, distance);

            camera.aspect = width / height;

        }

        /// <summary>
        /// Compute the projection matrix for an off center camera
        /// </summary>
        protected void ComputeOffCenterCamera(Vector3 botLeftCorner, Vector3 botRightCorner, Vector3 topLeftCorner, Vector3 topRightCorner) {

            camera.orthographic = false;
            camera.ResetAspect();

            Vector3 pa, pb, pc, pd;
            pa = botLeftCorner; //Bottom-Left
            pb = botRightCorner; //Bottom-Right
            pc = topLeftCorner; //Top-Left
            pd = topRightCorner; //Top-Right

            Vector3 pe = camera.transform.position;// eye position

            Vector3 vr = (pb - pa).normalized; // right axis of screen
            Vector3 vu = (pc - pa).normalized; // up axis of screen
            Vector3 vn = Vector3.Cross(vr, vu).normalized; // normal vector of screen

            Vector3 va = pa - pe; // from pe to pa
            Vector3 vb = pb - pe; // from pe to pb
            Vector3 vc = pc - pe; // from pe to pc
            Vector3 vd = pd - pe; // from pe to pd

            float n = camera.nearClipPlane; // distance to the near clip plane (screen)
            float f = camera.farClipPlane; // distance of far clipping plane
            float d = Vector3.Dot(va, vn); // distance from eye to screen
            float l = Vector3.Dot(vr, va) * n / d; // distance to left screen edge from the 'center'
            float r = Vector3.Dot(vr, vb) * n / d; // distance to right screen edge from 'center'
            float b = Vector3.Dot(vu, va) * n / d; // distance to bottom screen edge from 'center'
            float t = Vector3.Dot(vu, vc) * n / d; // distance to top screen edge from 'center'

            Matrix4x4 p = new Matrix4x4(); // Projection matrix
            p[0, 0] = 2.0f * n / (r - l);
            p[0, 2] = (r + l) / (r - l);
            p[1, 1] = 2.0f * n / (t - b);
            p[1, 2] = (t + b) / (t - b);
            p[2, 2] = (f + n) / (n - f);
            p[2, 3] = 2.0f * f * n / (n - f);
            p[3, 2] = -1.0f;

            try {
                camera.projectionMatrix = p; // Assign matrix to camera
            } catch (Exception e) {
                Debug.LogWarning("Frustrum error, matrix invalid : " + e.Message);
            }
        }

        #endregion
    }
}
