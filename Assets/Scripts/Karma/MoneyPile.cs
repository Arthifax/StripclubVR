using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyPile : MonoBehaviour
{
    [SerializeField] private GameObject _moneyObject = null;
    [SerializeField] private int _moneyWorth = 0;
    [SerializeField] private Transform _target = null;

    private MoneyController mc = null;

    private void Awake()
    {
        mc = FindObjectOfType<MoneyController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            SpawnMoney();
    }

    public GameObject SpawnMoney()
    {
        GameObject money = Instantiate(_moneyObject, transform.position, transform.rotation);
        money.GetComponent<Rigidbody>().AddForce((_target.position - transform.position) * 8);
        mc.GrabMoneyFromPile(_moneyWorth);
        mc.GiveMoneyToLovelyLady(_moneyWorth);
        mc.CheckMoneyPiles();
        return money;
    }

    public int GetWorth() { return _moneyWorth; }
}
