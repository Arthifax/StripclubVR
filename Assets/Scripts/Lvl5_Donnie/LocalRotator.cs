using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalRotator : MonoBehaviour
{
    [Range(-0.5f, 0.5f)]
    public float speedX = 0;

    [Range(-0.5f, 0.5f)]
    public float speedY = 0.2f;

    [Range(-0.5f, 0.5f)]
    public float speedZ = 0;

    void Update()
    {
        transform.Rotate(new Vector3(speedX, speedY, speedZ) * 100 * Time.deltaTime, Space.Self);
    }
}
