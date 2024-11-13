using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Augmenta.UnityOSC;

/// <summary>
/// https://github.com/Theoriz/Augmenta/wiki#data
/// </summary>

namespace Augmenta {

	public enum DesiredAugmentaObjectType
	{
		All,
		Oldest,
		Newest
	};

	public enum AugmentaEventType
	{
		AugmentaObjectEnter,
		AugmentaObjectUpdate,
		AugmentaObjectLeave,
		SceneUpdated,
		FusionUpdated
	};

	public enum AugmentaDataType
	{
		Main,
		Extra,
		Shape	//Not implemented in Augmenta protocol yet.
	}

	public enum AugmentaProtocolVersion
	{
		V1,
		V2
	}

	public class AugmentaManager : MonoBehaviour
	{
		#region Public Members

		//Augmenta ID
		public string augmentaId;

		//OSC Settings
		[SerializeField] private int _inputPort = 12000;
		public int inputPort {
			get { return _inputPort; }
			set {
				_inputPort = value;
				CreateAugmentaOSCReceiver();
			}
		}

		//Connectivity status
		public bool portBinded = false;
		public bool receivingData = false;

		public AugmentaProtocolVersion protocolVersion = AugmentaProtocolVersion.V2;

		//Augmenta Scene Settings
		public float pixelSize = 0.005f;
		public Vector2 scaling = Vector2.one;
		public Vector3 offset {
			get => transform.localPosition;
			set => transform.localPosition = value;
		}

        public Vector3 rotation
        {
            get => transform.localRotation.eulerAngles;
            set => transform.localRotation = Quaternion.Euler(value);
        }

        //Augmenta Objects Settings
        public bool flipX;
		public bool flipY;
		// Number of seconds before an augmenta object who hasn't been updated is removed
		public float augmentaObjectTimeOut = 1.0f; // seconds
		public DesiredAugmentaObjectType desiredAugmentaObjectType = DesiredAugmentaObjectType.All;
		public int desiredAugmentaObjectCount = 1;

		public float velocitySmoothing = 0.5f;
		public float positionOffsetFromVelocity = 0;

		public bool positionFromBoundingBox = false;

		//Augmenta Prefabs
		public GameObject augmentaScenePrefab;
		public GameObject augmentaObjectPrefab;

		//Custom Objects
		[Tooltip("Custom prefab to instantiate on Augmenta objects.")]
		public GameObject customObjectPrefab;

		public enum CustomObjectPositionType { World2D, World3D }
		[Tooltip("World2D = Projected on Augmenta scene plane. World3D = Above Augmenta scene plane according to height.")]
		public CustomObjectPositionType customObjectPositionType;

		public enum CustomObjectRotationType { None, AugmentaRotation, AugmentaOrientation }
		[Tooltip("AugmentaRotation = Follow the Augmenta object rotation. AugmentaOrientation = Follow the Augmenta object orientation.")]
		public CustomObjectRotationType customObjectRotationType;

		public enum CustomObjectScalingType { None, AugmentaSize }
		[Tooltip("AugmentaSize = Follow the Augmenta object size.")]
		public CustomObjectScalingType customObjectScalingType;

		//Fusion
		public Vector2Int videoOutputSizeInPixels = new Vector2Int();
		public Vector2 videoOutputSizeInMeters = new Vector2();
		public Vector2 videoOutputOffset = new Vector2();

		//Debug
		public bool mute = false;
		public bool showSceneDebug {
			get { return _showSceneDebug; }
			set { _showSceneDebug = value; ShowDebug(_showSceneDebug, _showObjectDebug); }
		}
		public bool showObjectDebug {
			get { return _showObjectDebug; }
			set { _showObjectDebug = value; ShowDebug(_showSceneDebug, _showObjectDebug); }
		}

		//Events
		public delegate void AugmentaObjectEnter(AugmentaObject augmentaObject, AugmentaDataType augmentaDataType);
		public event AugmentaObjectEnter augmentaObjectEnter;

		public delegate void AugmentaObjectUpdate(AugmentaObject augmentaObject, AugmentaDataType augmentaDataType);
		public event AugmentaObjectUpdate augmentaObjectUpdate;

		public delegate void AugmentaObjectLeave(AugmentaObject augmentaObject, AugmentaDataType augmentaDataType);
		public event AugmentaObjectLeave augmentaObjectLeave;

		public delegate void SceneUpdated();
		public event SceneUpdated sceneUpdated;

		public delegate void FusionUpdated();
		public event FusionUpdated fusionUpdated;

		public Dictionary<int, AugmentaObject> augmentaObjects;
		public AugmentaScene augmentaScene;

		#endregion

		#region Private Members

		private List<int> _expiredIds = new List<int>(); //Used to remove timed out objects

		private float _receivingDataTimer = 0;
		private float _autoConnectTimer = 0;
		private const float _autoConnectWaitDuration = 5;

		[SerializeField] private bool _showSceneDebug = true;
		[SerializeField] private bool _showObjectDebug = true;

		private bool _initialized = false;

		#endregion

		#region MonoBehaviour Functions

		private void Awake() {

			if (!_initialized)
				Initialize();
		}

		private void Update() {

			if (!_initialized)
				Initialize();

			//Check if objects are alive
			CheckAlive();

			//Autoconnect
			UpdateAutoConnect();

			//Update receivingData status
			UpdateReceivingData();
		}

		private void OnDisable() {

			if (_initialized) {
				//Remove OSC Receiver
				RemoveAugmentaOSCReceiver();
			}
		}

		#endregion

		/// <summary>
		/// Initialize Augmenta
		/// </summary>
		void Initialize() {

			//Initialize scene
			InitializeAugmentaScene();

			//Initialize objects array
			augmentaObjects = new Dictionary<int, AugmentaObject>();

			//Check that OSCMaster exists, if not create one
			if (FindFirstObjectByType<UnityOSC.OSCMaster>() == null) {
				GameObject oscMasterObject = new GameObject("OSCMaster");
				oscMasterObject.AddComponent<UnityOSC.OSCMaster>();
			}

			//Create OSC Receiver
			CreateAugmentaOSCReceiver();

			_initialized = true;
		}

		#region Augmenta Functions

		/// <summary>
		/// Initialize the augmenta scene object
		/// </summary>
		void InitializeAugmentaScene() {

			GameObject sceneObject = Instantiate(augmentaScenePrefab, transform);
			sceneObject.name = "Augmenta Scene " + augmentaId;
			SetLayerRecursively(sceneObject, gameObject.layer);

			augmentaScene = sceneObject.GetComponent<AugmentaScene>();

			augmentaScene.augmentaManager = this;
			augmentaScene.showDebug = showSceneDebug;
			augmentaScene.ShowDebug(showSceneDebug);

			augmentaScene.UpdateScene();
		}

		/// <summary>
		/// Send the Augmenta event of corresponding type, according to the desired object.
		/// </summary>
		/// <param name="eventType"></param>
		/// <param name="augmentaObject"></param>
		public void SendAugmentaEvent(AugmentaEventType eventType, AugmentaObject augmentaObject = null, AugmentaDataType augmentaDataType = AugmentaDataType.Main) {

			switch (eventType) {
				case AugmentaEventType.AugmentaObjectEnter:
					augmentaObjectEnter?.Invoke(augmentaObject, augmentaDataType);
					break;

				case AugmentaEventType.AugmentaObjectUpdate:
					augmentaObjectUpdate?.Invoke(augmentaObject, augmentaDataType);
					break;

				case AugmentaEventType.AugmentaObjectLeave:
					augmentaObjectLeave?.Invoke(augmentaObject, augmentaDataType);
					break;

				case AugmentaEventType.SceneUpdated:
					sceneUpdated?.Invoke();
					break;

				case AugmentaEventType.FusionUpdated:
					fusionUpdated?.Invoke();
					break;
			}
		}

		/// <summary>
		/// Add new Augmenta object.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		private AugmentaObject AddAugmentaObject(ArrayList args, AugmentaDataType objectDataType = AugmentaDataType.Main) {

			GameObject newAugmentaObjectObject = Instantiate(augmentaObjectPrefab, augmentaScene.gameObject.transform);

			AugmentaObject newAugmentaObject = newAugmentaObjectObject.GetComponent<AugmentaObject>();
			newAugmentaObject.augmentaManager = this;
			newAugmentaObject.useCustomObject = customObjectPrefab;
			newAugmentaObject.showDebug = showObjectDebug;
			newAugmentaObject.ShowDebug(showObjectDebug);

			UpdateAugmentaObject(newAugmentaObject, args, objectDataType);

			augmentaObjects.Add(newAugmentaObject.id, newAugmentaObject);

			newAugmentaObjectObject.name = "Augmenta Object " + newAugmentaObject.id;
			SetLayerRecursively(newAugmentaObjectObject, gameObject.layer);

			return newAugmentaObject;
		}

		/// <summary>
		/// Update an Augmenta object data from incoming data
		/// </summary>
		/// <param name="augmentaObject"></param>
		/// <param name="args"></param>
		/// <param name="augmentaDataType"></param>
		private void UpdateAugmentaObject(AugmentaObject augmentaObject, ArrayList args, AugmentaDataType augmentaDataType = AugmentaDataType.Main) {

			switch (augmentaDataType) {
				case AugmentaDataType.Main:
					UpdateAugmentaObjectMain(augmentaObject, args);
					break;
				case AugmentaDataType.Extra:
					UpdateAugmentaObjectExtra(augmentaObject, args);
					break;
			}

		}

		/// <summary>
		/// Update an Augmenta object main data from incoming data.
		/// </summary>
		/// <param name="augmentaObject"></param>
		/// <param name="args"></param>
		private void UpdateAugmentaObjectMain(AugmentaObject augmentaObject, ArrayList args) {

			Vector2 centroid = Vector2.zero;
			Vector2 velocity = Vector2.zero;
			Vector3 highest = Vector3.zero;
			Rect boundingRect = new Rect();
			float orientation = 0;
			float rotation = 0;

			switch (protocolVersion) {

				case AugmentaProtocolVersion.V1:

					augmentaObject.id = (int)args[0];
					augmentaObject.oid = (int)args[1];
					augmentaObject.ageInFrames = (int)args[2];
					centroid = new Vector2((float)args[3], (float)args[4]);
					velocity = new Vector2((float)args[5], (float)args[6]);
					augmentaObject.depth = (float)args[7];
					boundingRect = new Rect((float)args[8], (float)args[9], (float)args[10], (float)args[11]);
					highest = new Vector3((float)args[12], (float)args[13], (float)args[14]);
					break;

				case AugmentaProtocolVersion.V2:

					augmentaObject.id = (int)args[1];
					augmentaObject.oid = (int)args[2];
					augmentaObject.ageInSeconds = (float)args[3];
					centroid = new Vector2((float)args[4], (float)args[5]);
					velocity = new Vector2((float)args[6], (float)args[7]);
					orientation = (float)args[8];
					boundingRect = new Rect((float)args[9], (float)args[10], (float)args[11], (float)args[12]);
					rotation = (float)args[13];
					highest = new Vector3(augmentaObject.highest.x, augmentaObject.highest.y, (float)args[14]);
					break;
			}

			if (flipX) {
				centroid.x = 1 - centroid.x;
				velocity.x = -velocity.x;
				orientation = orientation > 180 ? 360.0f - orientation : 180.0f - orientation;
				boundingRect.x = 1 - boundingRect.x;
				rotation = rotation > 180 ? 360.0f - rotation : 180.0f - rotation;
				highest.x = 1 - highest.x;
			}

			if (flipY) {
				centroid.y = 1 - centroid.y;
				velocity.y = -velocity.y;
				orientation = 360.0f - orientation;
				boundingRect.y = 1 - boundingRect.y;
				rotation = 360.0f - rotation;
				highest.y = 1 - highest.y;
			}

			//NaN checks
			if (float.IsNaN(orientation) || float.IsInfinity(orientation))
			{
				orientation = 0;
			}

			augmentaObject.centroid = centroid;
			augmentaObject.velocity = velocity;
			augmentaObject.orientation = orientation;
			augmentaObject.boundingRect = boundingRect;
			augmentaObject.boundingRectRotation = rotation;
			augmentaObject.highest = highest;

			//Inactive time reset to zero : the object has just been updated
			augmentaObject.inactiveTime = 0;

			augmentaObject.UpdateAugmentaObject();
		}

		/// <summary>
		/// Update an Augmenta object extra data from incoming data.
		/// </summary>
		/// <param name="augmentaObject"></param>
		/// <param name="args"></param>
		private void UpdateAugmentaObjectExtra(AugmentaObject augmentaObject, ArrayList args) {

			Vector3 highest = new Vector3((float)args[3], (float)args[4], augmentaObject.highest.z);

			if (flipX) {
				highest.x = 1 - highest.x;
			}

			if (flipY) {
				highest.y = 1 - highest.y;
			}

			augmentaObject.id = (int)args[1];
			augmentaObject.oid = (int)args[2];
			augmentaObject.highest = highest;
			augmentaObject.distanceToSensor = (float)args[5];
			augmentaObject.reflectivity = (float)args[6];

			//Inactive time reset to zero : the object has just been updated
			augmentaObject.inactiveTime = 0;

			augmentaObject.UpdateAugmentaObject();
		}

		/// <summary>
		/// Remove an object with its id
		/// </summary>
		/// <param name="id"></param>
		public void RemoveAugmentaObject(int id) {

			Destroy(augmentaObjects[id].gameObject);
			augmentaObjects.Remove(id);
		}

		/// <summary>
		/// Remove all augmenta objects
		/// </summary>
		public void RemoveAllAugmentaObjects() {

			while (augmentaObjects.Count > 0) {
				RemoveAugmentaObject(augmentaObjects.ElementAt(0).Key);
			}
		}

		/// <summary>
		/// Return true if the object is desired (i.e. should be added/updated).
		/// </summary>
		/// <param name="oid"></param>
		/// <returns></returns>
		public bool IsAugmentaObjectDesired(int oid) {

			if (desiredAugmentaObjectType == DesiredAugmentaObjectType.Oldest) {
				return oid < desiredAugmentaObjectCount;
			} else if (desiredAugmentaObjectType == DesiredAugmentaObjectType.Newest) {
				return oid >= (augmentaScene.augmentaObjectCount - desiredAugmentaObjectCount);
			} else {
				return true;
			}
		}

		/// <summary>
		/// Check if augmenta objects are alive
		/// </summary>
		void CheckAlive() {

			_expiredIds.Clear();

            foreach (int key in augmentaObjects.Keys)
            {
                if (!mute && augmentaObjects[key].inactiveTime < augmentaObjectTimeOut)
                {
                    // We add a frame to the inactiveTime count
                    augmentaObjects[key].inactiveTime += Time.deltaTime;
                }
                else
                {
                    // The object hasn't been updated for a certain number of frames : mark for removal
                    _expiredIds.Add(key);
                }
            }

            //Remove expired objects
            foreach (int id in _expiredIds) {
				SendAugmentaEvent(AugmentaEventType.AugmentaObjectLeave, augmentaObjects[id]);
				RemoveAugmentaObject(id);
			}
		}

		/// <summary>
		/// Update data received timer
		/// </summary>
		void UpdateReceivingData() {

			_receivingDataTimer += Time.deltaTime;

			receivingData = _receivingDataTimer > 2 ? false : true;
		}

		/// <summary>
		/// Update autoConnect timer and status
		/// </summary>
		void UpdateAutoConnect() {

			if (portBinded) {
				_autoConnectTimer = 0;
			} else {
				_autoConnectTimer += Time.deltaTime;
				if (_autoConnectTimer > _autoConnectWaitDuration) {
					_autoConnectTimer = 0;
					CreateAugmentaOSCReceiver();
				}
			}
		}

		#endregion

		#region OSC Functions 

		/// <summary>
		/// Create an OSC receiver for Augmenta at the inputPort. Return true if success, false otherwise.
		/// </summary>
		/// <returns>
		/// </returns>
		public void CreateAugmentaOSCReceiver() {

			RemoveAugmentaOSCReceiver();

			if (UnityOSC.OSCMaster.CreateReceiver("Augmenta-" + augmentaId, inputPort) != null) {
				UnityOSC.OSCMaster.Receivers["Augmenta-" + augmentaId].messageReceived += OSCMessageReceived;
				portBinded = true;
			} else {
				Debug.LogError("Could not create OSC receiver at port " + inputPort + ".");
				portBinded = false;
			}
		}

		/// <summary>
		/// Remove the Augmenta OSC receiver.
		/// </summary>
		public void RemoveAugmentaOSCReceiver() {

			if (UnityOSC.OSCMaster.Receivers.ContainsKey("Augmenta-" + augmentaId)) {
				UnityOSC.OSCMaster.Receivers["Augmenta-" + augmentaId].messageReceived -= OSCMessageReceived;
				UnityOSC.OSCMaster.RemoveReceiver("Augmenta-" + augmentaId);
				portBinded = false;
			}
		}

		/// <summary>
		/// Parse incoming Augmenta messages.
		/// </summary>
		/// <param name="message"></param>
		public void OSCMessageReceived(OSCMessage message) {

			if (mute) return;

			_receivingDataTimer = 0;

			switch (protocolVersion) {
				case AugmentaProtocolVersion.V1:
					ParseAugmentaProtocolV1(message);
					break;

				case AugmentaProtocolVersion.V2:
					ParseAugmentaProtocolV2(message);
					break;
			}
		}

		/// <summary>
		/// Parse the OSC message using Augmenta protocol V1
		/// </summary>
		/// /// <param name="message"></param>
		private void ParseAugmentaProtocolV1(OSCMessage message) {

			string address = message.Address;
			ArrayList args = new ArrayList(message.Data);

			int id, oid;
			AugmentaObject augmentaObject = null;

			switch (address) {

				case "/au/personEntered/":
				case "/au/personEntered":

					id = (int)args[0];
					oid = (int)args[1];

					if (IsAugmentaObjectDesired(oid)) {
						if (!augmentaObjects.ContainsKey(id)) {
							//New object
							augmentaObject = AddAugmentaObject(args);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectEnter, augmentaObject);
						} else {
							//Object was already there
							augmentaObject = augmentaObjects[id];
							UpdateAugmentaObject(augmentaObject, args);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectUpdate, augmentaObject);
						}
					}

					break;

				case "/au/personUpdated/":
				case "/au/personUpdated":

					id = (int)args[0];
					oid = (int)args[1];

					if (IsAugmentaObjectDesired(oid)) {
						if (!augmentaObjects.ContainsKey(id)) {
							augmentaObject = AddAugmentaObject(args);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectEnter, augmentaObject);
						} else {
							augmentaObject = augmentaObjects[id];
							UpdateAugmentaObject(augmentaObject, args);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectUpdate, augmentaObject);
						}
					}

					break;

				case "/au/personWillLeave/":
				case "/au/personWillLeave":

					id = (int)args[0];
					oid = (int)args[1];

					if (IsAugmentaObjectDesired(oid)) {
						if (augmentaObjects.ContainsKey(id)) {
							augmentaObject = augmentaObjects[id];
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectLeave, augmentaObject);
							RemoveAugmentaObject(id);
						}
					}

					break;

				case "/au/scene/":
				case "/au/scene":

					augmentaScene.augmentaObjectCount = (int)args[2];
					augmentaScene.width = (int)args[5] * pixelSize;
					augmentaScene.height = (int)args[6] * pixelSize;

					SendAugmentaEvent(AugmentaEventType.SceneUpdated);

					break;
			}
		}

		/// <summary>
		/// Parse the OSC message using Augmenta protocol V2
		/// </summary>
		/// <param name="message"></param>
		private void ParseAugmentaProtocolV2(OSCMessage message) {

			string address = message.Address;
			ArrayList args = new ArrayList(message.Data);

			int id, oid;
			AugmentaObject augmentaObject = null;

			switch (address) {

				case "/object/enter/":
				case "/object/enter":

					id = (int)args[1];
					oid = (int)args[2];

					if (IsAugmentaObjectDesired(oid)) {
						if (!augmentaObjects.ContainsKey(id)) {
							//New object
							augmentaObject = AddAugmentaObject(args);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectEnter, augmentaObject);
						} else {
							//Object was already there
							augmentaObject = augmentaObjects[id];
							UpdateAugmentaObject(augmentaObject, args);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectUpdate, augmentaObject);
						}
					}

					break;

				case "/object/update/":
				case "/object/update":

					id = (int)args[1];
					oid = (int)args[2];

					if (IsAugmentaObjectDesired(oid)) {
						if (!augmentaObjects.ContainsKey(id)) {
							augmentaObject = AddAugmentaObject(args);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectEnter, augmentaObject);
						} else {
							augmentaObject = augmentaObjects[id];
							UpdateAugmentaObject(augmentaObject, args);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectUpdate, augmentaObject);
						}
					}

					break;

				case "/object/leave/":
				case "/object/leave":

					id = (int)args[1];
					oid = (int)args[2];

					if (IsAugmentaObjectDesired(oid)) {
						if (augmentaObjects.ContainsKey(id)) {
							augmentaObject = augmentaObjects[id];
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectLeave, augmentaObject);
							RemoveAugmentaObject(id);
						}
					}

					break;

				case "/object/enter/extra/":
				case "/object/enter/extra":

					id = (int)args[1];
					oid = (int)args[2];

					if (IsAugmentaObjectDesired(oid)) {
						if (!augmentaObjects.ContainsKey(id)) {
							//New object
							augmentaObject = AddAugmentaObject(args, AugmentaDataType.Extra);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectEnter, augmentaObject, AugmentaDataType.Extra);
						} else {
							//Object was already there
							augmentaObject = augmentaObjects[id];
							UpdateAugmentaObject(augmentaObject, args, AugmentaDataType.Extra);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectUpdate, augmentaObject, AugmentaDataType.Extra);
						}
					}

					break;

				case "/object/update/extra/":
				case "/object/update/extra":

					id = (int)args[1];
					oid = (int)args[2];

					if (IsAugmentaObjectDesired(oid)) {
						if (!augmentaObjects.ContainsKey(id)) {
							augmentaObject = AddAugmentaObject(args, AugmentaDataType.Extra);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectEnter, augmentaObject, AugmentaDataType.Extra);
						} else {
							augmentaObject = augmentaObjects[id];
							UpdateAugmentaObject(augmentaObject, args, AugmentaDataType.Extra);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectUpdate, augmentaObject, AugmentaDataType.Extra);
						}
					}

					break;

				case "/object/leave/extra/":
				case "/object/leave/extra":

					id = (int)args[1];
					oid = (int)args[2];

					if (IsAugmentaObjectDesired(oid)) {
						if (augmentaObjects.ContainsKey(id)) {
							augmentaObject = augmentaObjects[id];
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectLeave, augmentaObject, AugmentaDataType.Extra);
							RemoveAugmentaObject(id);
						}
					}

					break;

				case "/scene/":
				case "/scene":

					augmentaScene.augmentaObjectCount = (int)args[1];
					augmentaScene.width = (float)args[2];
					augmentaScene.height = (float)args[3];

					SendAugmentaEvent(AugmentaEventType.SceneUpdated);

					break;

				case "/fusion/":
				case "/fusion":

					videoOutputOffset = new Vector2((float)args[0], (float)args[1]);
					videoOutputSizeInMeters = new Vector2((float)args[2], (float)args[3]);
					videoOutputSizeInPixels = new Vector2Int((int)args[4], (int)args[5]);

					SendAugmentaEvent(AugmentaEventType.FusionUpdated);

					break;

			}
		}

		#endregion

		#region Utility Functions

		/// <summary>
		/// Clamp angle between 0 and 360 degrees
		/// </summary>
		/// <param name="angle"></param>
		/// <returns></returns>
		private float ClampAngle(float angle) {

			while (angle < 0)
				angle += 360.0f;

			return angle % 360.0f;
		}

		/// <summary>
		/// Change the layer of the gameobject and all its child
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="layer"></param>
		void SetLayerRecursively(GameObject obj, int layer) {
			obj.layer = layer;

			foreach (Transform child in obj.transform) {
				SetLayerRecursively(child.gameObject, layer);
			}
		}

		#endregion

		#region External Function

		public void ChangeCustomObjectPrefab(GameObject newCustomObjectPrefab) {

			//Don't change prefab or initialize if called from the editor as it will create garbage
			if (!Application.isPlaying)
				return;

			if (!_initialized)
				Initialize();

			customObjectPrefab = newCustomObjectPrefab;

			foreach (var augmentaObject in augmentaObjects) {

				augmentaObject.Value.ChangeCustomObject();
				augmentaObject.Value.useCustomObject = customObjectPrefab;
			}
		}

		#endregion

		#region Debug Functions

		/// <summary>
		/// Show debug of scene and persons
		/// </summary>
		/// <param name="show"></param>
		public void ShowDebug(bool showScene, bool showObject) {

			if (!_initialized)
				Initialize();

			if (augmentaScene)
				augmentaScene.showDebug = showScene;

			foreach(var augmentaObject in augmentaObjects) {
				augmentaObject.Value.showDebug = showObject;
			}
		}

		#endregion
	}
}