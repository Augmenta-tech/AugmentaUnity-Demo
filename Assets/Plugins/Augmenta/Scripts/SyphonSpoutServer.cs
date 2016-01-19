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

	public int renderWidth = 1920;
	public int renderHeight = 1080;

	private int oldRenderWidth;
	private int oldRenderHeight;

	// Use this for initialization
	void Awake () {

		#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
		// Add syphon components
		gameObject.AddComponent<Syphon>();
		gameObject.AddComponent<SyphonServerTextureCustomResolution>();

		#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		// Add spout components 
		gameObject.AddComponent<Spout.Spout>();
		gameObject.AddComponent<Spout.SpoutSender>();
		gameObject.AddComponent<Spout.InvertCamera>();
		gameObject.GetComponent<Spout.SpoutSender>().texture = gameObject.GetComponent<Camera>().targetTexture;

		#endif

	}

	void Start () {

		// Init values
		oldRenderWidth = renderWidth;
		oldRenderHeight = renderHeight;
		aspectRatio = (float)renderWidth/(float)renderHeight;

	}
		
	void OnGUI () {
		// Resolution
		lockAspectRatio = GUI.Toggle(new Rect(15,60,120,20), lockAspectRatio, "Lock aspect ratio");

		renderWidth = int.Parse(GUI.TextField(new Rect(140, 60, 40, 20), renderWidth.ToString(), 25));
		GUI.Label(new Rect(180,60-2,15,30), "x");
		renderHeight = int.Parse(GUI.TextField(new Rect(195, 60, 40, 20), renderHeight.ToString(), 25));

		// Affect values in case user did change the resolution
		if (GUI.changed) {

			// If aspect ratio locked
			if(lockAspectRatio){
				// If user modified width, change height to match aspect ratio
				if(renderWidth != oldRenderWidth){
					renderHeight = Mathf.RoundToInt((float)renderWidth / aspectRatio);
				}
				// If user modified height, change width to match aspect ratio
				else if(renderHeight != oldRenderHeight){
					renderWidth = Mathf.RoundToInt(renderHeight * aspectRatio);
				}
			}

			#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			// Apply changes to syphon server
			gameObject.GetComponent<SyphonServerTextureCustomResolution>().renderWidth = renderWidth;
			gameObject.GetComponent<SyphonServerTextureCustomResolution>().renderHeight = renderHeight;
			gameObject.GetComponent<SyphonServerTextureCustomResolution>().createOrResizeRenderTexture();

			#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			// Apply changes to syphon server
			// TODO

			#endif

			// If aspect ratio not locked, compute it 
			if(!lockAspectRatio){
				aspectRatio = (float)renderWidth/(float)renderHeight;
			}

			// Save current values as old values to be able to compare them to new values next time they are changed in UI
			oldRenderWidth = renderWidth;
			oldRenderHeight = renderHeight;

		}
			
	}

}
