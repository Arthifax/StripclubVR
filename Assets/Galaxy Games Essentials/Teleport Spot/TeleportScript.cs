using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class TeleportScript : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject rayOrigin;
    private bool clicked = false;
    private bool allowTeleport = true;
    private float yHeightAdded = 1.7f;
    public bool tpWithHeight = false;
    public string tagName = "Teleporter";
    public void AllowTeleport()
    {
        allowTeleport = false;
    }

    void Update()
    {
        RaycastHit hit;

        var rightHandDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandDevices);

        if (rightHandDevices.Count == 1)
        {
            InputDevice device = rightHandDevices[0];
            bool triggerButtonValue = false;
            if (device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerButtonValue) && triggerButtonValue)
            {
                if (!clicked)
                {
                    if (Physics.Raycast(rayOrigin.transform.position, rayOrigin.transform.forward, out hit))
                    {
                        if (allowTeleport && hit.transform.tag == tagName)
                        {
                            GameObject objectHit = hit.transform.gameObject;
                            if (tpWithHeight)
                            {
                                player.transform.position = new Vector3(objectHit.transform.position.x, objectHit.transform.position.y + yHeightAdded, objectHit.transform.position.z);
                            }
                            else
                            {
                                player.transform.position = new Vector3(objectHit.transform.position.x, player.transform.position.y, objectHit.transform.position.z);
                            }
                        }
                    }
                    clicked = true;
                }
            }

            if (device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerButtonValue) && !triggerButtonValue)
            {
                clicked = false;
            }
        }
    }
}
