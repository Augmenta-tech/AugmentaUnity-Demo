using UnityEngine;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using OSC.NET;

public class UnityOSCReceiver : MonoBehaviour {
	
	private bool connected = false;
	public int port = 12000;
	private OSCReceiver receiver;
	private Thread thread;
	
	private List<OSCMessage> processQueue = new List<OSCMessage>();		
	public delegate void OSCMessageReceivedHandler(OSCMessage msg);
	public static event OSCMessageReceivedHandler OSCMessageReceived;
	
	public UnityOSCReceiver() {}

	public int getPort() {
		return port;
	}
			
	public void Start() {
		LoadSettings ();
		connect();
	}

	public bool reconnect(){
		if (receiver != null){
			if (receiver.getPort() == port){
				return false;
			}
		}

		// else
		if(connected){
			disconnect();
			Invoke("connect", 10); //wait a tick to sync threading before reconnecting
		}
		else{
			connect();
		}
		return true;

	}
	
	public void connect(){
		
		try {
			Debug.Log("Connecting to port " + port);
			receiver = new OSCReceiver(port);
			connected = true;
			thread = new Thread(new ThreadStart(listen));
			Debug.Log("Starting listening thread...");
			thread.Start();
		} catch (Exception e) {
			Debug.Log("Failed to connect to port "+port);
			Debug.Log(e.Message);
		}
	}
	/**
	 * Call update every frame in order to dispatch all messages that have come
	 * in on the listener thread
	 */
	public void Update() {
		//processMessages has to be called on the main thread
		//so we used a shared proccessQueue full of OSC Messages
		lock(processQueue){
			foreach( OSCMessage message in processQueue){
				if(OSCMessageReceived != null){
					OSCMessageReceived(message); // Uses events/delegates for speed, as opposed to BroadcastMessage. Clients should subscribe to this event.
				}
			}
			processQueue.Clear();
		}
	}
	
	public void OnApplicationQuit(){
		disconnect();
		SaveSettings ();
	}
	
	public void disconnect() {
      	if (receiver!=null){
			Debug.Log("Disconnecting...");
      		 receiver.Close();
      	}
      	
       	receiver = null;
		connected = false;
	}

	public bool isConnected() { return connected; }

	private void listen() {
		Debug.Log("Listening to port " + port);
		while(connected) {
			try {
				OSCPacket packet = receiver.Receive();

				if (packet!=null) {
					lock(processQueue){
						//Debug.Log( "Adding  packets " + processQueue.Count );
						if (packet.IsBundle()) {
							ArrayList messages = packet.Values;
							for (int i=0; i<messages.Count; i++) {
								processQueue.Add( (OSCMessage)messages[i] );
							}
						} else{
							processQueue.Add( (OSCMessage)packet );
						}
					}
				} else Console.WriteLine("null packet");
			} catch (Exception e) {
				Debug.Log( e.Message );
				Console.WriteLine(e.Message);
			}
		}
	}

	void OnGUI(){
		if (!MainScript.hide) {
			GUI.Label (new Rect (20, 29, 65, 25), "Osc port");
			if (connected) {
				GUI.color = Color.green;
			} else {
				GUI.color = Color.red;
			}
			if (int.TryParse (GUI.TextField (new Rect (90, 30, 45, 20), port.ToString (), 25), out port)) {
				if (GUI.changed) {
					if (port > 1024) {
						if (reconnect ()) {
							Invoke ("callClearAllPersons", 0.1f);
						}
					} else {
						connected = false;
					}
				}
			}
			GUI.color = Color.white;
		}
	}

	void callClearAllPersons(){
		GameObject.Find("AugmentaReceiver").BroadcastMessage("clearAllPersons");
	}

	void SaveSettings(){
		PlayerPrefs.SetInt ("oscPort", port);
	}

	void LoadSettings(){
		port = PlayerPrefs.GetInt ("oscPort", port);
	}
}
