/*
© Siemens AG, 2017-2018
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

using System.Threading;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    [RequireComponent(typeof(RosConnector))]
    public abstract class UnitySubscriber<T> : MonoBehaviour where T : Message, new()
    {
        public string Topic;
        public float TimeStep;
        public bool AutoResubscribeOnConnected = true;

        private RosConnector rosConnector;
        private readonly int SecondsTimeout = 1;

        protected RosMsgForwardService rosMsgForwardService;
        private bool addedReconnectionEventHandler = false;

        protected virtual void Start()
        {
            rosMsgForwardService = (RosMsgForwardService)Toolkit.singleton.GetService("RosMsgForwardService");

            if (rosMsgForwardService == null)
            {
                Toolkit.singleton.RegisterServiceOfferer(new RosMsgForwardService());
                rosMsgForwardService = (RosMsgForwardService)Toolkit.singleton.GetService("RosMsgForwardService");
            }

            rosConnector = GetComponent<RosConnector>();
            new Thread(Subscribe).Start();
        }

        private void Subscribe()
        {
            if (AutoResubscribeOnConnected && !addedReconnectionEventHandler)
            {
                addedReconnectionEventHandler = true;
                rosConnector.OnRosConnectorReConnected += Protocol_OnConnected;
            }
            if (!rosConnector.IsConnected.WaitOne(SecondsTimeout * 1000))
                Debug.LogWarning("Failed to subscribe: RosConnector not connected");

            if (rosConnector.RosSocket != null) //Can be null if no connetion could established
                rosConnector.RosSocket.Subscribe<T>(Topic, ReceiveMessage, (int)(TimeStep * 1000)); // the rate(in ms in between messages) at which to throttle the topics
        }

        private void Protocol_OnConnected(object sender, System.EventArgs e)
        {
            Subscribe();
        }

        protected abstract void ReceiveMessage(T message);
    }
}