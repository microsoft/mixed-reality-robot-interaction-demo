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

using System.ComponentModel;
using System.Threading;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    [RequireComponent(typeof(RosConnector))]
    public abstract class UnityPublisher<T> : MonoBehaviour where T : Message, new()
    {
        public string Topic;
        protected string publicationId;
        protected bool canPublish = false;
        public bool AutoResubscribeOnConnected = true;

        private bool addedReconnectionEventHandler = false;

        protected RosConnector rosConnector;

        protected virtual void Start()
        {
            rosConnector = GetComponent<RosConnector>();

            if (!addedReconnectionEventHandler)
            {
                rosConnector.OnRosConnectorReConnected += RosConnector_OnRosConnectorReConnected;
                addedReconnectionEventHandler = true;
            }

            if (rosConnector.isConnected)
                new Thread(HandleAdvertisement).Start();
        }

        private void HandleAdvertisement()
        {
            if (!rosConnector.IsConnected.WaitOne(1000))
            {
                Debug.LogWarning("Failed to upblish: RosConnector not connected");
            }

            publicationId = rosConnector.RosSocket.Advertise<T>(Topic);
            if (!string.IsNullOrEmpty(publicationId))
            {
                canPublish = true;
            }
            else
            {
                Debug.Log("Failed to Advertise!");
            }
        }

        private void RosConnector_OnRosConnectorReConnected(object sender, System.EventArgs e)
        {
            HandleAdvertisement();
        }

        protected void Publish(T message)
        {
            if (string.IsNullOrEmpty(publicationId))
            {
                Debug.LogWarning("Publication id was null. Publishing did not succeed.");
                return;
            }

            if (!rosConnector.isConnected)
            {
                return;
            }

            rosConnector.RosSocket.Publish(publicationId, message);
        }
    }
}