using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Augmenta;

/*
    * Augmenta OSC Protocol :

        /au/personWillLeave/ args0 arg1 ... argn
        /au/personUpdated/   args0 arg1 ... argn
        /au/personEntered/   args0 arg1 ... argn

        where args are :

        0: pid (int)
        1: oid (int)
        2: age (int)
        3: centroid.x (float)
        4: centroid.y (float)
        5: velocity.x (float)
        6: velocity.y (float)
        7: depth (float)
        8: boundingRect.x (float)
        9: boundingRect.y (float)
        10: boundingRect.width (float)
        11: boundingRect.height (float)
        12: highest.x (float)
        13: highest.y (float)
        14: highest.z (float)
        15:
        16:
        17:
        18:
        19:
        20+ : contours (if enabled)

        /au/scene/   args0 arg1 ... argn

        0: currentTime (int)
        1: percentCovered (float)
        2: numPeople (int)
        3: averageMotion.x (float)
        4: averageMotion.y (float)
        5: scene.width (int)
        6: scene.height (int)
*/

public class auListener : MonoBehaviour  {

	// Broadcast augmenta message DELEGATE
	public delegate void broadcastMessageHandler(string msg, Person person);
	public static event broadcastMessageHandler broadcastMessage;

	// Scene object
	private Scene scene;

	// Number of frames before a point who hasn't been updated is removed
	public static float timeOut = .5f; // seconds

	// Dinctionnary of persons
	private static Dictionary<int, Person> arrayPerson = new Dictionary<int, Person>(); // Containing all current persons

	// Smooth factor for smoothing augmenta data
	private float smoothAmount = 0;
	public float SmoothAmount{
		get{
			return smoothAmount;
		}
		set{
			if(value < 0){
				smoothAmount = 0;
			} else if(value > 0.99){
				smoothAmount = 0.99f;
			} else {
				smoothAmount = value;
			}
		}
	}

	public Scene GetScene(){
		return scene;
	}

	public int arrayPersonCount(){
		return arrayPerson.Count;
	}

	public static Dictionary<int, Person> GetPeopleArray(){
		return arrayPerson;
	}

	public void Start(){
		Debug.Log("[Augmenta] Subscribing to OSC Message Receiver");
		UnityOSCReceiver.OSCMessageReceived += new UnityOSCReceiver.OSCMessageReceivedHandler(OSCMessageReceived);

		scene = new Scene ();

		// Start the coroutine that check if everyone is alive
		StartCoroutine("checkAlive");
	}
	
	public void onEnable(){
		Debug.Log("[Augmenta] Subscribing to OSC Message Receiver");
		UnityOSCReceiver.OSCMessageReceived += new UnityOSCReceiver.OSCMessageReceivedHandler(OSCMessageReceived);
	}

	public void onDisable(){
		Debug.Log("[Augmenta] Unsubscribing to OSC Message Receiver");
		UnityOSCReceiver.OSCMessageReceived -= new UnityOSCReceiver.OSCMessageReceivedHandler(OSCMessageReceived);
	}
	
	public void OSCMessageReceived(OSC.NET.OSCMessage message){
		string address = message.Address;
		ArrayList args = message.Values;

		//Debug.Log("OSC received with address : "+address);
		
		if (address == "/au/personEntered/" || address == "/au/personEntered") {
			int pid = (int)args[0];
			Person currentPerson = null;
			if (!arrayPerson.ContainsKey(pid)) {
				addPerson(args);
			} else {
				currentPerson = arrayPerson[pid];
				updatePerson(currentPerson, args);
				//personUpdated(person);
				if(broadcastMessage != null){
					broadcastMessage("PersonUpdated", currentPerson);
				}
			}

		}
		else if(address == "/au/personUpdated/" || address == "/au/personUpdated"){				
			int pid = (int)args[0];
			Person currentPerson = null;
			if (!arrayPerson.ContainsKey(pid)) {
				currentPerson = addPerson(args);
			}
			else{
				currentPerson = arrayPerson[pid];
				updatePerson(currentPerson, args);
				//personUpdated(person);
				if(broadcastMessage != null){
					broadcastMessage("PersonUpdated", currentPerson);
				}
			}
		}
		else if(address == "/au/personWillLeave/" || address == "/au/personWillLeave"){
			int pid = (int)args[0];
			if (arrayPerson.ContainsKey(pid)) {
				Person personToRemove = arrayPerson[pid];				
				if(broadcastMessage != null){
					broadcastMessage("PersonWillLeave", personToRemove);
				}
				arrayPerson.Remove(pid);
				//personWillLeave(personToRemove);
			}
		}
		else if(address == "/au/scene/" || address == "/au/scene"){
			scene.currentTime = (int)args [0];
			scene.percentCovered = (float)args [1];
			scene.numPeople = (int)args [2];
			scene.averageMotion = new Vector2 ((float)args [3], (float)args [4]);
			scene.width = (int)args [5];
			scene.height = (int)args [6];
			scene.depth = (int)args [7];
		}
		else{
			//print(address + " ");
		}
	}
		
	private Person addPerson(ArrayList args) {
		Person newPerson = new Person();
		updatePerson(newPerson, args);
		arrayPerson.Add(newPerson.pid, newPerson);	
		//personEntered(newPerson);
		if(broadcastMessage != null){
			broadcastMessage("PersonEntered", newPerson);
		}
		return newPerson;
	}
	
	private void updatePerson(Person p, ArrayList args) {
		p.pid = (int)args[0];
		p.oid = (int)args[1];
		p.age = (int)args[2];
		p.centroid.x = (float)args[3];
		p.centroid.y = (float)args[4];
		p.velocity.x = (float)args[5];
		p.velocity.y = (float)args[6];
		p.depth = (float)args[7];
		p.boundingRect.x = (float)args[8];
		p.boundingRect.y = (float)args[9];
		p.boundingRect.width = (float)args[10];
		p.boundingRect.height = (float)args[11];
		p.highest.x = (float)args[12];
		p.highest.y = (float)args[13];
		p.highest.z = (float)args[14];

		//Debug.Log("Update with width = "+p.boundingRect.width+" and height = "+p.boundingRect.height);

		if(smoothAmount != 0){
			p.smooth(smoothAmount);
		}

		// Inactive time reset to zero : the point has just been updated
		p.lastUpdateTime = Time.time;
	}

	public void clearAllPersons(){
		arrayPerson.Clear();
	}

	// Co-routine to check if person is alive or not
	IEnumerator checkAlive() {
		while(true) {
			ArrayList ids = new ArrayList();
			foreach(KeyValuePair<int, Person> p in arrayPerson) {
				ids.Add(p.Key);
			}
			foreach(int id in ids) {
				if(arrayPerson.ContainsKey(id)){
								
					Person p = arrayPerson[id];
					
					if(Time.time > p.lastUpdateTime + timeOut) {
						//Debug.Log("***: DESTROY");
						// The point hasn't been updated for a certain number of frames : remove
						arrayPerson.Remove(id);
						if(broadcastMessage != null){
							broadcastMessage("PersonWillLeave", p);
						}
					}
				}
			}
			ids.Clear();
			yield return 0;
		}
	}

	public static Person GetOldest(){
		Person bestPerson = null;
		int maxAge = 0;
		foreach(KeyValuePair<int, Person> p in arrayPerson) {
			if(p.Value.age > maxAge){
				bestPerson = p.Value;
				maxAge = p.Value.age;
			}
			// If several persons have the same oldest age, take the one with smallest pid
			else if(p.Value.age == maxAge){
				if(p.Value.pid < bestPerson.pid){
					bestPerson = p.Value;
				}
			}
		}
		return bestPerson;
	}
	
	public static Person GetNewest(){
		Person bestPerson = null;
		int minAge = int.MaxValue;
		foreach(KeyValuePair<int, Person> p in arrayPerson) {
			if(p.Value.age < minAge){
				bestPerson = p.Value;
				minAge = p.Value.age;
			}
			// If several persons have the same newest age, take the one with greatest pid
			else if(p.Value.age == minAge){
				if(p.Value.pid > bestPerson.pid){
					bestPerson = p.Value;
				}
			}
		}
		return bestPerson;
	}

}
