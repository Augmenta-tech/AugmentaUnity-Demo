//using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hold the object values from the Augmenta protocol and update the object debug view.
/// </summary>
namespace Augmenta
{
	public class AugmentaObject : MonoBehaviour
	{
        [Header("Augmenta Object Settings")]
        public AugmentaManager augmentaManager;
        public bool showDebug = false;
        public GameObject debugObject;
        public GameObject debugVelocityPivot;
        public GameObject debugVelocity;
        public GameObject debugOrientationPivot;
        public GameObject debugOrientation;

        [Header("Augmenta Object Values")]
        public int id;
		public int oid;
		public int ageInFrames;
        public float ageInSeconds;
		public Vector2 centroid;
        public Vector2 velocity;
        public float orientation;
		public float depth;
		public Rect boundingRect;
        public float boundingRectRotation;
		public Vector3 highest;
        public float distanceToSensor;
        public float reflectivity;

        [Header("Unity Object Values")]
        public float inactiveTime;
        [Tooltip("Object center position on the Augmenta Scene plane, ignoring its height.")]
        public Vector3 worldPosition2D;
        [Tooltip("Object center position above the Augmenta Scene plane, taking its height into account.")]
        public Vector3 worldPosition3D;
        [Tooltip("Object size in meters.")]
        public Vector3 worldScale;
        [Tooltip("Object velocity on the Augmenta Scene plane in meters per second.")]
        public Vector3 worldVelocity2D;
        [Tooltip("Object velocity in meters per second.")]
        public Vector3 worldVelocity3D;

        [HideInInspector] public bool useCustomObject;

        private GameObject _customObject;
        private IAugmentaObjectBehaviour _customObjectBehaviour;

        private Material _augmentaObjectMaterialInstance;

        private Vector3 _previousWorldPosition2D;
        private Vector3 _previousWorldPosition3D;

        private bool _initialized = false;
        private bool _previousPositionIsValid = false;

        #region MonoBehaviour Functions

        private void OnEnable() {

            //Initialization
            if (!_initialized)
                Initialize();
        }

        void Update() {

            //Initialization
            if (!_initialized)
                Initialize();

            //Update debug state if incoherent
            if (showDebug != debugObject.activeSelf)
                ShowDebug(showDebug);
        }

        void OnDrawGizmos() {

            Gizmos.color = Color.red;
            DrawGizmoCube(debugObject.transform.position,
                          debugObject.transform.rotation, 
                          debugObject.transform.localScale);


            //Gizmos.DrawLine(worldPosition2D, worldPosition2D + worldVelocity2D);
        }

        void OnDisable() {

             CleanUp();
        }

        #endregion

		#region Object Handling Functions

		/// <summary>
		/// Initialize the augmenta object
		/// </summary>
		void Initialize() {

            if (!augmentaManager)
                return;

            //Get an instance of the debug material
            _augmentaObjectMaterialInstance = debugObject.GetComponent<Renderer>().material;

            //Apply a random color to the material
            Random.InitState(id);
            _augmentaObjectMaterialInstance.SetColor("_Color", Color.HSVToRGB(Random.value, 0.85f, 0.75f));

            _initialized = true;
        }

        /// <summary>
        /// Clean up the augmenta object before removing or disabling it
        /// </summary>
        void CleanUp()
        {
	        if (!_initialized)
		        return;

            Destroy(_augmentaObjectMaterialInstance);

            //Destroy custom object
            DestroyCustomObject();

			_previousPositionIsValid = false;
            _initialized = false;
		}

        /// <summary>
        /// Update the Augmenta object Unity parameters
        /// </summary>
        /// <param name="augmentaObject"></param>
        public void UpdateAugmentaObject() {

            //Initialization
            if (!_initialized)
                Initialize();

            //Update object values
            worldPosition2D = GetAugmentaObjectWorldPosition(false);
            worldPosition3D = GetAugmentaObjectWorldPosition(true);
            worldScale = GetAugmentaObjectWorldScale();

			if (_previousPositionIsValid) {
                worldVelocity2D = Vector3.Lerp(worldVelocity2D, (worldPosition2D - _previousWorldPosition2D) / Time.deltaTime, Time.deltaTime / Mathf.Max(augmentaManager.velocitySmoothing, 0.001f));
                worldVelocity3D = Vector3.Lerp(worldVelocity3D, (worldPosition3D - _previousWorldPosition3D) / Time.deltaTime, Time.deltaTime / Mathf.Max(augmentaManager.velocitySmoothing, 0.001f));
            }

            _previousWorldPosition2D = worldPosition2D;
            _previousWorldPosition3D = worldPosition3D;
            _previousPositionIsValid = true;

            worldPosition2D += worldVelocity2D * augmentaManager.positionOffsetFromVelocity;
            worldPosition3D += worldVelocity3D * augmentaManager.positionOffsetFromVelocity;

            //Update debug object size
            debugObject.transform.position = worldPosition3D;
            debugObject.transform.localRotation = Quaternion.Euler(0.0f, -boundingRectRotation, 0.0f);
            debugObject.transform.localScale = worldScale;

            //Update debug velocity
            debugVelocityPivot.transform.position = debugObject.transform.position;
            debugVelocity.transform.localPosition = new Vector3(0, highest.z * 0.5f, velocity.magnitude * 0.5f);
            debugVelocityPivot.transform.localRotation = Quaternion.Euler(0, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg + 90, 0);
            debugVelocity.transform.localScale = new Vector3(debugVelocity.transform.localScale.x, debugVelocity.transform.localScale.y, velocity.magnitude);
            debugVelocityPivot.transform.localScale = new Vector3(worldScale.x, 1, worldScale.z);

            //Update debug orientation
            debugOrientationPivot.transform.position = debugObject.transform.position;
            debugOrientation.transform.localPosition = new Vector3(0, highest.z * 0.5f, 0.25f);
            debugOrientationPivot.transform.localRotation = Quaternion.Euler(0, 90.0f-orientation, 0);
            debugOrientationPivot.transform.localScale = new Vector3(worldScale.x, 1, worldScale.z);

            //Update custom object
            if (useCustomObject) {

                //Instantiate it
                if (!_customObject) {
                    InstantiateCustomObject();
                }

                //Update custom object
                UpdateCustomObject();
            }
        }

        /// <summary>
        /// Return the Augmenta object world position from the Augmenta scene position, offsetted by half the object height or not.
        /// </summary>
        /// <returns></returns>
        Vector3 GetAugmentaObjectWorldPosition(bool offset) {

            Vector2 normalizedPosition = augmentaManager.positionFromBoundingBox ? boundingRect.position : centroid;

            return augmentaManager.augmentaScene.transform.TransformPoint((normalizedPosition.x - 0.5f) * augmentaManager.augmentaScene.width * augmentaManager.scaling.x,
                                                                          offset ? highest.z * 0.5f : 0,
                                                                          -(normalizedPosition.y - 0.5f) * augmentaManager.augmentaScene.height * augmentaManager.scaling.y);
        }

        /// <summary>
        /// Return the Augmenta object scale
        /// </summary>
        /// <returns></returns>
        Vector3 GetAugmentaObjectWorldScale() {

            return new Vector3(boundingRect.width * augmentaManager.augmentaScene.width,
                               highest.z,
                               boundingRect.height * augmentaManager.augmentaScene.height);
        }

		#endregion

		#region Custom Object Handling Functions

        /// <summary>
        /// Instantiate the custom object
        /// </summary>
        void InstantiateCustomObject() {

            _customObject = Instantiate(augmentaManager.customObjectPrefab, transform.parent);
			_customObject.name += (" " + id.ToString());

            //If it has a behaviour, launch Spawn
            _customObjectBehaviour = _customObject.GetComponentInChildren<IAugmentaObjectBehaviour>();
            if (_customObjectBehaviour != null)
                _customObjectBehaviour.Spawn();
        }

        /// <summary>
        /// Update the custom object
        /// </summary>
        void UpdateCustomObject() {

            //Update position
            switch (augmentaManager.customObjectPositionType) {
                case AugmentaManager.CustomObjectPositionType.World2D: _customObject.transform.position = worldPosition2D; break;
                case AugmentaManager.CustomObjectPositionType.World3D: _customObject.transform.position = worldPosition3D; break;
            }

            //Update rotation
            switch (augmentaManager.customObjectRotationType) {
                case AugmentaManager.CustomObjectRotationType.AugmentaRotation: _customObject.transform.localRotation = Quaternion.Euler(0.0f, -boundingRectRotation, 0.0f); break;
                case AugmentaManager.CustomObjectRotationType.AugmentaOrientation: _customObject.transform.localRotation = Quaternion.Euler(0.0f, orientation, 0.0f); break;
            }

            //Update scale
            switch (augmentaManager.customObjectScalingType) {
                case AugmentaManager.CustomObjectScalingType.AugmentaSize: _customObject.transform.localScale = worldScale; break;
            }
        }

		/// <summary>
		/// Update the custom object prefab
		/// </summary>
		public void ChangeCustomObject() {

            //Destroy the custom object
            //It will be instantiated at the next update with the updated prefab if necessary
            DestroyCustomObject();
        }

        /// <summary>
        /// Destroy the custom object instantiated
        /// </summary>
        void DestroyCustomObject() {

            if (!_customObject)
                return;

            //If it has a behaviour, call Destroy, otherwise destroy it directly
            if (_customObjectBehaviour != null) {
                _customObjectBehaviour.Destroy();
                _customObject = null;
            } else {
                Destroy(_customObject);
                _customObject = null;
            }
        }

        #endregion

        #region Gizmos Functions

        public void DrawGizmoCube(Vector3 position, Quaternion rotation, Vector3 scale) {

            Matrix4x4 cubeTransform = Matrix4x4.TRS(position, rotation, scale);
            Matrix4x4 oldGizmosMatrix = Gizmos.matrix;

            Gizmos.matrix *= cubeTransform;

            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

            Gizmos.matrix = oldGizmosMatrix;
        }

        #endregion

        #region Debug Functions

        /// <summary>
        /// Activate/desactivate debug object
        /// </summary>
        /// <param name="show"></param>
        public void ShowDebug(bool show) {

            debugObject.SetActive(show);
            debugOrientationPivot.SetActive(show);
            debugVelocityPivot.SetActive(show);
        }

        #endregion
    }
}