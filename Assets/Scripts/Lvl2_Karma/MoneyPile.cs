using UnityEngine;
using UnityEngine.Events;

namespace KarmaVR
{
    public sealed class MoneyPile : MonoBehaviour
    {
        [SerializeField] private MoneyPuzzle puzzle;
        [SerializeField, Min(1)] private int denomination = 50;
        [SerializeField] private UnityEvent moneyAdded = new UnityEvent();
        [SerializeField] private UnityEvent insufficientFunds = new UnityEvent();

        // Wire XR Simple Interactable > Select Entered to this method.
        public void AddToPayment()
        {
            if (!isActiveAndEnabled || puzzle == null || !puzzle.IsOpen)
                return;

            if (puzzle.TryAddToPayment(denomination))
                moneyAdded.Invoke();
            else
                insufficientFunds.Invoke();
        }
    }
}
