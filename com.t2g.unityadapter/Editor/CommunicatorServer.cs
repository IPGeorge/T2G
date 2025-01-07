#if UNITY_EDITOR
using System;
using UnityEditor;
using Unity.Jobs;
using Unity.Networking.Transport;
using Unity.Collections;
using SimpleJSON;
using UnityEngine;

namespace T2G.UnityAdapter
{
    public class CommunicatorServer : Communicator
    {
        public Action OnServerStarted;
        public Action OnFailedToStartServer;
        public Action OnClientConnected;
        public Action BeforeDisconnectClient;
        public Action AfterDisconnectClient;
        public Action OnClientDisconnected;
        public Action BeforeShutdownServer;
        public Action AfterShutdownServer;
        public Action<string> OnLogMessage;
        public Action<string> OnReceivedInstruction;

        static CommunicatorServer _instance = null;
        public static CommunicatorServer Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new CommunicatorServer();
                }
                return _instance;
            }
        }

        protected override void Dispose()
        {
            if (IsActive)
            {
                if (_connections[0].IsCreated)
                {
                    _networkDriver.Disconnect(_connections[0]);
                }
                base.Dispose();
            }
        }

        public void StartServer()
        {
            if (!IsActive)
            {
                Instance.Initialize();
            }

            var endpoint = NetworkEndpoint.AnyIpv4.WithPort(Port);
            if (_networkDriver.Bind(endpoint) == 0)
            {

                _networkDriver.Listen();
                OnServerStarted?.Invoke();
            }
            else
            {
                Dispose();
#if T2G
                EditorPrefs.SetBool(Defs.k_StartListener, false);
#endif
                OnFailedToStartServer?.Invoke();
            }

            EditorApplication.update += UpdateServer;
        }

        public void StopServer()
        {
            BeforeShutdownServer?.Invoke();
            
            EditorApplication.update -= UpdateServer;
            
            if (_connections != null && _connections.IsCreated && _connections.Length > 0  && _connections[0].IsCreated)
            {
                BeforeDisconnectClient?.Invoke();
                _networkDriver.Disconnect(_connections[0]);
                _connections[0] = default(NetworkConnection);
                OnClientDisconnected?.Invoke();
            }

            Dispose();
            AfterShutdownServer?.Invoke();
        }

        void UpdateServer()
        {
            if(!IsActive)
            {
                return;
            }

            _jobHandle = _networkDriver.ScheduleUpdate();

            if (!IsConnected)
            {
                //Debug.Log("Waiting for connection ...");
                var connectionJob = new ServerConnectionJob()
                {
                    Driver = _networkDriver,
                    Connections = _connections
                };

                _jobHandle = connectionJob.Schedule(_jobHandle);
                _jobHandle.Complete();
                if (IsConnected)
                {
                    OnClientConnected?.Invoke();
                }
            }

            if (IsConnected)
            {
                var sendReceiveJob = new ServerSendReceiveJob()
                {
                    Driver = _networkDriver,
                    Connections = _connections,
                    ReceivePool = _receiveMessagePool,
                    SendPool = _sendMessagePool
                };
                _jobHandle = sendReceiveJob.Schedule(_jobHandle);
                _jobHandle.Complete();
            }

            ProcessPooledReceivedMessage();
        }

        protected override void ProcessPooledReceivedMessage()
        {
            MessageStruct messageData;
            if (GetReceivedMessage(out messageData))
            {
                string msg = messageData.Message.ToString();
                OnReceivedMessage?.Invoke(msg);
                Executor.Instance.Execute(msg);
            }
        }

        struct ServerConnectionJob : IJob
        {
            public NetworkDriver Driver;
            public NativeArray<NetworkConnection> Connections;

            public void Execute()
            {
                NetworkConnection newConnection;
                while ((newConnection = Driver.Accept()) != default)
                {
                    Connections[0] = newConnection;
                }
            }
        }

        struct ServerSendReceiveJob : IJob
        {
            public NetworkDriver Driver;
            public NativeArray<NetworkConnection> Connections;
            public NativeArray<MessageStruct> ReceivePool;
            public NativeArray<MessageStruct> SendPool;

            public void Execute()
            {
                if (!Connections[0].IsCreated)
                {
                    return;
                }

                DataStreamReader readStream;
                NetworkEvent.Type command;
                var comm = CommunicatorServer.Instance;
                while ((command = Connections[0].PopEvent(Driver, out readStream)) != NetworkEvent.Type.Empty)
                {
                    if (command == NetworkEvent.Type.Data)
                    {
                        var receivedMessage = new MessageStruct()
                        {
                            Type = (eMessageType)readStream.ReadInt(),
                            Message = readStream.ReadFixedString4096()
                        };

                        switch(receivedMessage.Type)
                        {
                            case eMessageType.SettingsData:
                                {
                                    Settings.FromJson(receivedMessage.Message.ToString(), false);
                                    CommunicatorServer.Instance.OnLogMessage?.Invoke("Received Resource Path: " + Settings.RecoursePath);
                                }
                                break;
                            default:
                                comm.PoolReceivedMessage(receivedMessage, ref ReceivePool);
                                break;
                        }
                    }
                    else if (command == NetworkEvent.Type.Disconnect)
                    {
                        Connections[0] = default;
                        CommunicatorServer.Instance.OnClientDisconnected?.Invoke();
                    }
                }

                comm.SendPooledMessege(ref SendPool, ref Connections, ref Driver);
            }
        }
    }
}
#endif