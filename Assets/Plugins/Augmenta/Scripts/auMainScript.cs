using UnityEngine;
using System.Collections;

public class auMainScript : MonoBehaviour {

	static public bool debug = true;
	static public bool hide = false;

	// Object references
	UnityOSCReceiver osc;
	GameObject syphonCanvas;
	auListener listener;
	SyphonSpoutServer graphicServer;
	AreaCalibration areaCalibration;

	// GUI
	Rect windowRect = new Rect (0, 0, 250, 150);	// Initial position of UI window

	// Use this for initialization
	void Start () {
		StartCoroutine("GetSyphonCanvas"); // Get the reference like the others but we have to wait until it's instanciated
		osc = GameObject.Find ("OscReceiver").GetComponent<UnityOSCReceiver>();
		listener = GameObject.Find ("AugmentaReceiver").GetComponent<auListener> ();
		graphicServer = GameObject.Find ("MainCamera").GetComponent<SyphonSpoutServer> ();
		areaCalibration = GameObject.Find ("InteractiveArea").GetComponent<AreaCalibration> ();
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
			drawHiddenGUI ();
		} else {
			// Use "windowRect =" to make window draggable
			windowRect = GUI.Window (0, windowRect, drawGUI, "Augmenta settings");
		}
	}

	private void drawHiddenGUI(){
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

	private void drawGUI(int windowID){

		Vector2 anchor = new Vector2 (10, 25);
		int offsetX = 10;
		int offsetY = 30;

		//--------------------------------------------------
		// OSC
		//--------------------------------------------------
		Rect oscLabelRect = new Rect(anchor.x, anchor.y, 50, 25);
		Rect oscPortRect = new Rect(oscLabelRect.xMax + offsetX, anchor.y, 45, 20);
		Rect muteRect = new Rect(oscPortRect.xMax + offsetY, anchor.y, 50, 20);

		GUI.Label(oscLabelRect, "Osc port");
		if (osc.isConnected()) {
			GUI.color = Color.green;
		} else {
			GUI.color = Color.red;
		}
		if (int.TryParse (GUI.TextField (oscPortRect, osc.port.ToString (), 25), out osc.port)) {
			if (GUI.changed) {
				if (osc.port >= 1024 || osc.port <= 65535) {
					if (osc.reconnect()) {
						Invoke ("callClearAllPersons", 0.1f);
					}
				} else {
					osc.disconnect();
				}
			}
		}
		GUI.color = Color.white;
		osc.mute = GUI.Toggle (muteRect, osc.mute, "Mute");


		//--------------------------------------------------
		// Resolution
		//--------------------------------------------------
		Rect autoResolutionRect = new Rect(anchor.x, anchor.y + offsetY, 110, 20);
		Rect labelAutoRect = new Rect(autoResolutionRect.xMax + offsetX, anchor.y + offsetY, 100, 20);
		Rect labelWidthRect = new Rect(autoResolutionRect.xMax + offsetX, anchor.y + offsetY, 40, 20);
		Rect lockRatioRect = new Rect(labelWidthRect.xMax + 5, anchor.y + offsetY, 20, 20);
		Rect labelHeightRect = new Rect(lockRatioRect.xMax, anchor.y + offsetY, 40, 20);

		graphicServer.autoResolution = GUI.Toggle (autoResolutionRect, graphicServer.autoResolution, "Auto resolution");
		if (graphicServer.autoResolution) {
			GUI.Label (labelAutoRect, graphicServer.renderWidth+" x "+graphicServer.renderHeight);
		} else {
			graphicServer.renderWidth = int.Parse (GUI.TextField (labelWidthRect, graphicServer.renderWidth.ToString (), 25));
			graphicServer.lockAspectRatio = GUI.Toggle (lockRatioRect, graphicServer.lockAspectRatio, "");
			graphicServer.renderHeight = int.Parse (GUI.TextField (labelHeightRect, graphicServer.renderHeight.ToString (), 25));
		}
	
		//--------------------------------------------------
		// Augmenta area size
		//--------------------------------------------------
		Rect areaResizeRect = new Rect (anchor.x, anchor.y + 2 * offsetY, 150, 20);

		areaCalibration.areaAutoResize = GUI.Toggle (areaResizeRect, areaCalibration.areaAutoResize, "Auto Augmenta area");

		//--------------------------------------------------
		// Smooth
		//--------------------------------------------------
		Rect smoothSliderRect = new Rect(anchor.x, anchor.y + 3*offsetY + 5, 120, 20);
		Rect smoothLabelRect = new Rect(smoothSliderRect.xMax + offsetX, anchor.y + 3*offsetY, 60, 20); 

		GUI.Label (smoothLabelRect, "Smooth");
		listener.SmoothAmount = GUI.HorizontalSlider(smoothSliderRect, listener.SmoothAmount, 0, 0.99f);


		// Make the window draggable
		// Just the window's header allow to drag window
		GUI.DragWindow(new Rect(0, 0, 10000, 20));
	}

}
