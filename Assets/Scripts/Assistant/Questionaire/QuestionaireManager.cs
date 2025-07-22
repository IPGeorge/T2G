using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace T2G
{
    public class QuestionaireManager : MonoBehaviour
    {
        static readonly string[] s_Hints = {
            "Input 'cancel' to cancel this operation; To skip the question, input 'skip' or nothing."
        };

        static public QuestionaireManager Instance { get; private set; } = null;

        Stack<Topic> _topicsStack = new Stack<Topic>();
        Topic _currentTopic = null;

        public bool IsActive
        {
            get
            {
                return (_currentTopic != null);
            }
        }

        ConsoleController _console;

        void Start()
        {
            Instance = this;
            _console = ConsoleController.Instance;
        }

        public bool StartANewTopic(Topic topic)
        {
            if (topic == null)
            {
                return false;
            }

            if (_console == null)
            {
                _console = ConsoleController.Instance;
            }

            _topicsStack.Push(topic);
            _currentTopic = topic;
            ShowTitleHint();
            ShowCurrentQuestion();
            return true;
        }

        void ShowTitleHint()
        {
            ConsoleController.Instance.WriteConsoleMessage(ConsoleController.eSender.Assistant, _currentTopic.Title);
            for (int i = 0; i < s_Hints.Length; ++i)
            {
                ConsoleController.Instance.WriteConsoleMessage(ConsoleController.eSender.Assistant, s_Hints[i]);
            }
        }

        public bool ShowCurrentQuestion()
        {
            if (_currentTopic == null)
            {
                return false;
            }

            var currentQuestion = _currentTopic.GetCurrentQuestion();
            if (!string.IsNullOrWhiteSpace(currentQuestion))
            {
                ConsoleController.Instance.WriteConsoleMessage(ConsoleController.eSender.Assistant, currentQuestion);
            }
            return true;
        }


        public Topic AnswerQuestion(string answer, out string prompt)
        {
            prompt = string.Empty;
            if (_currentTopic == null)
            {
                return null;        //No topic is available
            }

            Topic topic = _currentTopic;
            if(_currentTopic.AnswerQuestion(answer) < 0)
            {
                if (_topicsStack.Count > 0)
                {
                    foreach(var answerPrompt in _currentTopic.Answers)
                    {
                        prompt += answerPrompt + "\n";
                    }
                    _currentTopic = _topicsStack.Pop();
                }
                else
                {
                    _currentTopic = null;
                }
            }

            return topic;          //Continue with the next question
        }
    }
}