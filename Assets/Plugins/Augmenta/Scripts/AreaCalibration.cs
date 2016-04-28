using UnityEngine;
using System.Collections;

public class AreaCalibration : MonoBehaviour {

	GameObject globalCam;
	Vector2 InteractiveAreaSize;
	bool oldAreaAutoResize = true;
	public bool areaAutoResize = true;

	// Use this for initialization
	void Start () {
		LoadSettings ();
		oldAreaAutoResize = areaAutoResize;
		globalCam = GameObject.FindGameObjectWithTag ("MainCamera");
		adjustScene ();
		//StartCoroutine("KeepAdjustingScene",3);
	}
	
	// Update is called once per frame
	void Update () {

		GetComponent<Renderer>().enabled = auMainScript.debug;

		if (auMainScript.debug && !areaAutoResize) {
			if (Input.GetKeyDown ("r")) {
				Reset ();
			}

			if (Input.GetKey ("w")) {
				if (Input.GetKey (KeyCode.RightArrow)) {
					this.transform.position += new Vector3 (0.1f, 0f, 0f);
				}
				if (Input.GetKey (KeyCode.LeftArrow)) {
					this.transform.position += new Vector3 (-0.1f, 0f, 0f);
				}
				if (Input.GetKey (KeyCode.UpArrow)) {
					this.transform.position += new Vector3 (0f, 0f, 0.1f);
				}
				if (Input.GetKey (KeyCode.DownArrow)) {
					this.transform.position += new Vector3 (0f, 0f, -0.1f);
				}
			} else if (Input.GetKey ("x")) {
				if (Input.GetKey (KeyCode.RightArrow)) {
					this.transform.localScale += new Vector3 (0.01f, 0f, 0f);
				}
				if (Input.GetKey (KeyCode.LeftArrow)) {
					this.transform.localScale += new Vector3 (-0.01f, 0f, 0f);
				}
				if (Input.GetKey (KeyCode.UpArrow)) {
					this.transform.localScale += new Vector3 (0f, 0f, 0.01f);
				}
				if (Input.GetKey (KeyCode.DownArrow)) {
					this.transform.localScale += new Vector3 (0f, 0f, -0.01f);
				}
			} else if (Input.GetKey ("c")) {
				// TOFIX : the scale of the objects changes slightly when rotating the area
				if (Input.GetKey (KeyCode.RightArrow)) {
					this.transform.Rotate (new Vector3 (0f, 1f, 0f));
				}
				if (Input.GetKey (KeyCode.LeftArrow)) {
					this.transform.Rotate (new Vector3 (0f, -1f, 0f));
				}
			}
		}

		// Test change in the value of areaAutoResize
		if (areaAutoResize != oldAreaAutoResize) {
			adjustScene ();
		}
		oldAreaAutoResize = areaAutoResize;

	}

	IEnumerator KeepAdjustingScene(float delay) {
		while (true) {
			adjustScene ();
			yield return new WaitForSeconds (delay);
		}
	}

	public void adjustScene(){
		if (areaAutoResize) {
			//Debug.Log ("Adjust scene");

			float epsilon = 0.003f;
		
			this.transform.eulerAngles = new Vector3(0, 0, 0);
			this.transform.position = new Vector3(0, 0, 0);

			Vector2 currentAreaWorldSize = new Vector2(0f,0f);
			currentAreaWorldSize.x = this.GetComponent<Renderer>().bounds.size.x;
			currentAreaWorldSize.y = this.GetComponent<Renderer>().bounds.size.z;
			//Debug.Log ("Current area size : " + currentAreaWorldSize);

			Vector2 worldSize = new Vector2(0f,0f);
			worldSize.x = -2*(globalCam.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(0, Screen.currentResolution.width, 0)).x);
			worldSize.y = -2*(globalCam.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(Screen.currentResolution.height, 0, 0)).z);
			//Debug.Log ("World size : "+worldSize.ToString());

			while(currentAreaWorldSize.x < worldSize.x-epsilon) {
				this.transform.localScale += new Vector3(0.005f, 0f, 0f);
				currentAreaWorldSize.x = this.GetComponent<Renderer>().bounds.size.x;
			}
			while(currentAreaWorldSize.y < worldSize.y-epsilon) {
				this.transform.localScale += new Vector3(0f, 0f, 0.005f);
				currentAreaWorldSize.y = this.GetComponent<Renderer>().bounds.size.z;
			}
			while(currentAreaWorldSize.x > worldSize.x+epsilon) {
				this.transform.localScale -= new Vector3(0.005f, 0f, 0f);
				currentAreaWorldSize.x = this.GetComponent<Renderer>().bounds.size.x;
			}
			while(currentAreaWorldSize.y > worldSize.y+epsilon) {
				this.transform.localScale -= new Vector3(0f, 0f, 0.005f);
				currentAreaWorldSize.y = this.GetComponent<Renderer>().bounds.size.z;
			}

			//Debug.Log ("END adjust scene");
		}
	}

	void OnApplicationQuit() {
		SaveSettings ();
	}

	void SaveSettings(){
		Debug.Log ("Saving interactive area settings");

		PlayerPrefs.SetFloat ("auPositionX", this.transform.position.x);
		PlayerPrefs.SetFloat ("auPositionY", this.transform.position.z);
		PlayerPrefs.SetFloat ("auScaleX", this.transform.localScale.x);
		PlayerPrefs.SetFloat ("auScaleY", this.transform.localScale.z);
		PlayerPrefs.SetFloat ("auRotationW", this.transform.rotation.w);
		PlayerPrefs.SetFloat ("auRotationX", this.transform.rotation.x);
		PlayerPrefs.SetFloat ("auRotationY", this.transform.rotation.y);
		PlayerPrefs.SetFloat ("auRotationZ", this.transform.rotation.z);

		PlayerPrefs.SetInt ("areaAutoResize", areaAutoResize?1:0);
	}

	void LoadSettings(){
		Debug.Log ("Loading interactive area settings");

		this.transform.position = new Vector3(PlayerPrefs.GetFloat("auPositionX"), 0f, PlayerPrefs.GetFloat("auPositionY")); 
		this.transform.localScale = new Vector3(PlayerPrefs.GetFloat ("auScaleX"), 1f, PlayerPrefs.GetFloat("auScaleY"));
		this.transform.rotation = new Quaternion (PlayerPrefs.GetFloat ("auRotationX"), PlayerPrefs.GetFloat ("auRotationY"), PlayerPrefs.GetFloat ("auRotationZ"), PlayerPrefs.GetFloat ("auRotationW"));

		areaAutoResize = PlayerPrefs.GetInt ("areaAutoResize", areaAutoResize?1:0) == 1 ? true : false;
	}

	void Reset(){
		this.transform.position = new Vector3 (0, 0, 0);
		this.transform.rotation = Quaternion.identity;
		this.transform.localScale = new Vector3 (1, 1, 1);
	}

}
