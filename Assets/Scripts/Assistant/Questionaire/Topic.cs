

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace T2G
{
    public abstract class Topic
    {
        public string Title;
        public List<(string question, string defaultAnswer, bool includeInAnswer)> Questions = new List<(string, string, bool)>();
        public List<string> Answers = new List<string>();
        public string ResponseMessage;

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
            if (CurrentQuestionIndex >= 0 && CurrentQuestionIndex < Questions.Count)
            {
                return Questions[CurrentQuestionIndex].question;
            }
            return null;
        }

        public int AnswerQuestion(string answer)    //return -1: end, >= 0: next question index.
        {
            if (CurrentQuestionIndex < 0 || CurrentQuestionIndex >= Questions.Count)
            {
                CurrentQuestionIndex = -1;
                return -1;
            }

            string answerInLower = answer.ToLower();

            if (string.Compare(answerInLower, "cancel") == 0)
            {
                ConsoleController.Instance.WriteConsoleMessage(ConsoleController.eSender.Assistant, "Canceled!");
                CurrentQuestionIndex = -1;
                return -1;
            }

            var currentQuestion = Questions[CurrentQuestionIndex];

            string answerPrompt = currentQuestion.includeInAnswer ? $"{currentQuestion.question}: " : string.Empty;
            if (string.Compare(answerInLower, "skip") == 0 || string.IsNullOrWhiteSpace(answer))
            {
                answerPrompt += currentQuestion.defaultAnswer;
                ConsoleController.Instance.WriteConsoleMessage(ConsoleController.eSender.Assistant, $"Your answer: {currentQuestion.defaultAnswer}");
            }
            else
            {
                answerPrompt += answer;
                ConsoleController.Instance.WriteConsoleMessage(ConsoleController.eSender.Assistant, $"Your answer: {answer}");
            }

            Answers.Add(answerPrompt);

            CurrentQuestionIndex++;
            if (CurrentQuestionIndex >= Questions.Count)
            {
                CurrentQuestionIndex = -1;
            }

            return CurrentQuestionIndex;
        }

        public bool TopicIsOver()
        {
            return (Answers.Count == Questions.Count || CurrentQuestionIndex < 0);
        }
    }
}