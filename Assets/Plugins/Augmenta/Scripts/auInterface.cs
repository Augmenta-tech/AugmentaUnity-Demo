using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Augmenta;
using System;

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

	// Broadcast augmenta message DELEGATE
	public delegate void personEnteredHandler(int id, GameObject obj);
	public static event personEnteredHandler personEnteredMessage;
	public delegate void personUpdatedHandler(int id, GameObject obj);
	public static event personUpdatedHandler personUpdatedMessage;
	public delegate void personWillLeaveHandler(int id);
	public static event personWillLeaveHandler personWillLeaveMessage;

	private static Dictionary<int,GameObject> arrayPersonCubes = new Dictionary<int,GameObject>();

	public Material	[] materials;
	public static GameObject boundingPlane; // Put the people on this plane
	public GameObject personMarker; // Used to represent people moving about in our example

	/// Ways to get Augmenta objects
	/// - ALL : all persons are returned
	/// - OLDEST : only the oldest person in augmenta scene is returned
	/// - NEWEST : only the newest person in augmenta scene is returned
	public enum Mode{
		ALL,
		OLDEST, 
		NEWEST
	}

	/// Currently selected mode
	public static Mode mode = Mode.ALL;


	void Start () {
		// Launched at scene startup
		auListener.broadcastMessage += EventReceiver;

		boundingPlane = this.transform.Find ("InteractiveArea").gameObject;
	}
		
	public void OnEnable(){
		auListener.broadcastMessage += EventReceiver;
	}

	public void OnDisable(){
		auListener.broadcastMessage -= EventReceiver;
	}

	void Update () {
		// Called once per frame

	}

	/// Return Augmenta objects considering currently selected mode to get them
	public static Dictionary<int,GameObject> GetAugmentaObjects(){
		switch (mode) {
		case Mode.ALL:
			return auInterface.GetAll ();

		case Mode.NEWEST:
			Dictionary<int,GameObject> newDict = new Dictionary<int,GameObject> ();
			if(auInterface.GetNewest() != null)
				newDict.Add (auInterface.GetNewest ().GetComponent<PersonObject> ().GetId (), auInterface.GetNewest ());
			return newDict;

		case Mode.OLDEST:
			Dictionary<int,GameObject> oldDict = new Dictionary<int,GameObject> ();
			if (auInterface.GetOldest() != null)
				oldDict.Add (auInterface.GetOldest ().GetComponent<PersonObject> ().GetId (), auInterface.GetOldest ());
			return oldDict;

		default:
			return null;
		}
	}

	public void EventReceiver(string msg, Person person){
		switch (mode) {
		case Mode.ALL:
			if (msg == "PersonEntered") {
				PersonEntered(person);
			} else if (msg == "PersonUpdated") {
				PersonUpdated(person);
			} else if (msg == "PersonWillLeave") {
				PersonWillLeave(person);
			}
			break;

		case Mode.NEWEST:
			if (person == auListener.GetNewest ()) {
				if (msg == "PersonEntered") {
					PersonEntered (person);
				} else if (msg == "PersonUpdated") {
					PersonUpdated (person);
				} else if (msg == "PersonWillLeave") {
					PersonWillLeave (person);
				}
			} else {
				if (msg == "PersonUpdated" || msg == "PersonWillLeave") {
					PersonWillLeave (person);
				}
			}
			break;

		case Mode.OLDEST:
			if (person == auListener.GetOldest ()) {
				if (msg == "PersonEntered") {
					PersonEntered (person);
				} else if (msg == "PersonUpdated") {
					PersonUpdated (person);
				} else if (msg == "PersonWillLeave") {
					PersonWillLeave (person);
				}
			} else {
				if (msg == "PersonUpdated" || msg == "PersonWillLeave") {
					PersonWillLeave (person);
				}
			}
			break;
		}

	}
	
	public void PersonEntered(Person person){
		//Debug.Log("Person entered pid : " + person.pid);
		if(!arrayPersonCubes.ContainsKey(person.pid)){
			GameObject personObject = (GameObject)Instantiate(personMarker, Vector3.zero, Quaternion.identity);
			personObject.transform.parent = boundingPlane.transform;
			// Reset transform
			personObject.transform.localPosition = Vector3.zero;
			personObject.transform.localRotation = Quaternion.identity;
			personObject.transform.localScale = Vector3.one;
			UpdatePerson(person, personObject);

			personObject.GetComponent<Renderer>().material = materials[person.pid % materials.Length];
			arrayPersonCubes.Add(person.pid,personObject);

			// Transmit message
			if (personEnteredMessage != null) {
				personEnteredMessage (person.pid, personObject);
			}
		}
	}

	public void PersonUpdated(Person person) {
		//Debug.Log("Person updated pid : " + person.pid);
		if (arrayPersonCubes.ContainsKey (person.pid)) {
			GameObject personObject = arrayPersonCubes [person.pid];
			UpdatePerson (person, personObject);

			// Transmit message
			if (personUpdatedMessage != null) {
				personUpdatedMessage (person.pid, personObject);
			}
		} else {
			PersonEntered (person);
		}
	}

	public void PersonWillLeave(Person person){
		//Debug.Log("Person leaving with ID " + person.pid);
		if(arrayPersonCubes.ContainsKey(person.pid)){
			//Debug.Log("Destroying cube");
			GameObject personObject = arrayPersonCubes[person.pid];
			// Send only the pid : the actual object will be destroyed
			arrayPersonCubes.Remove(person.pid);
			//delete it from the scene	
			Destroy(personObject);

			// Transmit message
			if (personWillLeaveMessage != null) {
				personWillLeaveMessage (person.pid);
			}
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
				pC.GetComponent<Renderer>().enabled = bEnable;
			}
		}
		
	}
	
	public void ClearAllPersons(){
		Debug.Log("Clear all cubes");
		foreach(var pKey in arrayPersonCubes.Keys){
			Destroy(arrayPersonCubes[pKey]);
		}
		arrayPersonCubes.Clear();
	}

	private void UpdatePerson(Person person, GameObject personObject){
		MovePerson(person, personObject);
		PersonObject po = personObject.GetComponent<PersonObject>();
		po.SetId(person.pid);
	}

	//maps the Augmenta coordinate system into one that matches the size of the boundingPlane
	private void MovePerson(Person person, GameObject personObject){

		Transform pt = personObject.transform;
		//Transform bt = boundingPlane.transform;
		Bounds meshBounds = boundingPlane.GetComponent<MeshFilter>().sharedMesh.bounds;

		pt.localPosition = new Vector3((float)(person.centroid.x - 0.5f) * meshBounds.size.x, pt.localScale.y/2, (float)(person.centroid.y - 0.5f) * meshBounds.size.z * -1);
		pt.localScale = new Vector3 (person.boundingRect.width * meshBounds.size.x, pt.localScale.y, person.boundingRect.height * meshBounds.size.z);

	}	

	// Return oldest tracked person
	public static GameObject GetOldest(){
		if (auListener.GetOldest() != null) {
			return arrayPersonCubes[auListener.GetOldest().pid];
		} else {
			return null;
		}
	}

	// Return newest tracked person
	public static GameObject GetNewest(){
		if (auListener.GetNewest() != null) {
			return arrayPersonCubes[auListener.GetNewest().pid];
		} else {
			return null;
		}
	}

	// Return all tracked persons
	public static Dictionary<int,GameObject> GetAll(){
		return arrayPersonCubes;
	}

	public static void SetMode(string modeName){
		try {
			mode = (Mode)System.Enum.Parse (typeof(Mode), modeName);
		} 
		catch(ArgumentException){
			Debug.LogError ("AugmentaManager : the mode " + modeName + " you try to set does not exist in AugmentaManager.mode enumeration");
		}
	}

	public static void SetMode(Mode newMode){
		mode = newMode;
	}


}
