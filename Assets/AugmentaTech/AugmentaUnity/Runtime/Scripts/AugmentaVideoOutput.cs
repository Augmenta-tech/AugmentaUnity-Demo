using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;

namespace Augmenta
{
    /// <summary>
    /// Handle a video output to a texture according to the data from Fusion.
    /// </summary>
    public class AugmentaVideoOutput : MonoBehaviour
    {
        public AugmentaManager augmentaManager;

        public enum CameraMode { None, VideoOutput, External }
        public CameraMode cameraMode {
			get { return _cameraMode; }
            set { _cameraMode = value; Initialize(); }
		}

        public new Camera camera;

        public RenderTexture videoOutputTexture {
			get { return cameraMode == CameraMode.External ? _outputCustomRenderTexture : _outputRenderTexture; }
        }

        public Color paddingColor = Color.black;

        [Tooltip("Use data from Fusion to determine the output size in pixels.")]
        public bool autoOutputSizeInPixels = true;
        [Tooltip("Use data from Fusion to determine the output size in meters.")]
        public bool autoOutputSizeInMeters = true;
        [Tooltip("Use data from Fusion to determine the output offset.")]
        public bool autoOutputOffset = true;

        public Vector2Int videoOutputSizeInPixels {
			get { return autoOutputSizeInPixels ? augmentaManager.videoOutputSizeInPixels : _videoOutputSizeInPixels; }
            set { _videoOutputSizeInPixels = value; RefreshVideoTexture(); }
        }
        
        public Vector2 videoOutputSizeInMeters {
            get { return autoOutputSizeInMeters ? augmentaManager.videoOutputSizeInMeters : _videoOutputSizeInMeters; }
            set { _videoOutputSizeInMeters = value; }
		}

        public Vector2 videoOutputOffset {
			get { return autoOutputOffset ? augmentaManager.videoOutputOffset : _videoOutputOffset; }
            set { _videoOutputOffset = value; }
		}

        public Vector3 botLeftCorner = Vector3.zero;
        public Vector3 botRightCorner = Vector3.zero;
        public Vector3 topLeftCorner = Vector3.zero;
        public Vector3 topRightCorner = Vector3.zero;

        public delegate void VideoOutputTextureUpdated();
        public event VideoOutputTextureUpdated videoOutputTextureUpdated;

        public bool renderVideoOutputCameraToTexture = true;

        [SerializeField] private Vector2Int _videoOutputSizeInPixels = new Vector2Int();
        [SerializeField] private Vector2 _videoOutputSizeInMeters = new Vector2();
        [SerializeField] private Vector2 _videoOutputOffset = new Vector2();

        [SerializeField] private CameraMode _cameraMode;

        private RenderTexture _outputRenderTexture;
        private CustomRenderTexture _outputCustomRenderTexture;

        private Material outputCRTMaterial {
			get { if(!_outputCRTMaterial) _outputCRTMaterial = new Material(Shader.Find("Augmenta/CameraDisplayTexture"));
                return _outputCRTMaterial;
            }
		}
        private Material _outputCRTMaterial;
        private Matrix4x4 _cameraVPMatrix;
        private Vector4 _botLeftCameraUV;
        private Vector4 _botRightCameraUV;
        private Vector4 _topLeftCameraUV;
        private Vector4 _topRightCameraUV;

        private Vector3 _offset;

        private bool _initialized = false;

		#region MonoBehavious Functions

		private void OnEnable() {

            if (!_initialized)
                Initialize();
		}

		private void Update() {

            if (!_initialized)
                Initialize();

            UpdateVideoOutputCorners();

            if (cameraMode == CameraMode.External)
                UpdateCRTMaterial();
        }

		private void OnDisable() {

            if (_initialized)
                CleanUp();
		}

		private void OnDrawGizmos() {


            Gizmos.color = Color.magenta;

            Gizmos.DrawLine(botLeftCorner, botRightCorner);
            Gizmos.DrawLine(botRightCorner, topRightCorner);
            Gizmos.DrawLine(topRightCorner, topLeftCorner);
            Gizmos.DrawLine(topLeftCorner, botLeftCorner);
        }

		#endregion

		#region Setup 

		public void Initialize() {

            if (_initialized)
                CleanUp();

			if (!augmentaManager) {
                Debug.LogError("AugmentaManager is not specified in AugmentaVideoOutput " + name+".");
                return;
			}

			if (cameraMode == CameraMode.VideoOutput) {
                AugmentaVideoOutputCamera augmentaVideoOutputCamera = GetComponentInChildren<AugmentaVideoOutputCamera>();

				if (!augmentaVideoOutputCamera) {
                    Debug.LogError("Could not find an AugmentaVideoOutputCamera in " + name + " hierarchy.");
                    return;
				} else {
                    camera = augmentaVideoOutputCamera.camera;
				}
			} else if (cameraMode == CameraMode.External) {
				if (!camera) {
                    Debug.LogError("No camera specified in " + name + " which is set to use an external camera.");
                    return;
                }
			}

            //Initialize videoOutputTexture
            RefreshVideoTexture();

            augmentaManager.fusionUpdated += OnFusionUpdated;

            _initialized = true;
		}

        void CleanUp() {

            augmentaManager.fusionUpdated -= OnFusionUpdated;
        }

		#endregion

		#region Augmenta Events

		void OnFusionUpdated() {

            if (cameraMode == CameraMode.None)
                return;

            if (!videoOutputTexture)
                RefreshVideoTexture();

            //Check video texture size
            if (videoOutputSizeInPixels.x != videoOutputTexture.width || videoOutputSizeInPixels.y != videoOutputTexture.height) {
                RefreshVideoTexture();
            }

		}

        #endregion

        #region Video Texture

        public void RefreshVideoTexture() {

            if (cameraMode == CameraMode.None || !camera)
                return;

            if (videoOutputSizeInPixels.x == 0 || videoOutputSizeInPixels.y == 0)
                return;

            if (videoOutputTexture) {
                if (videoOutputSizeInPixels.x != videoOutputTexture.width || videoOutputSizeInPixels.y != videoOutputTexture.height) {
                    videoOutputTexture.Release();
                } else {
                    return;
                }
            }

			//Create texture
            CreateOutputRenderTexture();

            if (cameraMode == CameraMode.External)
                CreateOutputCustomRenderTexture();

            //Send texture updated event
            videoOutputTextureUpdated?.Invoke();

        }

        void CreateOutputRenderTexture() {

            _outputRenderTexture = new RenderTexture(videoOutputSizeInPixels.x, videoOutputSizeInPixels.y, 0, RenderTextureFormat.ARGB64, RenderTextureReadWrite.Default);
            _outputRenderTexture.Create();

            if (!(cameraMode == CameraMode.VideoOutput && !renderVideoOutputCameraToTexture)) {
                //Assign texture as render target of video output camera
                camera.targetTexture = _outputRenderTexture;
            }
        }

        void CreateOutputCustomRenderTexture() {

            _outputCustomRenderTexture = new CustomRenderTexture(videoOutputSizeInPixels.x, videoOutputSizeInPixels.y, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            _outputCustomRenderTexture.updateMode = CustomRenderTextureUpdateMode.Realtime;
            _outputCustomRenderTexture.Create();

            _outputCustomRenderTexture.material = outputCRTMaterial;

            _outputCRTMaterial.SetTexture("_MainTex", _outputRenderTexture);

        }

        void UpdateVideoOutputCorners() {

            if (!augmentaManager.augmentaScene)
                return;

            _offset = augmentaManager.augmentaScene.debugObject.transform.TransformDirection(new Vector3(videoOutputOffset.x, -videoOutputOffset.y, 0));
            topLeftCorner = augmentaManager.augmentaScene.debugObject.transform.TransformPoint(new Vector3(-0.5f, 0.5f, 0)) + _offset;
            botLeftCorner = topLeftCorner + augmentaManager.augmentaScene.debugObject.transform.TransformDirection(Vector3.down * videoOutputSizeInMeters.y * augmentaManager.scaling);
            botRightCorner = botLeftCorner + augmentaManager.augmentaScene.debugObject.transform.TransformDirection(Vector3.right * videoOutputSizeInMeters.x * augmentaManager.scaling);
            topRightCorner = botRightCorner + augmentaManager.augmentaScene.debugObject.transform.TransformDirection(Vector3.up * videoOutputSizeInMeters.y * augmentaManager.scaling);
        }

        #endregion

        #region External Camera

        void UpdateCRTMaterial() {

            UpdateCameraDisplayMatrix();
            UpdateCameraUV();

            outputCRTMaterial.SetColor("_PaddingColor", paddingColor);
            outputCRTMaterial.SetVector("_BotCamUV", new Vector4(_botLeftCameraUV.x, _botLeftCameraUV.y, _botRightCameraUV.x, _botRightCameraUV.y));
            outputCRTMaterial.SetVector("_TopCamUV", new Vector4(_topLeftCameraUV.x, _topLeftCameraUV.y, _topRightCameraUV.x, _topRightCameraUV.y));
        }

        void UpdateCameraDisplayMatrix() {

            Matrix4x4 V = camera.worldToCameraMatrix;
            Matrix4x4 P = camera.projectionMatrix;

            //bool d3d = SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D") > -1;

            //if (d3d) {
            //    // Invert Y for rendering to a render texture
            //    for (int j = 0; j < 4; j++) {
            //        P[1, j] = -P[1, j];
            //    }
            //    // Scale and bias from OpenGL -> D3D depth range
            //    for (int j = 0; j < 4; j++) {
            //        P[2, j] = P[2, j] * 0.5f + P[3, j] * 0.5f;
            //    }
            //}

            _cameraVPMatrix = P * V;
        }

        void UpdateCameraUV() {

            _botLeftCameraUV = _cameraVPMatrix * new Vector4(botLeftCorner.x, botLeftCorner.y, botLeftCorner.z, 1);
            _botLeftCameraUV = (_botLeftCameraUV / _botLeftCameraUV.w) * 0.5f + Vector4.one * 0.5f;

            _botRightCameraUV = _cameraVPMatrix * new Vector4(botRightCorner.x, botRightCorner.y, botRightCorner.z, 1);
            _botRightCameraUV = (_botRightCameraUV / _botRightCameraUV.w) * 0.5f + Vector4.one * 0.5f;

            _topLeftCameraUV = _cameraVPMatrix * new Vector4(topLeftCorner.x, topLeftCorner.y, topLeftCorner.z, 1);
            _topLeftCameraUV = (_topLeftCameraUV / _topLeftCameraUV.w) * 0.5f + Vector4.one * 0.5f;

            _topRightCameraUV = _cameraVPMatrix * new Vector4(topRightCorner.x, topRightCorner.y, topRightCorner.z, 1);
            _topRightCameraUV = (_topRightCameraUV / _topRightCameraUV.w) * 0.5f + Vector4.one * 0.5f;
        }

        #endregion
    }
}
