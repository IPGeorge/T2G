

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace T2G
{
    public abstract class Topic
    {
        public string Title { get; protected set; }
        protected List<(string question, string defaultAnswer, bool includeInAnswer)> _questions = new List<(string, string, bool)>();
        protected List<string> _answers = new List<string>();
        public string AnswersSummary { get; protected set; } = string.Empty;

        protected Instruction _instruction = null;
        public Instruction Instruction => _instruction;
        public Topic() 
        {
            _instruction = null;
        }

        public Topic(Instruction instruction)
        {
            _instruction = instruction;
        }

        public virtual async Awaitable PostTopicProcess(string prompt) { await Task.Yield(); }

        public int CurrentQuestionIndex { get; private set; }

        public string GetCurrentQuestion()
        {
            if (CurrentQuestionIndex >= 0 && CurrentQuestionIndex < _questions.Count)
            {
                return _questions[CurrentQuestionIndex].question;
            }
            return null;
        }

        public bool AnswerQuestion(string answer)
        {
            AnswersSummary = string.Empty;
            if (CurrentQuestionIndex < 0 || CurrentQuestionIndex >= _questions.Count)
            {
                CurrentQuestionIndex = -1;
                return false;
            }

            string answerInLower = answer.ToLower();

            if (string.Compare(answerInLower, "cancel") == 0)
            {
                ConsoleController.Instance.WriteConsoleMessage(ConsoleController.eSender.Assistant, "Canceled!");
                CurrentQuestionIndex = -1;
                return false;
            }

            var currentQuestion = _questions[CurrentQuestionIndex];

            string resolvedAnswer = currentQuestion.includeInAnswer ? $"{currentQuestion.question}: " : string.Empty;
            if (string.Compare(answerInLower, "skip") == 0 || string.IsNullOrWhiteSpace(answer))
            {
                resolvedAnswer += currentQuestion.defaultAnswer;
                ConsoleController.Instance.WriteConsoleMessage(ConsoleController.eSender.Assistant, $"Your answer: {currentQuestion.defaultAnswer}");
            }
            else
            {
                resolvedAnswer += answer;
                ConsoleController.Instance.WriteConsoleMessage(ConsoleController.eSender.Assistant, $"Your answer: {answer}");
            }

            _answers.Add(resolvedAnswer);

            CurrentQuestionIndex++;
            if (CurrentQuestionIndex >= _questions.Count)
            {
                foreach (var promptAnswer in _answers)
                {
                    AnswersSummary += promptAnswer + "\n";
                }
                CurrentQuestionIndex = -1;
            }

            return true;
        }

        public bool TopicIsOver()
        {
            return (_answers.Count == _questions.Count || CurrentQuestionIndex < 0);
        }
    }
}