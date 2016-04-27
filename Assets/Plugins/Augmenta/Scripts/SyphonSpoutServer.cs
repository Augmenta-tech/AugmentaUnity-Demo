using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Camera))]
public class SyphonSpoutServer : MonoBehaviour {

	/*
	 * This class creates the appropriate component for texture sharing according 
	 * to the target platform of the app (Syphon for OSX, Spout for Windows)
	 */ 

	public bool autoResolution = true;
	public bool lockAspectRatio = false;
	private float aspectRatio = 16/9;

	public int renderWidth = 1920;
	public int renderHeight = 1080;
	private int minRenderSize = 300;
	private bool forceRenderSizeUpdate = true;

	private int oldRenderWidth;
	private int oldRenderHeight;

	#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
	private enum GraphicServer{
		SYPHON,
		FUNNEL
	};

	private GraphicServer graphicServer = GraphicServer.SYPHON;
	#endif

	void Start () {

		LoadSettings ();
		Debug.Log ("Render size after load settings : " + renderWidth + "x" + renderHeight);

		// Init Syphon or Spout server
		SetupGraphicServer ();

		// Init values
		oldRenderWidth = renderWidth;
		oldRenderHeight = renderHeight;
		aspectRatio = (float)renderWidth/(float)renderHeight;
	}

	public void SetResolution(int x, int y){
		// Allow a change from other scripts only if the resolution is in AUTO mode
		if (autoResolution && x >= minRenderSize && y >= minRenderSize) {
			renderWidth = x;
			renderHeight = y;
		}
	}

	void Update(){
		// Update the fbo if the render size has changed
		if (forceRenderSizeUpdate || (renderWidth != oldRenderWidth || renderHeight != oldRenderHeight && renderWidth > 100 && renderHeight > 100)) {
			forceRenderSizeUpdate = false;
			UpdateRender ();
			GameObject.Find("InteractiveArea").BroadcastMessage ("adjustScene", SendMessageOptions.DontRequireReceiver);
		}

		// Safety check to make sure that the syphon texture is reset correctly when changing window size
		if(gameObject.GetComponent<Camera>().targetTexture == null){
			ResizeSyphonRenderTexture ();
		}
	}


		
	void UpdateRender () {
		
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

		ResizeRenderTexture ();

		// If aspect ratio not locked, compute it 
		if (!lockAspectRatio) {
			aspectRatio = (float)renderWidth / (float)renderHeight;
		}

		// Save current values as old values to be able to compare them to new values next time they are changed in UI
		oldRenderWidth = renderWidth;
		oldRenderHeight = renderHeight;

		UpdateWindowSize ();
		
	}

	void UpdateWindowSize(){
		int sw = Screen.currentResolution.width;
		int sh = Screen.currentResolution.height;
		if (renderWidth < minRenderSize || renderHeight < minRenderSize) {
			// Do nothing
		} else if (renderWidth < 0.9f * sw && renderHeight < 0.9 * sh) {
			Screen.SetResolution (renderWidth, renderHeight, false);
		} else {
			float renderRatio = (float)renderWidth / (float)renderHeight;
			float screenRatio = sw / sh;
			if (renderRatio > screenRatio) {
				int newWidth = (int)(0.9f * sw);
				Screen.SetResolution (newWidth, (int)(newWidth / renderRatio), false);
			} else {
				int newHeight = (int)(0.9f * sh);
				//Debug.LogError ("sh :" + sh + " / newheight : " + newHeight + " / calc width : " + (int)(newHeight * renderRatio));
				Screen.SetResolution ((int)((float)newHeight * (float)renderRatio), newHeight, false);
			}
		}

	}

	void SetupGraphicServer(){
		#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
		
		// Check current Unity version
		// < 5.3
		if(float.Parse( Application.version.Substring(0,3))<5.3){ //test the current Unity version. if it's oldest than 5.3 use syphon, else use funnel
			// Use Syphon (because have some issues on version >5.2 when writing this code)
			graphicServer = GraphicServer.SYPHON;
		}
		// >= 5.3
		else{
			// Use funnel, compatible with 5.3+ and latest OpenGL backend
			graphicServer = GraphicServer.FUNNEL;
		}
		
		switch(graphicServer){
		case GraphicServer.SYPHON:
			SetupSyphonServer();
			break;
			
		case GraphicServer.FUNNEL:
			SetupFunnelServer();
			break;
			
		default:
			break;
		}
		
		#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

		SetupSpoutServer();
		#endif
	}

	void ResizeRenderTexture(){
		#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
		// Apply changes to syphon server
		switch(graphicServer){
		case GraphicServer.SYPHON:
			ResizeSyphonRenderTexture();
			break;
			
		case GraphicServer.FUNNEL:
			ResizeFunnelRenderTexture();
			break;
			
		default:
			break;
		}
		#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		// Apply changes to spout server
		ResizeSpoutRenderTexture();
		#endif
	}

	#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
	void SetupSyphonServer(){
		// Add syphon components
		gameObject.AddComponent<Syphon>();
		gameObject.AddComponent<SyphonServerTextureCustomResolution>();
	}

	void SetupFunnelServer(){
		// Add funnel components
		gameObject.AddComponent<Funnel.Funnel>();
		gameObject.GetComponent<Funnel.Funnel>().renderMode = Funnel.Funnel.RenderMode.PreviewOnGameView;
	}

	void ResizeSyphonRenderTexture(){
		gameObject.GetComponent<SyphonServerTextureCustomResolution> ().renderWidth = renderWidth;
		gameObject.GetComponent<SyphonServerTextureCustomResolution> ().renderHeight = renderHeight;
		gameObject.GetComponent<SyphonServerTextureCustomResolution> ().createOrResizeRenderTexture ();
	}
	
	void ResizeFunnelRenderTexture(){
		gameObject.GetComponent<Funnel.Funnel> ().screenWidth = renderWidth;
		gameObject.GetComponent<Funnel.Funnel> ().screenHeight = renderHeight;
	}

	#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
	void SetupSpoutServer(){
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
	}

	void ResizeSpoutRenderTexture(){
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
	}
	#endif
	
	void SaveSettings(){
		PlayerPrefs.SetInt ("renderWidth", renderWidth);
		PlayerPrefs.SetInt ("renderHeight", renderHeight);
		PlayerPrefs.SetInt ("autoResolution", autoResolution?1:0);
	}

	void LoadSettings(){
		renderWidth = PlayerPrefs.GetInt ("renderWidth");
		renderHeight = PlayerPrefs.GetInt("renderHeight");
		autoResolution = PlayerPrefs.GetInt ("autoResolution") == 1 ? true : false;
	}

	void OnApplicationQuit(){
		SaveSettings ();
	}

}
