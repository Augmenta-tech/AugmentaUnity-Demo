using System;
using System.Net;
using UnityEngine;
using System.Collections.Generic;

namespace Augmenta.UnityOSC
{
    public class OSCMaster : MonoBehaviour
    {
        private static OSCMaster _instance;
        public static OSCMaster Instance {
            get {

				//if (_instance == null) {
				//    Debug.Log("Creating OSCMaster gameobject");
				//    _instance = new GameObject("OSCMaster").AddComponent<OSCMaster>();
				//}

				return _instance;
            }
            set {
                _instance = value;
            }
        }

        public static Dictionary<string, OSCReceiver> Receivers;
        public static Dictionary<string, OSCClient> Clients;

        public bool ShowDebug;

        public bool LogIncoming;
        public bool LogOutgoing;

        void Awake() {
            Instance = this;
            Receivers = new Dictionary<string, OSCReceiver>();
            Clients = new Dictionary<string, OSCClient>();
        }

        private void Update() {
            foreach (var receiver in Receivers) {
                while (receiver.Value.WaitingMessagesCount() > 0) //Allow to switch from receiver/server thread to main thread
                    receiver.Value.PropagateEvent();
            }
        }

        public static bool HasClient(string clientId) {
            return Clients.ContainsKey(clientId);
        }

        public static bool HasReceiver(string receiverId) {
            return Receivers.ContainsKey(receiverId);
        }

        public static void CreateClient(string clientId, string destination, int port) {
            CreateClient(clientId, IPAddress.Parse(destination), port);
        }

        public static void CreateClient(string clientId, IPAddress destination, int port) {
            var client = new OSCClient(destination, port) {
                Name = clientId
            };

            Clients.Add(clientId, client);

            if (Instance.ShowDebug)
                Debug.Log("Client " + clientId + " on " + destination + ":" + port + "created.");
        }

        public static void RemoveClient(string clientId) {
            Clients[clientId].Close();
            Clients.Remove(clientId);

            if (Instance.ShowDebug)
                Debug.Log("Client " + clientId + " removed.");
        }

        public static OSCReceiver CreateReceiver(string receiverId, int port) {
            var receiver = new OSCReceiver {
                Name = receiverId
            };

            if (!receiver.Open(port)) {
                return null;
            }

            Receivers.Add(receiverId, receiver);

            if (Instance.ShowDebug)
                Debug.Log("Receiver " + receiverId + " on " + port + " created.");

            return receiver;
        }

        public static void RemoveReceiver(string receiverId) {
            Receivers[receiverId].Close();
            Receivers.Remove(receiverId);

            if (Instance.ShowDebug)
                Debug.Log("Receiver " + receiverId + " removed.");
        }

        public static void SendMessageUsingClient(string clientId, OSCMessage msg) {
            Clients[clientId].Send(msg);

            if (Instance.LogOutgoing) {
                Debug.Log("[" + clientId + " to " + Clients[clientId].ClientIPAddress.ToString() + ":" + Clients[clientId].Port + "|" + DateTime.Now.ToLocalTime() + "] " + msg.Address);
                foreach (var data in msg.Data)
                    Debug.Log(data);
            }
        }

        public static void SendMessage(OSCMessage m, string host, int port) {
            if (Instance.LogOutgoing) {
                string args = "";
                for (int i = 0; i < m.Data.Count; i++)
                    args += (i > 0 ? ", " : "") + m.Data[i].ToString();

                Debug.Log("[OSCMaster to" + host + ":" + port + " | " + DateTime.Now.ToLocalTime() + "] " + m.Address + " : " + args);
            }

            var tempClient = new OSCClient(System.Net.IPAddress.Loopback, port);
            tempClient.SendTo(m, host, port);
            tempClient.Close();
        }


        void OnDestroy() {
            foreach (var pair in Clients) {
                pair.Value.Close();
            }

            foreach (var pair in Receivers) {
                pair.Value.Close();
            }

            //_instance = null;
        }
    }
}
