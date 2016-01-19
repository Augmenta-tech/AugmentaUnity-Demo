using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Camera))]
public class SyphonSpoutServer : MonoBehaviour {

	/*
	 * This class creates the appropriate component for texture sharing according 
	 * to the target platform of the app (Syphon for OSX, Spout for Windows)
	 */ 

	// Use this for initialization
	void Awake () {

		#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
		// Add syphon components
		gameObject.AddComponent<Syphon>();
		gameObject.AddComponent<SyphonServerTexture>();

		#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		// Add spout components 
		gameObject.AddComponent<Spout.Spout>();
		gameObject.AddComponent<Spout.SpoutSender>();
		gameObject.AddComponent<Spout.InvertCamera>();
		gameObject.GetComponent<Spout.SpoutSender>().texture = gameObject.GetComponent<Camera>().targetTexture;

		#endif

	}
}
