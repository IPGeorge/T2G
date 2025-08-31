using System.Collections.Generic;
using UnityEngine;


namespace T2G
{
    [ExecuteAlways]
    public class TextPrinter : MonoBehaviour
    {
        static TextPrinter _instance = null;
        public static TextPrinter Instance
        {
            get
            {
                if (_instance == null)
                {
                    var textPrinter = GameObject.FindFirstObjectByType<TextPrinter>();
                    if(textPrinter == null)
                    {
                        var gameObj = new GameObject("TextPrinter");
                        textPrinter = gameObj.AddComponent<TextPrinter>();
                    }
                    _instance = textPrinter;
                }
                return _instance;
            }
        }

        [SerializeField] List<string> Texts = new List<string>();
        [SerializeField] List<GUIStyle> GUIStyles = new List<GUIStyle>();
        [SerializeField] List<Rect> Rects = new List<Rect>();

        GUIStyle _guiStyle = new GUIStyle();
        Vector2 _margin = Vector2.zero;
        Dictionary<string, (Rect, GUIStyle)> _screenTexts = new Dictionary<string, (Rect, GUIStyle)>();


        void UpdateSerialized(string text, GUIStyle style, Rect rect)
        {
            if (_screenTexts.ContainsKey(text))
            {
                for (int i = 0; i < Texts.Count; ++i)
                {
                    if (string.Compare(Texts[i], text) == 0)
                    {
                        Texts.RemoveAt(i);
                        GUIStyles.RemoveAt(i);
                        Rects.RemoveAt(i);
                    }
                }
            }

            Texts.Add(text);
            GUIStyles.Add(style);
            Rects.Add(rect);
        }

        private void Start()
        {
            _screenTexts.Clear();
            for (int i = 0; i < Texts.Count; ++i)
            {
                _screenTexts[Texts[i]] = (Rects[i], GUIStyles[i]);
            }
        }

        public void PrintText(string text, int x, int y, GUIStyle guiStyle = null)
        {
            var style = guiStyle ?? new GUIStyle(_guiStyle);
            style.alignment = TextAnchor.UpperLeft;
            var size = style.CalcSize(new GUIContent(text));
            Rect rect = new Rect(x, y, size.x, size.y);
            UpdateSerialized(text, style, rect);
            _screenTexts[text] = (rect, style);
        }

        public void PrintText(string text, TextAnchor aligement, GUIStyle guiStyle = null)
        {
            var style = guiStyle ?? new GUIStyle(_guiStyle);
            style.alignment = aligement;
            Rect rect = new Rect(_margin.x, _margin.y, Screen.width - _margin.x * 2, Screen.height - _margin.y * 2);
            UpdateSerialized(text, style, rect);
            _screenTexts[text] = (rect, style);
        }

        public void PrintText(string text, string aligement, GUIStyle guiStyle = null)
        {
            TextAnchor anchor = TextAnchor.MiddleCenter;
            switch(aligement)
            {
                case "top-left":
                case "upper-left":
                    anchor = TextAnchor.UpperLeft;
                    break;
                case "top-middle":
                case "upper-center":
                    anchor = TextAnchor.UpperCenter;
                    break;
                case "top-right":
                case "upper-right":
                    anchor = TextAnchor.UpperRight;
                    break;
                case "middle-left":
                    anchor = TextAnchor.MiddleLeft;
                    break;
                case "center":
                    anchor = TextAnchor.MiddleCenter;
                    break;
                case "middle-right":
                    anchor = TextAnchor.MiddleRight;
                    break;
                case "bottom-left":
                case "lower-left":
                    anchor = TextAnchor.LowerLeft;
                    break;
                case "bottom-middle":
                case "lower-center":
                    anchor = TextAnchor.LowerCenter;
                    break;
                case "bottom-right":
                case "lower-right":
                    anchor = TextAnchor.LowerRight;
                    break;
            }
            PrintText(text, anchor, guiStyle);
        }

        public void RemoveText(string text)
        {
            if (_screenTexts.ContainsKey(text))
            {
                _screenTexts.Remove(text);
            }
        }

        public void SetTextAttributes(string attrib, object value)
        {
            switch (attrib)
            {
                case "size":
                    _guiStyle.fontSize = (int)value;
                    break;
                case "color":
                    if (ColorUtility.TryParseHtmlString((string)value, out var color))
                    {
                        _guiStyle.normal.textColor = color; 
                    }
                    break;
                case "bold":
                    if ((bool)value)
                    {
                        _guiStyle.fontStyle |= FontStyle.Bold;
                    }
                    else
                    {
                        _guiStyle.fontStyle &= ~FontStyle.Bold;
                    }
                    break;
                case "italic":
                    if ((bool)value)
                    {
                        _guiStyle.fontStyle |= FontStyle.Italic;
                    }
                    else
                    {
                        _guiStyle.fontStyle &= ~FontStyle.Italic;
                    }
                    break;
            }
        }

        private void OnGUI()
        {
            foreach (var screenText in _screenTexts)
            {
                GUI.Label(screenText.Value.Item1, screenText.Key, screenText.Value.Item2);
            }
        }
    }
}