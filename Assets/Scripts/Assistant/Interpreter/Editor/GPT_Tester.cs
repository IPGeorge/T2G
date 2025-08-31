using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using System;

#if UNITY_EDITOR

namespace T2G
{

    public class GPT_Tester : EditorWindow
    {
        [MenuItem("T2G/GPT Trainer/Test ...")]
        public static void Test()
        {
            var wnd = GetWindow<GPT_Tester>("Test the GPT Model");
            Vector2 windowSize = new Vector2(800, 400);
            Rect mainRect = EditorGUIUtility.GetMainWindowPosition();
            float x = mainRect.x + (mainRect.width - windowSize.x) * 0.5f;
            float y = mainRect.y + (mainRect.height - windowSize.y) * 0.5f;
            wnd.position = new Rect(x, y, windowSize.x, windowSize.y);
        }

        string _input = string.Empty;
        string _output = string.Empty;
        bool _isQuerying = false;

        private async void OnGUI()
        {
            EditorGUILayout.LabelField("Input:", EditorStyles.boldLabel);
            _input = EditorGUILayout.TextArea(_input, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

            if (_isQuerying)
            {
                GUILayout.Label("Working on it ...");
            }
            else
            {
                if (GUILayout.Button("Get Response", GUILayout.Width(200)))
                {
                    StartQuerying();
                }
            }

            EditorGUILayout.LabelField("Output:", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(_output, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
        }

        void StartQuerying()
        {
            if(_isQuerying)
            {
                return;
            }

            _isQuerying = true;
            Repaint();
            _ = TestAsync();
        }

        async Task TestAsync()
        {
            try
            {
                var response = await GPT_Translation.Test(_input);
                _output = response ?? string.Empty;
                
            }
            catch(Exception _)
            {
                _output = string.Empty;
            }
            finally
            {
                Debug.Log($"Output received: {_output}");
                _isQuerying = false;
                Repaint();
            }
        }
    }



}

#endif