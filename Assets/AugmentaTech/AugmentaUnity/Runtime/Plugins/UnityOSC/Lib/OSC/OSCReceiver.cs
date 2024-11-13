//
//	  UnityOSC - Open Sound Control interface for the Unity3d game engine
//
//	  Copyright (c) 2012 Jorge Garcia Martin
//	  Last edit: Gerard Llorach 2nd August 2017
//
// 	  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// 	  documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// 	  the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
// 	  and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// 	  The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// 	  of the Software.
//
// 	  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// 	  TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// 	  THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// 	  CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// 	  IN THE SOFTWARE.
//


using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Augmenta.UnityOSC
{
    public class OSCReceiver
    {
        public delegate void MessageReceived(OSCMessage message);
        public event MessageReceived messageReceived;

        public string Name;

        private Queue<OSCMessage> _queue;
        private OSCServer _server;

        public bool Open(int port)
        {
            _queue = new Queue<OSCMessage>();
#if UNITY_EDITOR
            if (PlayerSettings.runInBackground == false)
            {
                Debug.LogWarning("Recommend PlayerSettings > runInBackground = true");
            } 
#endif
            if (_server != null)
            {
                _server.Close();
            }

            try
            {
                _server = new OSCServer(port);
                _server.SleepMilliseconds = 0;
                _server.PacketReceivedEvent += didReceivedEvent;
            }
            catch(Exception e)
            {
                Debug.LogError("[" + Name + "] Couldn't open port " + port + " | " +e.Message);
                return false;
            }

            return true;
        }
        
        public void PropagateEvent()
        {
            var msg = GetLastMessage();
           //if (msg == null)
               // return;

            if (messageReceived != null)
                messageReceived(msg);
        }

        public OSCMessage GetLastMessage()
        {
            var lastMessage = new OSCMessage("");
            lock (_queue)
            {
                lastMessage = _queue.Dequeue();
            }
            return lastMessage;
        }

        public int WaitingMessagesCount()
        {
            return _queue.Count;
        }

        public void Close()
        {
            if (_server != null)
            {
                _server.Close();
                _server = null;
            }
        }
        
        void didReceivedEvent(OSCServer sender, OSCPacket packet)
        {
            lock (_queue)
            {
                if (packet.IsBundle())
                {
                    var bundle = packet as OSCBundle;

                    foreach (object obj in bundle.Data)
                    {
                        OSCMessage msg = obj as OSCMessage;

                        if (OSCMaster.Instance.LogIncoming)
                        {
                            Debug.Log("[" + Name + "] " + msg.Address);
                            foreach (var data in msg.Data)
                                Debug.Log(data);
                        }

                        _queue.Enqueue(msg);
                    }
                }
                else
                {
                    OSCMessage msg = packet as OSCMessage;

                    if (OSCMaster.Instance.LogIncoming)
                    {
                        Debug.Log("[" + Name + "] " + msg.Address);
                        foreach (var data in msg.Data)
                            Debug.Log(data);
                    }

                    _queue.Enqueue(msg);
                }
            }
        }
    }
}