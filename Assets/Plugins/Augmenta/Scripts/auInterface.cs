using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Augmenta;

public class auInterface : MonoBehaviour {
	/*

	 This class serves as an interface allowing you to easily get all the GameObjects representing the people in your physical scene, mapped into your virtual scene.
	 It is linked to the "InteractiveArea" object on which the people's objects will be instanciated. Translate the area as you like to set where people need to be in the virtual scene.
	 
	 USING AUGMENTA BY GETTING ALL THE OBJECTS IN THE SCENE :
		- Get the dictionary of the Augmenta objects in the scene by doing :
			Dictionary<int,GameObject> myAugmentaObjects = auInterface.getAugmentaObjects();
		- Loop through it to read the informations :
			foreach(KeyValuePair<int, GameObject> pair in myAugmentaObjects) {
				Debug.Log("The point with id ["+pair.Key+"] is located at : x="+pair.Value.GetComponent<PersonObject>().getCentroid().x+" and y="+pair.Value.GetComponent<PersonObject>().getCentroid().z);
				Debug.Log ("He's "+pair.Value.GetComponent<PersonObject>().getAge()+" frames old");
			}

 	 USING AUGMENTA BY LISTENING TO THE MESSAGES :
		- Add the new functions : "ObjectEntered(GameObject o)", "ObjectUpdated(GameObject o)", and "ObjectWillLeave(int id)" to your code
		- Each function will be called by the auInterface when objects are added/moved/removed, which can be useful especially when you have to instanciate one object per person in the scene
		- As you may have noticed, when the object is about to be removed, you'll have to rely on its ID only.


	 USING AUGMENTA BY DOING YOUR CUSTOM CODE :
		- If you want to bypass the auInterface and the InteractiveArea, we're okay with that : you just have to know that the position informations will be given between 0 an 1
		- You can listen to the messages broadcasted by the auListener : PersonEntered(Person p) / PersonUpdated(Person p) / PersonWillLeave(Person p)
		- Or access the Persons array like this :
			 foreach(KeyValuePair<int, Person> pair in auListener.getPeopleArray()) {
			   	Debug.Log("The point with id ["+pair.Key+"] has raw values : x="+pair.Value.centroid.x+" and y="+pair.Value.centroid.y);
			 }

	*/

	private static Dictionary<int,GameObject> arrayPersonCubes = new Dictionary<int,GameObject>();

	public Material	[] materials;
	public GameObject boundingPlane; // Put the people on this plane
	public GameObject personMarker; // Used to represent people moving about in our example

	public static bool debug = true;
	private bool boundingBoxValid = true;
	
	void Start () {
		// Launched at scene startup
	}

	void Update () {
		// Called once per frame
	}
	
	public static Dictionary<int,GameObject> getAugmentaObjects(){
		return arrayPersonCubes;
	}
	
	public void PersonEntered(Person person){
		//Debug.Log("Person entered pid : " + person.pid);
		if(!arrayPersonCubes.ContainsKey(person.pid)){
			GameObject personObject = (GameObject)Instantiate(personMarker, Vector3.zero, Quaternion.identity);
			personObject.transform.parent = boundingPlane.transform.parent.transform;
			updatePerson(person, personObject);

			personObject.renderer.material = materials[person.pid % materials.Length];
			arrayPersonCubes.Add(person.pid,personObject);
			BroadcastMessage("ObjectEntered", personObject, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void PersonUpdated(Person person) {
		Debug.Log("Person updated pid : " + person.pid);
		if(arrayPersonCubes.ContainsKey(person.pid)){
			GameObject cubeToMove = arrayPersonCubes[person.pid];
			updatePerson(person, cubeToMove);
			BroadcastMessage("ObjectUpdated", cubeToMove, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void PersonWillLeave(Person person){
		//Debug.Log("Person leaving with ID " + person.pid);
		if(arrayPersonCubes.ContainsKey(person.pid)){
			//Debug.Log("Destroying cube");
			GameObject cubeToRemove = arrayPersonCubes[person.pid];
			// Send only the pid : the actual object will be destroyed
			BroadcastMessage("ObjectWillLeave", person.pid, SendMessageOptions.DontRequireReceiver);
			arrayPersonCubes.Remove(person.pid);
			//delete it from the scene	
			Destroy(cubeToRemove);
		}
	}

	public void SetDrawCubes(bool bEnable)
	{
		//bDrawCube = bEnable;
		
		ArrayList ids = new ArrayList();
		
		foreach(KeyValuePair<int, GameObject> cube in arrayPersonCubes) {
			ids.Add(cube.Key);
		}
		foreach(int id in ids) {
			if(arrayPersonCubes.ContainsKey(id)){
				GameObject pC = arrayPersonCubes[id];
				pC.renderer.enabled = bEnable;
			}
		}
		
	}
	
	public void clearAllPersons(){
		Debug.Log("Clear all cubes");
		foreach(var pKey in arrayPersonCubes.Keys){
			Destroy(arrayPersonCubes[pKey]);
		}
		arrayPersonCubes.Clear();
	}

	private void updatePerson(Person person, GameObject personObject){
		movePerson(person, personObject);
		PersonObject po = personObject.GetComponent<PersonObject>();
		po.setId(person.pid);
	}

	//maps the Augmenta coordinate system into one that matches the size of the boundingPlane
	private void movePerson(Person person, GameObject personObject){

		Transform pt = personObject.transform;
		Transform bt = boundingPlane.transform;
		Bounds meshBounds = boundingPlane.GetComponent<MeshFilter>().sharedMesh.bounds;

		// Reset
		pt.position = Vector3.zero;
		pt.rotation = Quaternion.identity;
		GameObject centroidObject = new GameObject();
		centroidObject.transform.position = Vector3.zero;
		centroidObject.transform.rotation = Quaternion.identity;

		// Test if the bounding box info is valid
		if (person.boundingRect.x >= 0 && person.boundingRect.y >=0 && person.boundingRect.width > 0 && person.boundingRect.height > 0){
			boundingBoxValid = true;
		} else if (boundingBoxValid) {
			boundingBoxValid = false;
			Debug.LogWarning("The bounding box informations are not valid, we'll be using the centroid and a default scale instead for the debug display");
		}

		if (boundingBoxValid){
			// Take care of the bounding box's scale
			Vector3 bsize = bt.GetComponent<MeshRenderer>().bounds.size;
			Vector3 psize = pt.GetComponent<MeshRenderer>().bounds.size;
			Vector3 scale = pt.localScale;
			scale.x = bsize.x * scale.x / psize.x * person.boundingRect.width;
			scale.z = bsize.z * scale.z / psize.z * person.boundingRect.height;
			pt.localScale = scale;
		}

		// Rotate
		// Warning : may cause small imprecisions in the points positioning...keep the area straight if you can
		pt.Rotate(bt.rotation.eulerAngles);
		centroidObject.transform.Rotate(bt.rotation.eulerAngles);

		if (boundingBoxValid){
			// Move inside the area depending on the bounding box's values and depending on the scale
			pt.Translate( (float)(person.boundingRect.x+person.boundingRect.width/2 - 0.5f) * meshBounds.size.x * bt.localScale.x, 0, (float)(person.boundingRect.y+person.boundingRect.height	/2 - .5) * meshBounds.size.z * -1 * bt.localScale.z);
		} else {
			pt.Translate( (float)(person.centroid.x - 0.5f) * meshBounds.size.x * bt.localScale.x, 0, (float)(person.centroid.y - .5) * meshBounds.size.z * -1 * bt.localScale.z);
		}
		// Compute the centroid inside the area
		centroidObject.transform.Translate( (float)(person.centroid.x - 0.5f) * meshBounds.size.x * bt.localScale.x, 0, (float)(person.centroid.y - .5) * meshBounds.size.z * -1 * bt.localScale.z);

		// Put the bottom of the cubes on the plane (not the center)
		pt.Translate( 0, pt.localScale.y/2, 0 );
		centroidObject.transform.Translate( 0, pt.localScale.y/2, 0 );

		// Move to reach the position of the area
		pt.Translate(bt.position, Space.World);
		centroidObject.transform.Translate(bt.position, Space.World);

		// Update the centroid's value
		personObject.GetComponent<PersonObject>().setCentroid(centroidObject.transform.position);

		Destroy(centroidObject);

	}	
}
