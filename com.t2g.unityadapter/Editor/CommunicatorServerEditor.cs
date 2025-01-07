//#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;

namespace T2G.UnityAdapter
{
    [InitializeOnLoad]
    public class LauchEditorStartUp
    {
        static LauchEditorStartUp()
        {
            bool dashboardIsOpen = SessionState.GetBool(Defs.k_DashboardIsOpen, true);
            if (!dashboardIsOpen)
            {
                EditorApplication.quitting += EditorApplication_quitting;
                CommunicatorServerEditor.OpenDashboard();
            }
        }

        private static void EditorApplication_quitting()
        {
            CommunicatorServerEditor.CloseDashboard();
            EditorApplication.quitting -= EditorApplication_quitting;
        }
    }

    public class CommunicatorServerEditor : EditorWindow
    {
        static CommunicatorServer _server;
        static Vector2 _scroll = Vector2.zero;
        static string _text = string.Empty;
        static bool _repaintText = false;
        static CommunicatorServerEditor _CommunicatorWindow = null;


        [MenuItem("T2G/Communicator", false)]
        public static void OpenDashboard()
        {
            Type inspectorType = Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll");
            _CommunicatorWindow = EditorWindow.GetWindow<CommunicatorServerEditor>("Communicator Server", new Type[] { inspectorType });
            SessionState.SetBool(Defs.k_DashboardIsOpen, true);
        }

        public static void CloseDashboard()
        {
            CommunicatorServerEditor.Uninitialize();

            if (_CommunicatorWindow != null)
            {
                _CommunicatorWindow.Close();
            }
            SessionState.SetBool(Defs.k_DashboardIsOpen, false);
        }


        [InitializeOnLoadMethod]
        public static void InitializeOnLoad()
        {
            CommunicatorServer communicatorServer = CommunicatorServer.Instance;

            communicatorServer.OnServerStarted += () =>
            {
                AddConsoleText("\n System> Server started.");
            };

            communicatorServer.AfterShutdownServer += () =>
            {
                AddConsoleText("\n System> Server was shut down.");
            };

            communicatorServer.OnFailedToStartServer += () => 
            {
                AddConsoleText("\n System> Failed to start litsening server!");
            };

            communicatorServer.OnClientConnected += () =>
            {
                AddConsoleText("\n System> Client was connected!");
            };

            communicatorServer.OnClientDisconnected += () =>
            {
                AddConsoleText("\n System> Client was disconnected!");
            };

            communicatorServer.OnReceivedMessage += (message) =>
            {
                AddConsoleText("\n Received> " + message);
            };

            communicatorServer.OnSentMessage += (message) =>
            {
                AddConsoleText("\n Sent> " + message);
            };

            communicatorServer.OnLogMessage += (message) =>
            {
                AddConsoleText("\n Received> " + message);
            };

            if (_server == null)
            {
                _server = CommunicatorServer.Instance;
                AssemblyReloadEvents.beforeAssemblyReload += () =>
                {
                    _server.StopServer();
                };
            }

#if T2G
            bool startServer = EditorPrefs.GetBool(Defs.k_StartListener, false);
            if (startServer)
            {
                _text = string.Empty;
                _server.StartServer();
            }
#else
            _text = string.Empty;
            _server.StartServer();
#endif
        }

        private void Update()
        {
            RepaintText();
        }

        void RepaintText()
        {
            if (_repaintText)
            {
                if (_CommunicatorWindow == null)
                {
                    _CommunicatorWindow = GetWindow<CommunicatorServerEditor>();
                }
                _CommunicatorWindow?.Repaint();
            }
        }

        static void AddConsoleText(string textToAdd)
        {
            _text += textToAdd;
            _repaintText = true;
        }

        public static void Uninitialize()
        {
            if(_server != null)
            {
                _server.StopServer();
            }

            CommunicatorServer communicatorServer = CommunicatorServer.Instance;
            communicatorServer.OnServerStarted = null;
            communicatorServer.AfterShutdownServer = null;
            communicatorServer.OnFailedToStartServer = null;
            communicatorServer.OnClientConnected = null;
            communicatorServer.OnClientDisconnected = null;
            communicatorServer.OnReceivedMessage = null;
            communicatorServer.OnSentMessage = null;
            communicatorServer.OnLogMessage = null;
            communicatorServer = null;
        }

        public void OnGUI()
        {
            if(_server == null)
            {
                _server = CommunicatorServer.Instance;
                AssemblyReloadEvents.beforeAssemblyReload += () => {
                    _server.StopServer();
                };
            }

            bool isActive = EditorGUILayout.Toggle("Server is on: ", _server.IsActive);
            if(isActive != _server.IsActive)
            {
                if (isActive)
                {
                    _server.StartServer();
                }
                else
                {
                    _server.StopServer();
                }
                EditorPrefs.SetBool(Defs.k_StartListener, isActive);
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Toggle("Client is connected: ", _server.IsConnected);
            EditorGUI.EndDisabledGroup();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            _text = EditorGUILayout.TextArea(_text, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        public static CommunicatorServer GetServer()
        {
            return _server;
        }
    }
}

//#endif