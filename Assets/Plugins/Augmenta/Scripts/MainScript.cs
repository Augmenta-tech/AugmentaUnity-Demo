using UnityEngine;
using System.Collections;

public class MainScript : MonoBehaviour {

	static public bool debug = true;
	static public bool hide = false;

	// Object references
	UnityOSCReceiver osc;
	GameObject syphonCanvas;
	auListener listener;
	SyphonSpoutServer graphicServer;

	// Use this for initialization
	void Start () {
		StartCoroutine("GetSyphonCanvas"); // Get the reference like the others but we have to wait until it's instanciated
		osc = GameObject.Find ("OscReceiver").GetComponent<UnityOSCReceiver>();
		listener = GameObject.Find ("AugmentaReceiver").GetComponent<auListener> ();
		graphicServer = GameObject.Find ("MainCamera").GetComponent<SyphonSpoutServer> ();
	}
	
	// Update is called once per frame
	void Update () {
		// ---------------------
		// Controls
		// ---------------------
		if (Input.GetKeyDown ("s")) {
			// Force every save settings function in the program
			Debug.Log ("Save all settings manually");
			GameObject[] gos = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));
			foreach (GameObject go in gos) {
				if (go && go.transform.parent == null) {
					go.gameObject.BroadcastMessage("SaveSettings", SendMessageOptions.DontRequireReceiver);
				}
			}
			PlayerPrefs.Save ();
		}
		if (Input.GetKeyDown ("d")) {
			debug = !debug;
			Debug.Log ("Changed debug mode to " + debug);
		}
		if (Input.GetKeyDown ("h")) {
			hide = !hide;
			Debug.Log ("Changed hide mode to : " + hide);
		}
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit ();
		}
		// ---------------------


		// Disable drawing if hide
		if (hide) {
			if (syphonCanvas != null) {
				syphonCanvas.SetActive (false);
			}
		} else {
			if (syphonCanvas != null) {
				syphonCanvas.SetActive (true);
			}
		}

		// Transfer scene width/height data from auListener to SyphonSpoutServer
		// we do this here to avoid adding augmenta dependency in the SyphonSpoutServer
		int w = listener.GetScene().width;
		int h = listener.GetScene().height;
		graphicServer.SetResolution (w, h);

	}

	IEnumerator GetSyphonCanvas(){
		while (syphonCanvas == null) {
			yield return 0;
			syphonCanvas = GameObject.Find ("SyphonServerCustomRezUICanvas");
			if (syphonCanvas != null) {
				GameObject.Find ("SyphonServerCustomRezCam").GetComponent<Camera> ().backgroundColor = Color.black;
			}
		}
	}

	void OnGUI(){
		if (hide) {

			// Create a string with info about OSC connection
			string oscConnectedString;
			if (osc.isConnected()) {
				oscConnectedString = " (connected)";
			} else {
				oscConnectedString = " (WARNING : not connected !)";
			}

			GUI.Label (new Rect (20, 29, 500, 1000), 
			"[ Press 'H' to show / hide the program ]\n"+
			"\n"+
			"Listening to augmenta data on port " + osc.getPort () + oscConnectedString + "\n" +
			"\n"+
			"Instructions : \n" +
			"- 'S' to manually save the settings (auto save on quit)\n"+
			"- 'D' to toggle debug mode \n" +
			"\n" +
			"Augmenta calibration : \n"+
			"- Press the 'Auto Augmenta area' toogle if your app has the same resolution as Merge's output \n"+
			"- To calibrate in manual mode, uncheck the toggle and the use the following keys :\n"+
			"\t> 'W' + arrow keys to move the area\n"+
			"\t> 'X' + arrow keys to scale the area\n"+
			"\t> 'C' + left/right keys to rotate the area\n"
			);

		}
	}

}
