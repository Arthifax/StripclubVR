using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace KarmaVR
{
    public sealed class AnswerKeyboard : MonoBehaviour
    {
        [Serializable]
        private sealed class Answer
        {
            public string text;
            [Min(1)] public int reward;
        }

        [SerializeField] private MoneyPuzzle puzzle;
        [SerializeField] private TextMeshProUGUI field;
        [SerializeField] private Answer[] answers = new Answer[0];
        [SerializeField] private UnityEvent correct = new UnityEvent();
        [SerializeField] private UnityEvent incorrect = new UnityEvent();
        [SerializeField] private UnityEvent keyPressed = new UnityEvent();

        private readonly HashSet<string> rewarded =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private bool CanType => puzzle != null && puzzle.IsOpen && field != null;

        public void PressKey(string key)
        {
            if (!CanType || string.IsNullOrEmpty(key))
                return;

            field.text += key;
            keyPressed.Invoke();
        }

        public void Backspace()
        {
            if (!CanType || field.text.Length == 0)
                return;

            field.text = field.text.Substring(0, field.text.Length - 1);
            keyPressed.Invoke();
        }

        public void Submit()
        {
            if (!CanType)
                return;

            string input = field.text.Trim();
            field.text = string.Empty;
            if (input.Length == 0)
                return;

            foreach (Answer answer in answers)
            {
                if (answer != null && answer.reward > 0 &&
                    string.Equals(input, answer.text?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    rewarded.Add(input))
                {
                    puzzle.Award(answer.reward);
                    correct.Invoke();
                    return;
                }
            }

            incorrect.Invoke();
        }
    }
}
