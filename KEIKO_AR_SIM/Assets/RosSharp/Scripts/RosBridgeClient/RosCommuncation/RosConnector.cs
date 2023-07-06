/*
© Siemens AG, 2017-2019
Author: Dr. Martin Bischoff (martin.bischoff@siemens.com)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

<http://www.apache.org/licenses/LICENSE-2.0>.

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Collections;
using System.Threading;
using RosSharp.RosBridgeClient.Protocols;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class RosConnector : MonoBehaviour, IServiceConsumer<IServiceMessage>
    {
        public int SecondsTimeout = 10;

        public RosSocket RosSocket { get; private set; }
        public RosSocket.SerializerEnum Serializer;
        public Protocol protocol;
        public string RosBridgeServerUrl = "ws://localhost:9090";
        public bool AutoReconnectOnConnectionLost = true;
        public event EventHandler OnRosConnectorReConnected;
        public bool isConnected = false;

        public ManualResetEvent IsConnected { get; private set; }

        [Header("Filter strings for Logs")]
        [Tooltip("Logs from the library are filtered using these strings. Only if it does not start with any of them, it is logged.")]
        public string[] FilterStrings = new string[]
           {
                "{\"topic\":"
           };

        /// <summary> 
        /// Filters the input string and logs it if all filters do not apply. 
        /// </summary> 
        /// <param name="originalLog"></param> 
        private void FilteredDebugLog(string originalLog)
        {
            foreach (var filter in FilterStrings)
            {
                if (originalLog.StartsWith(filter)) return;
            }

            Debug.Log(originalLog);
        }

        public virtual void Awake()
        {
            Output.SetLogDelegate(FilteredDebugLog);

            RosBridgeServerUrl = PlayerPrefs.GetString("server_address");
            if (String.IsNullOrEmpty(RosBridgeServerUrl))
                RosBridgeServerUrl = "ws://192.168.1.121:9090";

#if WINDOWS_UWP
            // overwrite selection
            protocol = Protocol.WebSocketUWP;
#endif
            IsConnected = new ManualResetEvent(false);

            if (protocol == Protocol.WebSocketUWP)
            {
                manualOnCloseTrigger = true;
            }
        }

        public void Start()
        {
            Toolkit.singleton.RegisterServiceConsumer(this, "ConnectionStateService");
        }

        protected void ConnectAndWait()
        {
            RosSocket = ConnectToRos(protocol, RosBridgeServerUrl, OnConnected, OnClosed, Serializer);
        }

        public static RosSocket ConnectToRos(Protocol protocolType, string serverUrl, EventHandler onConnected = null, EventHandler onClosed = null, RosSocket.SerializerEnum serializer = RosSocket.SerializerEnum.Microsoft)
        {
            IProtocol protocol = ProtocolInitializer.GetProtocol(protocolType, serverUrl);
            protocol.OnConnected += onConnected;
            protocol.OnClosed += onClosed;

            return new RosSocket(protocol, serializer);
        }


        private static RosBridgeClient.Protocols.IProtocol GetProtocol(Protocol protocol, string rosBridgeServerUrl)
        {

#if WINDOWS_UWP
            Debug.Log("Defaulted to UWP Protocol");
            return new RosBridgeClient.Protocols.WebSocketUWPProtocol(rosBridgeServerUrl);
#else
            switch (protocol)
            {
                case Protocol.WebSocketNET:
                    return new RosBridgeClient.Protocols.WebSocketNetProtocol(rosBridgeServerUrl);
                case Protocol.WebSocketSharp:
                    return new RosBridgeClient.Protocols.WebSocketSharpProtocol(rosBridgeServerUrl);
                case Protocol.WebSocketUWP:
                    Debug.Log("WebSocketUWP only works when deployed to HoloLens, defaulting to WebSocketNetProtocol");
                    return new RosBridgeClient.Protocols.WebSocketNetProtocol(rosBridgeServerUrl);
                default:
                    return null;
            }
#endif
        }

        private void OnApplicationQuit()
        {
            RosSocket.Close();
        }

        private void OnConnected(object sender, EventArgs e)
        {
            isConnected = true;
            IsConnected.Set();
            Debug.Log("Connected to RosBridge.");

            OnRosConnectorReConnected?.Invoke(this, EventArgs.Empty);
        }

        private void OnClosed(object sender, EventArgs e)
        {
            isConnected = false;
            IsConnected.Reset();
            Debug.Log("Disconnected from RosBridge: " + RosBridgeServerUrl);
        }

        public void Update()
        {
            if (startReconnectionCycle)
            {
                startReconnectionCycle = false;
                StartCoroutine(Reconnect());
            }
        }

        private bool startReconnectionCycle = true;



        /// <summary>
        /// Attempts to reconnect until a connection is created
        /// </summary>
        /// <param name="waitForSeconds"></param>
        /// <returns></returns>
        IEnumerator Reconnect()
        {
            if (isConnected)
                yield break;

            Debug.Log($"Attempting to connect to {RosBridgeServerUrl}");

            new Thread(ConnectAndWait).Start();
            
            yield return new WaitForSeconds(2);

            if (isConnected)
                yield break;

            yield return Reconnect();
        }

        public void ConsumeServiceItem(IServiceMessage item, string serviceName)
        {
            if (!((ConnectionStateMessage)item).ConnectionState)
            {
                if (AutoReconnectOnConnectionLost)
                {
                    startReconnectionCycle = true;
                }

                if (manualOnCloseTrigger)
                {
                    OnClosed(this, EventArgs.Empty);
                }
            }
        }




        #region OnClose overwrite

        private bool manualOnCloseTrigger = false;

        #endregion
    }
}
