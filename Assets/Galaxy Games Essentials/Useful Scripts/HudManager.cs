using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HudManager : MonoBehaviour
{
    [SerializeField] Camera playerCam;

    void Update()
    {
        transform.position = playerCam.transform.position;
        transform.rotation = Quaternion.Euler(0, playerCam.transform.rotation.eulerAngles.y, 0);
    }
}