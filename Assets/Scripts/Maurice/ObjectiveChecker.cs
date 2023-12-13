using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class ObjectiveChecker : MonoBehaviour
{
    [SerializeField] private int objectiveAmount;
    [SerializeField] private int objectivesCompleted = 0;
    [SerializeField] private UnityEvent CompleteLevel;

    public void CompleteObjective()
    {
        objectivesCompleted++;

        if(objectivesCompleted == objectiveAmount)
        {
            Debug.Log("Game is completed");
            CompleteLevel.Invoke();
        }
    }
}
