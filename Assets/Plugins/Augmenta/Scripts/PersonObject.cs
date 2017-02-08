using UnityEngine;
using System.Collections;

public class PersonObject : MonoBehaviour {

	// Store the pid for convenience
	private int pid;
	private Vector3 centroid;

	// Use this for initialization
	void Start () {
		name = "PersonObject";
		if (GetComponent<Renderer>() != null) {
			GetComponent<Renderer>().enabled = false; // Hide by default
		}
	}
	
	// Update is called once per frame
	void Update () {
		// Hide/show the cube
		GetComponent<Renderer>().enabled = auMainScript.debug;
	}
	void OnDrawGizmos(){
		Gizmos.color = Color.red;
		Gizmos.DrawLine(centroid, centroid+3*transform.up);
	}

	public int GetId(){return pid;}
	public void SetId(int id){pid = id;}

	public Vector3 GetCentroid(){return centroid;}
	public void SetCentroid(Vector3 _centroid){centroid = _centroid;}

	public int GetOid(){return auListener.GetPeopleArray()[pid].oid;}

	public int GetAge(){return auListener.GetPeopleArray()[pid].age;}
}
