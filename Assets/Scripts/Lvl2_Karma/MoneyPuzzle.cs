using System;
using UnityEngine;

namespace KarmaVR
{
    public sealed class MoneyPuzzle : MonoBehaviour
    {
        [SerializeField, Min(0)] private int startingBalance = 1000;

        public int Balance { get; private set; }
        public int Payment { get; private set; }
        public bool IsOpen { get; private set; }
        public event Action Changed;

        private void Awake() => Balance = startingBalance;

        public void SetOpen(bool open)
        {
            IsOpen = open;
            Changed?.Invoke();
        }

        public bool CanAdd(int value) => IsOpen && value > 0 && Balance >= value;

        public bool TryAddToPayment(int value)
        {
            if (!CanAdd(value))
                return false;

            Balance -= value;
            Payment += value;
            Changed?.Invoke();
            return true;
        }

        public void ClearPayment()
        {
            if (IsOpen && Payment > 0)
                RefundPayment();
        }

        public void Award(int amount)
        {
            if (!IsOpen || amount <= 0)
                return;

            Balance += amount;
            Changed?.Invoke();
        }

        public void RefundPayment()
        {
            Balance += Payment;
            SpendPayment();
        }

        public void SpendPayment()
        {
            Payment = 0;
            Changed?.Invoke();
        }
    }
}
