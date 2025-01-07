using System;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Jobs;
using Unity.Collections;


namespace T2G.UnityAdapter
{
    public class CommunicatorClient : Communicator
    {
        public enum eClientState
        {
            Disconnected = 0,
            Connecting,
            Connected,
            Disconnecting
        }

        public Action OnConnectedToServer;
        public Action OnFailedToConnectToServer;
        public Action BeforeDiscinnectFromServer;
        public Action OnDisconnectedFromServer;
        public bool Silent { get; private set; } = false;

        float _connectTimer = 0.0f;
        readonly float k_connectTimeout = 3.0f;

        public eClientState ClientState { get; private set; } = eClientState.Disconnected;
        public override bool IsConnected
        {
            get
            {
                return (ClientState == eClientState.Connected);
            }
        }

        static CommunicatorClient _instance = null;
        public static CommunicatorClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CommunicatorClient();
                }
                return _instance;
            }
        }

        public void StartClient(float timeout = 0.0f, bool silent = false)
        {
            if (ClientState != eClientState.Disconnected)
            {
                return;
            }

            Silent = silent;

            if (IsActive)
            {
                Dispose();
            }

            Initialize();

            NetworkEndpoint endPoint;
            if (!NetworkEndpoint.TryParse(IPAddress, Port, out endPoint))
            {
                endPoint = NetworkEndpoint.LoopbackIpv4.WithPort(Port);
            }
            _connectTimer = (timeout <= 0.0f) ? k_connectTimeout : timeout;
            _connections[0] = _networkDriver.Connect(endPoint);
            ClientState = eClientState.Connecting;
        }

        public void Disconnect()
        {
            _jobHandle.Complete();
            if (ClientState == eClientState.Connected || ClientState == eClientState.Connecting)
            {
                if (!Silent)
                {
                    BeforeDiscinnectFromServer?.Invoke();
                }
                _connections[0].Disconnect(_networkDriver);
                _networkDriver.ScheduleUpdate().Complete();
                ClientState = eClientState.Disconnected;
            }
        }

        protected override void SendPooledMessege()
        {
            if (ClientState == eClientState.Connecting)
            {
                return;
            }
                
            if (ClientState == eClientState.Disconnected)
            {
                StartClient();
            }
            else
            {
                base.SendPooledMessege();
            }
        }

        public void UpdateClient()
        {
            _jobHandle.Complete();
            
            if (ClientState == eClientState.Connecting)
            {
                _connectTimer -= Time.deltaTime;

                if (_connectTimer <= 0.0f)
                {
                    ClientState = eClientState.Disconnected;
                    Dispose();
                    if (!Silent)
                    {
                        OnFailedToConnectToServer?.Invoke();
                    }
                }
            }

            if (ClientState == eClientState.Connected)
            {
                SendPooledMessege();
            }

            if (_networkDriver.IsCreated)
            {
                var job = new ClientJob()
                {
                    Driver = _networkDriver,
                    Connections = _connections,
                    ReceivePool = _receiveMessagePool
                };

                _jobHandle = _networkDriver.ScheduleUpdate();
                _jobHandle = job.Schedule(_jobHandle);
                _jobHandle.Complete();
            }

            ProcessPooledReceivedMessage();
        }


        struct ClientJob : IJob
        {
            public NetworkDriver Driver;
            public NativeArray<NetworkConnection> Connections;
            public NativeArray<MessageStruct> ReceivePool;

            public void Execute()
            {
                if (!Connections[0].IsCreated)
                {
                    return;
                }

                DataStreamReader readStream;
                NetworkEvent.Type command;
                var comm = CommunicatorClient.Instance;
                while ((command = Connections[0].PopEvent(Driver, out readStream)) != NetworkEvent.Type.Empty)
                {
                    if (command == NetworkEvent.Type.Data) //Received data
                    {
                        var receivedMessage = new MessageStruct()
                        {
                            Type = (eMessageType)readStream.ReadInt(),
                            Message = readStream.ReadFixedString4096()
                        };

                        comm.PoolReceivedMessage(receivedMessage, ref ReceivePool);
                    }
                    else if (command == NetworkEvent.Type.Connect)  //Connected
                    {
                        comm.ClientState = eClientState.Connected;
                        if (!comm.Silent)
                        {
                            comm.OnConnectedToServer?.Invoke();
                        }
                    }
                    else if (command == NetworkEvent.Type.Disconnect) //Disconnected
                    {
                        comm.ClientState = eClientState.Disconnected;
                        comm.Dispose();
                        if (!comm.Silent)
                        {
                            comm.OnDisconnectedFromServer?.Invoke();
                        }
                    }
                }
            }
        }
    }
}