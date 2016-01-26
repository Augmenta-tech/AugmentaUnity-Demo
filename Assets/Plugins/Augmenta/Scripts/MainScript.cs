using UnityEngine;
using System.Collections;

public class MainScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown ("s")) {
			// Force every save settings function in the program
			Debug.Log ("Save all settings manually");
			GameObject[] gos = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));
			foreach (GameObject go in gos) {
				if (go && go.transform.parent == null) {
					go.gameObject.BroadcastMessage("SaveSettings", SendMessageOptions.DontRequireReceiver);
				}
			}
		}

	}
}
