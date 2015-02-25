using UnityEngine;
using System.Collections;

public class PersonObject : MonoBehaviour {

	// Store the pid for convenience
	private int pid;
	private Vector3 centroid;

	// Use this for initialization
	void Start () {
		name = "PersonObject";
	}
	
	// Update is called once per frame
	void Update () {
		// Hide/show the cube
		renderer.enabled = auInterface.debug;
	}
	void OnDrawGizmos(){
		Gizmos.color = Color.red;
		Gizmos.DrawLine(centroid, centroid+3*transform.up);
	}

	public int getId(){return pid;}
	public void setId(int id){pid = id;}

	public Vector3 getCentroid(){return centroid;}
	public void setCentroid(Vector3 _centroid){centroid = _centroid;}

	public int getOid(){return auListener.getPeopleArray()[pid].oid;}

	public int getAge(){return auListener.getPeopleArray()[pid].age;}
}
