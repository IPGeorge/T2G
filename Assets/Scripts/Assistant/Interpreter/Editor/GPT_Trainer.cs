using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using System.Threading.Tasks;

#if UNITY_EDITOR

namespace T2G
{

    [Serializable]
    class TrainingData
    {
        public List<string> Inputs = new List<string>();
        public List<string> Outputs = new List<string>();
    }

    public class Edit_Instructions_Data : EditorWindow
    {
        static bool _isTraining = false;
        static int _trainingIndex = -1;
        static int _shouldTrainIndex = -1;

        [MenuItem("T2G/GPT Trainer/Edit Instructions Data")]
        public static void Train()
        {
            var wnd = GetWindow<Edit_Instructions_Data>("Train the GPT Model");
            Vector2 windowSize = new Vector2(800, 600);
            Rect mainRect = EditorGUIUtility.GetMainWindowPosition();
            float x = mainRect.x + (mainRect.width - windowSize.x) * 0.5f;
            float y = mainRect.y + (mainRect.height - windowSize.y) * 0.5f;
            wnd.position = new Rect(x, y, windowSize.x, windowSize.y);
        }

        TrainingData _trainingData = new TrainingData();
        Vector2 scrollPos = Vector2.zero;

        private void Update()
        {
            if (_isTraining && _shouldTrainIndex > _trainingIndex)
            {
                if (_shouldTrainIndex >= _trainingData.Inputs.Count)
                {
                    _isTraining = false;
                    _trainingIndex = _shouldTrainIndex = -1;
                    EditorUtility.DisplayDialog("Train the GPT model", "Training task completed!", "Ok");
                }
                else
                {
                    _trainingIndex = _shouldTrainIndex;
                    StartTraining(_trainingData.Inputs[_trainingIndex], _trainingData.Outputs[_trainingIndex]);
                }
            }
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(500));
            for(int i = 0; i < _trainingData.Inputs.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{i} Input/Output", EditorStyles.boldLabel);
                if (GUILayout.Button("-", GUILayout.Width(30)))
                {
                    _trainingData.Inputs.RemoveAt(i);
                    _trainingData.Outputs.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
                _trainingData.Inputs[i] = EditorGUILayout.TextArea(_trainingData.Inputs[i], GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                _trainingData.Outputs[i] = EditorGUILayout.TextArea(_trainingData.Outputs[i], GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            }
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.BeginHorizontal();
            
            if(GUILayout.Button("+"))
            {
                _trainingData.Inputs.Add(string.Empty);
                _trainingData.Outputs.Add(string.Empty);
            }

            if (GUILayout.Button("Load"))
            {
                if (EditorUtility.DisplayDialog("Load Training Data", "Loading will clear current data, please confirm to preceed.", "Ok", "Cancel"))
                {
                    string path = Path.Combine(Application.persistentDataPath, "GPT_TrainingData.txt");

                    if (File.Exists(path))
                    {
                        string json = File.ReadAllText(path);
                        _trainingData.Inputs.Clear();
                        _trainingData.Outputs.Clear();
                        _trainingData = JsonUtility.FromJson<TrainingData>(json);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Load Training Data", "Data file was not found!", "Ok");
                    }
                }
            }
            if (GUILayout.Button("Save"))
            {
                string path = Path.Combine(Application.persistentDataPath, "GPT_TrainingData.txt");
                string json = JsonUtility.ToJson(_trainingData, true);
                File.WriteAllText(path, json);
                EditorUtility.DisplayDialog("Save Training Data", "Training data was saved!", "Ok");
            }

            if (_isTraining)
            {
                GUILayout.Button("Is Training ...");
            }
            else
            {
                if (GUILayout.Button("Train") && _trainingData.Inputs.Count > 0)
                {
                    _trainingIndex = -1;
                    _shouldTrainIndex = 0;
                    _isTraining = true;
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }

        void StartTraining(string input, string output)
        {
            Debug.Log($"Start training {_trainingIndex} ...");
            Repaint();
            _ = TrainAsync(input, output);
        }

        async Task TrainAsync(string input, string output)
        {
            string response = string.Empty;
            try
            {
                Debug.Log($"Input => {input} \n Output => {output}");
                response = await GPT_Translation.Train(input, output);
            }
            catch (Exception _)
            {
            }
            finally
            {
                Debug.Log($"[Response {_trainingIndex}]: {response}");
                _shouldTrainIndex++;
                Repaint();
            }
        }

    }
}

#endif