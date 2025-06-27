#if UNITY_EDITOR
using System;
using UnityEditor;
using Unity.Jobs;
using Unity.Networking.Transport;
using Unity.Collections;
using SimpleJSON;
using UnityEngine;
using T2G;
using UnityEditor.Compilation;
using Unity.EditorCoroutines.Editor;
using System.Collections.Generic;
using System.IO;
using System.Collections;

namespace T2G.Communicator
{
    public class CommunicatorServer : Communicator
    {
        const string k_BackupFileName = "PooledMessages.json";

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

        public override void Dispose()
        {
            if (IsActive && _connections != null)
            {
                for (int i = 0; i < _connections.Length; ++i)
                {
                    if (_connections[i] != null && _connections[i].IsCreated)
                    {
                        _networkDriver.Disconnect(_connections[i]);
                    }
                }
                base.Dispose();
            }
        }

        public override bool IsActive => (_networkDriver.IsCreated && _networkDriver.Listening);
        public override bool IsConnected
        {
            get
            {
                return (IsActive && 
                    _connections != null && _connections.Length > 0 &&
                    _connections[0] != null && _connections[0].IsCreated);
            }
        }

        public bool IsChannelConnected(int channel)
        {
            if(!IsActive ||
                _connections == null || _connections.Length < 1 || _connections[0] == null ||
                channel < 0 || channel >= _connections.Length)
            {
                return false;
            }
            return (_connections[channel] != null && _connections[channel].IsCreated);
        }

#region InitialOnloadProcess
        /*  The order of compilation-caused reload  
                 *  1. CompilationPipeline.compilationStarted
                 *  2. Reload is preformed
                 *  3. CompilationPipeline.compilationFinished
                 *  4. [InitializeOnLoad] class contructors
                 *  5. [InitializeOnLoadMethod] methods
                 *  6. AssetPostprocessor.OnPostprocessAllAssets is triggered (this may happen before the InitialOnLoads) 
        */
        static void OnCompilicationStartedHandler(object contextValue)
        {
            AssetPostProcessHandler.IsProcessingAssets = true;
            CommunicatorServer.Instance.SaveSendingAndReceivingPools();
        }

        static void OnCompilicationFinishedHandler(object contextValue)
        {
            //do nothing
        }
        
        [InitializeOnLoadMethod]
        static void InitialOnloadProcess()
        {
            CompilationPipeline.compilationStarted += OnCompilicationStartedHandler;
            CompilationPipeline.compilationFinished += OnCompilicationFinishedHandler;
        }

        ~CommunicatorServer()
        {
            CompilationPipeline.compilationStarted -= OnCompilicationStartedHandler;
            CompilationPipeline.compilationFinished -= OnCompilicationFinishedHandler;
        }

        [Serializable]
        class BackupPoolsData
        {
            public int[] SendingMessageTypes;
            public string[] SendingMessages;
            public int[] ReceivingMessageTypes;
            public string[] ReceivingMessages;
        }

        void SaveSendingAndReceivingPools()
        {
            List<int> messageTypes = new List<int>();
            List<string> messages = new List<string>();
            BackupPoolsData data = new BackupPoolsData();

            while(PopSendMessage(out var message))
            {
                messageTypes.Add((int)message.Type);
                messages.Add(message.Message.ToString());
            }
            data.SendingMessageTypes = messageTypes.ToArray();
            data.SendingMessages = messages.ToArray();
            
            messageTypes.Clear();
            messages.Clear();
            while (PopReceivedMessage(out var message))
            {
                messageTypes.Add((int)message.Type);
                messages.Add(message.Message.ToString());
            }
            data.ReceivingMessageTypes = messageTypes.ToArray();
            data.ReceivingMessages = messages.ToArray();

            string json = EditorJsonUtility.ToJson(data);
            string path = Path.Combine(Application.persistentDataPath, k_BackupFileName);
            File.WriteAllText(path, json);
            Debug.Log("[CommunicatorServer.SaveSendingAndReceivingPools] Pooled messages were saved!");
        }

        void RestoreSendingAndReceivingPools()
        {
            int i;
            string path = Path.Combine(Application.persistentDataPath, k_BackupFileName);
            if(File.Exists(path))
            {
                string json = File.ReadAllText(path);
                File.Delete(path);
                try
                {
                    var data = JsonUtility.FromJson<BackupPoolsData>(json);
                    for (i = 0; i < data.SendingMessageTypes.Length; ++i)
                    {
                        SendMessage((eMessageType)data.SendingMessageTypes[i], data.SendingMessages[i]);
                    }

                    for (i = 0; i < data.ReceivingMessageTypes.Length; ++i)
                    {
                        PoolReceivedMessage((eMessageType)data.SendingMessageTypes[i], data.SendingMessages[i]);
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError($"[CommunicatorServer.RestoreSendingAndReceivingPools] Pooled messages data file was corrupted!\n{e.Message}");
                }
            }
        }

        #endregion InitialOnloadProcess

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
            EditorCoroutineUtility.StartCoroutine(RestorePooledMessages(), this);
        }

        IEnumerator RestorePooledMessages()
        {
            float waitTime = 60.0f;  //Wait for 60 seconds for assets are ready.
            while(waitTime > 0.0f && AssetPostProcessHandler.IsProcessingAssets)
            {
                yield return new WaitForSeconds(1.0f);
                waitTime -= 1.0f;
            }
            if (waitTime > 0.0f)
            {
                CommunicatorServer.Instance.RestoreSendingAndReceivingPools();
                Debug.Log("[CommunicatorServer.RestorePooledMessages] Pooled messages were succedssfully restored!");
            }
            else
            {
                Debug.LogError("[CommunicatorServer.RestorePooledMessages] Timeout!");
            }
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
                var sendReceiveJob = new ServerReceiveJob()
                {
                    Driver = _networkDriver,
                    Connections = _connections,
                    ReceivePool = _receiveMessagePool,
                    SendPool = _sendMessagePool
                };
                _jobHandle = sendReceiveJob.Schedule(_jobHandle);
                _jobHandle.Complete();

                CommunicatorServer.Instance.SendPooledMessege(ref _sendMessagePool, ref _connections, ref _networkDriver);
            }

            ProcessPooledReceivedMessage();
        }

        protected async override void ProcessPooledReceivedMessage()
        {
            if (PopReceivedMessage(out var messageData) && messageData.Type == eMessageType.Instruction)
            {
                string msg = messageData.Message.ToString();
                OnReceivedMessage?.Invoke(eMessageType.Instruction, msg);
                JSONObject jsonObj = JSON.Parse(msg).AsObject;
                Instruction instruction = new Instruction(jsonObj);
                await Executor.Executor.Instance.Execute(instruction);
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

        struct ServerReceiveJob : IJob
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
                            case eMessageType.T2GSettings:
                                {
                                    SettingsT2G.FromJson(receivedMessage.Message.ToString(), false);
                                    comm.OnLogMessage?.Invoke("Received Resource Path: " + SettingsT2G.RecoursePath);
                                }
                                break;
                            case eMessageType.Instruction:
                                {
                                    JSONObject jsonObj = JSON.Parse(receivedMessage.Message.ToString()).AsObject;
                                    string keyword = jsonObj["Keyword"];
                                    comm.OnLogMessage?.Invoke("Instruction:" + keyword + 
                                        "\n    Data:" + jsonObj["Data"] + 
                                        "\n    AssetPaths:" + jsonObj["ResolvedAssetPaths"]);
                                    Instruction instruction = new Instruction();
                                    instruction.Keyword = keyword;
                                    instruction.DataType = (Instruction.EDataType)jsonObj["DataType"].AsInt;
                                    instruction.Data = jsonObj["Data"];
                                    instruction.ResolvedAssetPaths = jsonObj["ResolvedAssetPaths"];
                                    instruction.RequiresPreviousSuccess = jsonObj["RequiresPreviousSuccess"].AsBool;
                                    instruction.ExecutionType = (Instruction.EExecutionType)jsonObj["ExecutionType"].AsInt;
                                    instruction.State = (Instruction.EInstructionState)jsonObj["State"].AsInt;
                                    Executor.Executor.Instance.EnqueueInstruction(instruction);
                                }
                                break;
                            case eMessageType.Message:
                            case eMessageType.Response:
                            default:
                                comm.PoolReceivedMessage(receivedMessage, ref ReceivePool);
                                break;
                        }
                    }
                    else if (command == NetworkEvent.Type.Disconnect)
                    {
                        Connections[0] = default;
                        comm.OnClientDisconnected?.Invoke();
                    }
                }
            }
        }
    }
}

#endif