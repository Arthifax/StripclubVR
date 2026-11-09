using UnityEngine;
using UnityEngine.Events;

namespace KarmaVR
{
    public sealed class MoneyPile : MonoBehaviour
    {
        [SerializeField] private MoneyPuzzle puzzle;
        [SerializeField, Min(1)] private int denomination = 50;
        [SerializeField] private MoneyBillVisual billPrefab;
        [SerializeField] private Transform billSpawnPoint;
        [SerializeField] private Transform moneyTarget;
        [SerializeField] private UnityEvent moneyAdded = new UnityEvent();
        [SerializeField] private UnityEvent insufficientFunds = new UnityEvent();

        // Wire XR Simple Interactable > Select Entered to this method.
        public void AddToPayment()
        {
            if (!isActiveAndEnabled || puzzle == null || !puzzle.IsOpen)
                return;

            if (puzzle.TryAddToPayment(denomination))
            {
                SpawnBill();
                moneyAdded.Invoke();
            }
            else
                insufficientFunds.Invoke();
        }

        private void SpawnBill()
        {
            if (billPrefab == null || moneyTarget == null)
                return;

            Transform origin = billSpawnPoint != null ? billSpawnPoint : transform;
            MoneyBillVisual bill = Instantiate(billPrefab, origin.position, origin.rotation);
            bill.FlyTo(moneyTarget.position);
        }
    }
}
