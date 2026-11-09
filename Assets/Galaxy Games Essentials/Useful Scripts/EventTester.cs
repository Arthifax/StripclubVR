using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventTester : MonoBehaviour
{
    public bool TestEvent = false;
    [Space]
    public UnityEvent MyTestEvent;

    void Update()
    {
        if (TestEvent)
        {
            TestEvent = false;
            MyTestEvent.Invoke();
        }
    }
}
