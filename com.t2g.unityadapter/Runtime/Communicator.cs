using System;
using Unity.Jobs;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using SimpleJSON;

namespace T2G.UnityAdapter
{
    public enum eMessageType : byte     //TODO: used to improve the communication's message serialization and deserialization 
    {
        //Primitive types
        Int,
        Byte,
        Bytes,
        Float,
        Double,
        Long,
        Short,
        UInt,
        ULong,
        UShort,
        PackedInt,
        PackedFloat,
        PackedDouble,
        PackedLong,
        PackedUInt,
        PackedULong,
        String32,
        String64,
        String128,
        String512,
        String4096,
        RawBits,
        //System reserved types
        SettingsData,
        //Complex data structures
        //...
    }

    public struct MessageStruct
    {
        public eMessageType Type;
        public FixedString4096Bytes Message;
    }

    [Serializable]
    public class Settings
    {
        public static bool Loaded { get; private set; } = false;
        public static string UnityEditorPath;
        public static string RecoursePath;
        public static string User;
        public static string Assistant;

        public static string ToJson(bool reload = true)
        {
            if (reload)
            {
                Load();
            }
            JSONObject jsonObj = new JSONObject();
            jsonObj.Add("UnityEditorPath", UnityEditorPath);
            jsonObj.Add("RecoursePath", RecoursePath);
            jsonObj.Add("User", User);
            jsonObj.Add("Assistant", Assistant);
            return jsonObj.ToString();
        }

        public static void FromJson(string jsonData, bool save = true)
        {
            JSONObject jsonObj = (JSONObject)JSON.Parse(jsonData);
            UnityEditorPath = jsonObj["UnityEditorPath"];
            RecoursePath = jsonObj["RecoursePath"];
            User = jsonObj["User"];
            Assistant = jsonObj["Assistant"];
            if (save)
            {
                Save();
            }
            Loaded = true;
        }

        public static void Load()
        {
#if UNITY_EDITOR
            UnityEditorPath = UnityEditor.EditorPrefs.GetString(Defs.k_UnityEditorPath, string.Empty);
            RecoursePath = UnityEditor.EditorPrefs.GetString(Defs.k_ResourcePath, string.Empty);
            User = UnityEditor.EditorPrefs.GetString(Defs.k_UserName, "You");
            Assistant = UnityEditor.EditorPrefs.GetString(Defs.k_AssistantName, "Assistant");
            Loaded = true;
#else
            UnityEditorPath = PlayerPrefs.GetString(Defs.k_UnityEditorPath, string.Empty);
            RecoursePath = PlayerPrefs.GetString(Defs.k_ResourcePath, string.Empty);
            User = PlayerPrefs.GetString(Defs.k_UserName, "You");
            Assistant = PlayerPrefs.GetString(Defs.k_AssistantName, "Assistant");
            Loaded = true;
#endif
        }

        public static void Save()
        {
#if UNITY_EDITOR
            UnityEditor.EditorPrefs.SetString(Defs.k_UnityEditorPath, UnityEditorPath);
            UnityEditor.EditorPrefs.SetString(Defs.k_ResourcePath, RecoursePath);
            UnityEditor.EditorPrefs.SetString(Defs.k_UserName, User);
            UnityEditor.EditorPrefs.SetString(Defs.k_AssistantName, Assistant);
#else
            PlayerPrefs.SetString(Defs.k_UnityEditorPath, UnityEditorPath);
            PlayerPrefs.SetString(Defs.k_ResourcePath, RecoursePath);
            PlayerPrefs.SetString(Defs.k_UserName, User);
            PlayerPrefs.SetString(Defs.k_AssistantName, Assistant);
#endif
        }
    }

    public class Communicator
    {
        public float ConnectiontimeOut = 3.0f;
        public readonly string IPAddress = "127.0.0.1";
        public ushort Port = 7778;
        public int SendMessagePoolSize = 32;
        public int ReceiveMessagePoolSize = 256;
        public readonly int MaxMessageLength = 4096;


        public Action<string> OnSystemError;
        public Action<string> OnSentMessage;
        public Action<string> OnReceivedMessage;

        public enum eNetworkPipeline
        {
            None = 0,
            FragmentationStageOnly,
            FragmentationAndReliableStages,
            FragmentationAndSimulatorStages,
            FragmentationAndReliableAndSimulatorStages
        }

        public eNetworkPipeline NetworkPipelineType = eNetworkPipeline.FragmentationAndReliableStages;

        protected NetworkPipeline _networkpipeline;
        protected NetworkSettings _networkSettings;
        protected NetworkDriver _networkDriver;
        protected NativeArray<NetworkConnection> _connections;
        protected JobHandle _jobHandle;


        protected NativeArray<MessageStruct> _sendMessagePool;
        protected int _sendPoolHead = 0;
        protected int _sendPoolTail = 0;
        protected NativeArray<MessageStruct> _receiveMessagePool;
        protected int _receivePoolHead = 0;
        protected int _receivePoolTail = 0;

        protected virtual void Initialize()
        {
            _networkSettings = new NetworkSettings();
            _networkSettings.WithNetworkConfigParameters();         //Use default
            _networkSettings.WithNetworkSimulatorParameters();      //Use default

            _networkDriver = NetworkDriver.Create(_networkSettings);

            switch (NetworkPipelineType)
            {
                case eNetworkPipeline.FragmentationStageOnly:
                    _networkpipeline = _networkDriver.CreatePipeline(typeof(FragmentationPipelineStage));
                    break;
                case eNetworkPipeline.FragmentationAndReliableStages:
                    _networkpipeline = _networkDriver.CreatePipeline(
                        typeof(FragmentationPipelineStage),
                        typeof(ReliableSequencedPipelineStage));
                    break;
                case eNetworkPipeline.FragmentationAndSimulatorStages:
                    _networkpipeline = _networkDriver.CreatePipeline(
                        typeof(FragmentationPipelineStage),
                        typeof(SimulatorPipelineStage));
                    break;
                case eNetworkPipeline.FragmentationAndReliableAndSimulatorStages:
                    _networkpipeline = _networkDriver.CreatePipeline(
                        typeof(FragmentationPipelineStage),
                        typeof(ReliableSequencedPipelineStage),
                        typeof(SimulatorPipelineStage));
                    break;
                default:
                    _networkpipeline = NetworkPipeline.Null;
                    break;
            }

            _connections = new NativeArray<NetworkConnection>(1, Allocator.Persistent);
            _connections[0] = default(NetworkConnection);
            _sendMessagePool = new NativeArray<MessageStruct>(SendMessagePoolSize, Allocator.Persistent);
            _receiveMessagePool = new NativeArray<MessageStruct>(ReceiveMessagePoolSize, Allocator.Persistent);
            _sendPoolHead = _sendPoolTail = _receivePoolHead = _receivePoolTail = 0;
        }

        protected virtual void Dispose()
        {
            if (!IsActive)
            {
                return;
            }

            _jobHandle.Complete();

            _connections.Dispose();
            _sendMessagePool.Dispose();
            _receiveMessagePool.Dispose();
            _sendPoolHead = _sendPoolTail = _receivePoolHead = _receivePoolTail = 0;
            _networkDriver.ScheduleUpdate().Complete();
            _networkDriver.Dispose();
        }

        public bool IsActive => (_networkDriver.IsCreated && _networkDriver.Listening);

        public bool IsSendPoolEmpty => (_sendPoolHead == _sendPoolTail);
        public bool IsReceivePoolEmpty => (_receivePoolHead == _receivePoolTail);
        public bool IsReceivingPoolFull => (
            (_receivePoolTail > 0 && _receivePoolHead == _receivePoolTail - 1)
            || (_receivePoolTail == 0 && _receivePoolHead == _receiveMessagePool.Length - 1));
        public virtual bool IsConnected
        {
            get
            {
                return (_networkDriver.IsCreated &&
                        _connections != null && 
                        _connections.IsCreated && 
                        _connections.Length > 0 && 
                        _connections[0].IsCreated);
            }
        }

        public bool SendMessage(MessageStruct messageData)
        {
            if (_sendPoolTail == 0 && _sendPoolHead == _sendMessagePool.Length - 1 ||
                _sendPoolTail > 0 && _sendPoolHead == _sendPoolTail - 1)
            {
                return false;       //The pool is full
            }

            _sendMessagePool[_sendPoolHead++] = messageData;

            if (_sendPoolHead == _sendMessagePool.Length)
            {
                _sendPoolHead = 0;
            }
            return true;
        }

        public bool SendMessage(string message)
        {
            if(string.IsNullOrEmpty(message))
            {
                return false;
            }

            MessageStruct msg = new MessageStruct
            {
                Type = eMessageType.String4096,
                Message = message
            };
            SendMessage(msg); 

            return true;
        }


        public bool GetReceivedMessage(out MessageStruct messageData)
        {
            if (IsReceivePoolEmpty)
            {
                messageData = default;
                return false;
            }

            messageData = _receiveMessagePool[_receivePoolTail++];
            if (_receivePoolTail == _receiveMessagePool.Length)
            {
                _receivePoolTail = 0;
            }
            return true;
        }

        protected bool PoolReceivedMessage(MessageStruct messageData)
        {
            if (IsReceivingPoolFull)
            {
                OnSystemError?.Invoke("The receiving pool is full!");
                return false;
            }
            _receiveMessagePool[_receivePoolHead++] = messageData;
            if (_receivePoolHead == _receiveMessagePool.Length)
            {
                _receivePoolHead = 0;
            }
            return true;
        }

        protected bool PoolReceivedMessage(MessageStruct messageData, ref NativeArray<MessageStruct> ReceivePool)
        {
            if (IsReceivingPoolFull)
            {
                OnSystemError?.Invoke("The receiving pool is full!");
                return false;
            }
            ReceivePool[_receivePoolHead++] = messageData;
            if (_receivePoolHead == ReceivePool.Length)
            {
                _receivePoolHead = 0;
            }
            return true;
        }

        protected virtual void SendPooledMessege()
        {
            if (_sendPoolHead != _sendPoolTail)
            {
                var sendMessage = _sendMessagePool[_sendPoolTail++];
                if(_sendPoolTail >= SendMessagePoolSize)
                {
                    _sendPoolTail = 0;
                }

                if (sendMessage.Message.Length <= MaxMessageLength)
                {
                    _networkDriver.BeginSend(_networkpipeline, _connections[0], out var writer);
                    writer.WriteInt((int)(sendMessage.Type));
                    writer.WriteFixedString4096(sendMessage.Message);
                    _networkDriver.EndSend(writer);
                    OnSentMessage?.Invoke(sendMessage.Message.ToString());
                }
            }
        }

        protected virtual void SendPooledMessege(ref NativeArray<MessageStruct> SendPool, 
            ref NativeArray<NetworkConnection> Connections, 
            ref NetworkDriver Driver)
        {
            if (_sendPoolHead != _sendPoolTail)
            {
                var sendMessage = SendPool[_sendPoolTail++];
                if (_sendPoolTail >= SendMessagePoolSize)
                {
                    _sendPoolTail = 0;
                }

                if (sendMessage.Message.Length <= MaxMessageLength)
                {
                    Driver.BeginSend(_networkpipeline, Connections[0], out var writer);
                    writer.WriteInt((int)(sendMessage.Type));
                    writer.WriteFixedString4096(sendMessage.Message);
                    Driver.EndSend(writer);
                    OnSentMessage?.Invoke(sendMessage.Message.ToString());
                }
            }
        }

        protected virtual void ProcessPooledReceivedMessage()
        {
            MessageStruct messageData;
            if(GetReceivedMessage(out messageData))
            {
                OnReceivedMessage?.Invoke(messageData.Message.ToString());
            }
        }
    }
}

