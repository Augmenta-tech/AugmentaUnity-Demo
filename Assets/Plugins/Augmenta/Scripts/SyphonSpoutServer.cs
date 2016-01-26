using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Camera))]
public class SyphonSpoutServer : MonoBehaviour {

	/*
	 * This class creates the appropriate component for texture sharing according 
	 * to the target platform of the app (Syphon for OSX, Spout for Windows)
	 */ 


	private bool lockAspectRatio = false;
	private float aspectRatio = 16/9;

	private int renderWidth = 1920;
	private int renderHeight = 1080;

	private int oldRenderWidth;
	private int oldRenderHeight;

	void Start () {

		LoadSettings ();

		// Init Syphon or Spout server
		#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
		// Add syphon components
		gameObject.AddComponent<Syphon>();
		gameObject.AddComponent<SyphonServerTextureCustomResolution>();

		#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		// Create a new camera for Spout, based on this camera
		// because Spout can't send a render texture and show the scene in app window with the same camera
		// This instantiation is made in Start function, to prevent cloned camera 
		// to replicate itself through this script before it is destroyed.
		GameObject spoutCamera = Instantiate(gameObject);

		// Remove useless cloned components
		Destroy(spoutCamera.GetComponent<SyphonSpoutServer>());
		Destroy(spoutCamera.GetComponent<AudioListener>());
		Destroy(spoutCamera.GetComponent<GUILayer>());

		// Set name and tag 
		spoutCamera.name = "Camera Spout Sender";
		spoutCamera.tag = "SpoutCamera";

		// Make spout sender camera child of main camera
		spoutCamera.transform.parent = this.transform;

		// Add spout components 
		gameObject.AddComponent<Spout.Spout>();	// Instantiate Spout script on main camera to prevent crash when exiting app
		spoutCamera.AddComponent<Spout.SpoutSender>();
		spoutCamera.AddComponent<Spout.InvertCamera>();

		// Setup spout components
		spoutCamera.GetComponent<Spout.SpoutSender>().sharingName = Application.productName;
		// Create a new render texture
		RenderTexture targetTexture = new RenderTexture(renderWidth, renderHeight, 24);
		// Set it to the camera 
		spoutCamera.GetComponent<Camera>().targetTexture = targetTexture;
		// And set it to Spout
		spoutCamera.GetComponent<Spout.SpoutSender>().texture = targetTexture;

		#endif

		// Init values
		oldRenderWidth = renderWidth;
		oldRenderHeight = renderHeight;
		aspectRatio = (float)renderWidth/(float)renderHeight;

	}

	void OnGUI(){
		// Resolution
		lockAspectRatio = GUI.Toggle(new Rect(15,60,120,20), lockAspectRatio, "Lock aspect ratio");
		renderWidth = int.Parse(GUI.TextField(new Rect(140, 60, 40, 20), renderWidth.ToString(), 25));
		GUI.Label(new Rect(180,60-2,15,30), "x");
		renderHeight = int.Parse(GUI.TextField(new Rect(195, 60, 40, 20), renderHeight.ToString(), 25));
	}

	void Update(){
		UpdateRender ();
	}


		
	void UpdateRender () {
		
		// Update the fbo if the render size has changed
		if (renderWidth != oldRenderWidth || renderHeight != oldRenderHeight && renderWidth > 100 && renderHeight > 100) {

			// If aspect ratio locked
			if (lockAspectRatio) {
				// If user modified width, change height to match aspect ratio
				if (renderWidth != oldRenderWidth) {
					renderHeight = Mathf.RoundToInt ((float)renderWidth / aspectRatio);
				}
				// If user modified height, change width to match aspect ratio
				else if (renderHeight != oldRenderHeight) {
					renderWidth = Mathf.RoundToInt (renderHeight * aspectRatio);
				}
			}

			#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			// Apply changes to syphon server
			gameObject.GetComponent<SyphonServerTextureCustomResolution> ().renderWidth = renderWidth;
			gameObject.GetComponent<SyphonServerTextureCustomResolution> ().renderHeight = renderHeight;
			gameObject.GetComponent<SyphonServerTextureCustomResolution> ().createOrResizeRenderTexture ();

			#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			// Apply changes to syphon server
			GameObject spoutChild = null;
			foreach (Transform child in gameObject.transform) {
				if(child.CompareTag("SpoutCamera")){
					spoutChild = child.gameObject;
					break;
				}
			}
			Spout.SpoutSender spoutSender = spoutChild.GetComponent<Spout.SpoutSender>();
			Camera camera = spoutChild.GetComponent<Camera>();

			// Disable Spout while updating targettexture to prevent crash
			spoutSender.enabled = false;

			// Create a new render texture because we can't reallocate an already existing one
			// Store camera target texture in temp variable to be able to release it while not in use by the camera
			RenderTexture targetTexture = camera.targetTexture;
			camera.targetTexture = null;
			targetTexture.Release();
			targetTexture = new RenderTexture(renderWidth, renderHeight, 24);
			// Set it to the camera 
			camera.targetTexture = targetTexture;
			// Set it to spout sender
			spoutSender.texture = targetTexture;

			// Enable spout sender
			spoutSender.enabled = true;

			#endif

			// If aspect ratio not locked, compute it 
			if (!lockAspectRatio) {
				aspectRatio = (float)renderWidth / (float)renderHeight;
			}

			// Save current values as old values to be able to compare them to new values next time they are changed in UI
			oldRenderWidth = renderWidth;
			oldRenderHeight = renderHeight;
		}
			
	}

	void SaveSettings(){
		PlayerPrefs.SetInt ("renderWidth", renderWidth);
		PlayerPrefs.SetInt ("renderHeight", renderHeight);
	}

	void LoadSettings(){
		renderWidth = PlayerPrefs.GetInt ("renderWidth");
		renderHeight = PlayerPrefs.GetInt("renderHeight");
	}
	void OnApplicationQuit(){
		SaveSettings ();
	}

}
