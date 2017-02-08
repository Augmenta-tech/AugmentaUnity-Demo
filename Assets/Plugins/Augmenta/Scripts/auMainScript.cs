using UnityEngine;
using System.Collections;

public class auMainScript : MonoBehaviour {

	static public bool debug = true;
	static public bool hide = false;

	// Object references
	UnityOSCReceiver osc;
	auListener listener;
	SharedTextureServer graphicServer;
	AreaCalibration areaCalibration;

	// GUI
	Rect windowRect = new Rect (0, 0, 250, 150);	// Initial position of UI window
	private string[] load = {"    ", "-   ", "--  ", "--- ", " ---", "  --", "   -"}; // Characters used for loading effect
	int oscPortUI; // osc port set in UI (different than the one in osc while not validated) 
	private Vector2 scrollPosition;

	// Use this for initialization
	void Start () {
		osc = GameObject.Find ("OscReceiver").GetComponent<UnityOSCReceiver>();
		listener = GameObject.Find ("AugmentaReceiver").GetComponent<auListener> ();
		graphicServer = GameObject.Find ("MainCamera").GetComponent<SharedTextureServer> ();
		areaCalibration = GameObject.Find ("InteractiveArea").GetComponent<AreaCalibration> ();
		oscPortUI = osc.port;
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
		graphicServer.ShowEditorView(!hide);
		
		// Transfer scene width/height data from auListener to SyphonSpoutServer
		// we do this here to avoid adding augmenta dependency in the SyphonSpoutServer
		int w = listener.GetScene().width;
		int h = listener.GetScene().height;
		graphicServer.SetResolution (w, h);
		
	}

	void OnGUI(){
		if (hide) {
			DrawHiddenGUI ();
		} else {
			// Keep draggable window inside screen
			int margin = 20;
			// Window out of the left side of the screen
			if (windowRect.xMax < margin)
				windowRect.x = -windowRect.width + margin;
			// Window out of the right side of the screen
			if (windowRect.xMin > Screen.width - margin)
				windowRect.x = Screen.width - margin;
			// Window out of the top side of the screen
			if (windowRect.yMin < 0)
				windowRect.y = 0;
			// Window out of the bottom side of the screen
			if (windowRect.yMin > Screen.height - margin)
				windowRect.y = Screen.height - margin;
			
			// Use "windowRect =" to make window draggable
			// use 12000 for window ID to prevent potential conflicts if use 0
			windowRect = GUI.Window (12000, windowRect, DrawGUI, "Augmenta settings");
		}

		if (osc.mute) {
			GUI.Label (new Rect (5, Screen.height - 20, 100, 25), "/!\\ OSC mute");
		}
	}

	private void DrawHiddenGUI(){
		// Create a string with info about OSC connection
		string oscConnectedString;
		if (osc.isConnected()) {
			oscConnectedString = " (connected)";
		} else {
			oscConnectedString = " (WARNING : not connected !)";
		}

		GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), Texture2D.blackTexture, ScaleMode.StretchToFill, false);

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

	private void DrawGUI(int windowID){

		int marginY = 5;

		scrollPosition = GUILayout.BeginScrollView(scrollPosition);

		//--------------------------------------------------
		// OSC
		//--------------------------------------------------
		GUILayout.BeginHorizontal ();
		// Label
		GUILayout.Label("Osc port", GUILayout.ExpandWidth(false));
		// Input osc port
		GUI.SetNextControlName("inputOscPort");
		if (osc.isConnected() && !osc.mute) {
			GUI.color = Color.green;
		} else if(osc.isReconnecting() || osc.mute){
			GUI.color = Color.gray;
		} else {
			GUI.color = Color.red;
		}
		if (int.TryParse (GUILayout.TextField (oscPortUI.ToString (), 25), out oscPortUI)) {
			// Change port when losing focus or enter key pressed
			if (oscPortUI != osc.port && (GUI.GetNameOfFocusedControl() != "inputOscPort" || (Event.current.isKey && Event.current.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == "inputOscPort"))) {
				ChangeOSCPort (oscPortUI);
			}
		}
		GUI.color = Color.white;
		// Display loading effect
		if (osc.isReconnecting ()) {
			GUILayout.Label(load[Time.frameCount%load.Length], GUILayout.ExpandWidth(false));
		}
		// Mute toggle
		osc.mute = GUILayout.Toggle (osc.mute, "Mute", GUILayout.ExpandWidth(false));

		GUILayout.EndHorizontal ();

		GUILayout.Space (marginY);

		//--------------------------------------------------
		// Resolution
		//--------------------------------------------------
		GUILayout.BeginHorizontal();
		graphicServer.autoResolution = GUILayout.Toggle (graphicServer.autoResolution, "Auto resolution", GUILayout.ExpandWidth(false));
		if (graphicServer.autoResolution) {
			GUILayout.Label (graphicServer.renderWidth+" x "+graphicServer.renderHeight);
		} else {
			graphicServer.renderWidth = int.Parse (GUILayout.TextField (graphicServer.renderWidth.ToString (), 25));
			graphicServer.lockAspectRatio = GUILayout.Toggle (graphicServer.lockAspectRatio, "", GUILayout.ExpandWidth(false));
			graphicServer.renderHeight = int.Parse (GUILayout.TextField (graphicServer.renderHeight.ToString (), 25));
		}
		GUILayout.EndHorizontal ();
	
		GUILayout.Space (marginY);

		//--------------------------------------------------
		// Augmenta area size
		//--------------------------------------------------
		areaCalibration.areaAutoResize = GUILayout.Toggle (areaCalibration.areaAutoResize, "Auto Augmenta area");

		GUILayout.Space (marginY);

		//--------------------------------------------------
		// Smooth
		//--------------------------------------------------
		GUILayout.BeginHorizontal();
		GUILayout.Label ("Smooth", GUILayout.ExpandWidth(false));
		// Slider style
		GUIStyle slider = new GUIStyle (GUI.skin.horizontalSlider);
		slider.margin = new RectOffset(slider.margin.left, slider.margin.right, slider.margin.top + 5, slider.margin.bottom);
		GUIStyle thumb = new GUIStyle (GUI.skin.horizontalSliderThumb);
		thumb.padding = new RectOffset(thumb.padding.left, thumb.padding.right, 5, thumb.padding.bottom);
		listener.SmoothAmount = GUILayout.HorizontalSlider(listener.SmoothAmount, 0, 0.99f, slider, thumb);
		GUILayout.EndHorizontal ();

		GUILayout.Space (marginY);

		//--------------------------------------------------
		// Debug
		//--------------------------------------------------
		if (debug) {
			// Save default color
			Color defaultColor = GUI.contentColor;
			GUI.contentColor = Color.yellow;
			debug = GUILayout.Toggle (debug, "Debug activated");
			// Put back default color
			GUI.contentColor = defaultColor;

			// Define text content
			GUIContent content = new GUIContent ();
			content.text = 	"You can calibrate Augmenta area in manual mode : \n" +
							"W + arrows to move the area\n" +
							"X + arrows to scale the area\n" +
							"C + left/right to rotate the area\n";
			// Style that we will use in the box
			GUIStyle style = GUI.skin.box;
			style.wordWrap = true;
			// Allocate space for the box
			Rect boxRect = GUILayoutUtility.GetRect (content, style);
			// Draw box
			GUI.Box (boxRect, GUIContent.none);
			// Draw text in the box
			GUI.Label (new Rect(boxRect.x+5, boxRect.y+5, boxRect.width-10, boxRect.height-10), content.text);
		} else {
			debug = GUILayout.Toggle(debug, "Debug");
		}



		GUILayout.EndScrollView ();

		// Make the window draggable
		// Just the window's header allow to drag window
		GUI.DragWindow(new Rect(0, 0, 10000, 20));
	}

	private void ChangeOSCPort(int port){
		osc.port = oscPortUI;
		if (osc.port >= 1024 || osc.port <= 65535) {
			if (osc.reconnect() && areaCalibration.areaAutoResize) {
				// Adjust scene size if auto resize activated because size may have changed 
				areaCalibration.AdjustScene ();
			}
		} else {
			osc.disconnect();
		}
	}

}
