using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyObject : MonoBehaviour
{
    public int _worth = 0;

    private MoneyController mc;

    public Vector3 startingPos = Vector3.zero;

    public bool hasUsed = false;
    public bool thrown = false;

    private bool startFading = false;
    private Renderer child = null;

    private void Awake()
    {
        mc = FindObjectOfType<MoneyController>();
        child = transform.GetChild(0).GetComponent<Renderer>();
    }

    private void Update()
    {
        if (startFading)
        {
            child.material.color = new Color(
                child.material.color.r,
                child.material.color.g,
                child.material.color.b,
                child.material.color.a - Time.deltaTime / 6);

            if (child.material.color.a <= 0)
                Destroy(gameObject);
        }

        //if (hasUsed)
        //    return;

        //Vector3 pos = transform.position;

        //if (startingPos == Vector3.zero)
        //{
        //    startingPos = pos;
        //    return;
        //}

        //if (Vector3.Distance(startingPos, pos) > .7f)
        //{
        //    // Give money to lovely lady
        //    mc.GiveMoneyToLovelyLady(_worth);
        //    hasUsed = true;
        //}
        //else if (transform.position.y < -2)
        //{
        //    // Give money back to player
        //    mc.PutMoneyBackOnPile(_worth);
        //    hasUsed = true;
        //}
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            if (!startFading)
                startFading = true;

            GetComponent<Rigidbody>().mass = 1;
            Destroy(this.GetComponent<FeatherFall>());
        }
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (!thrown)
            return;

        if (Vector3.Distance(startingPos, transform.position) > .2f)
            return;

        if (coll.tag == "PutMoneyBack")
        {
            mc.PutMoneyBackOnPile(_worth);
            Destroy(this.gameObject);
        }
    }
}
