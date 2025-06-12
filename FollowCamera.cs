using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Camera MainCamera; // Assign your VR camera here
    public Vector3 offset; // Set an offset if needed

    void Update()
    {
        if (MainCamera != null)
        {
            transform.position = MainCamera.transform.position + MainCamera.transform.forward * offset.z;
            transform.rotation = MainCamera.transform.rotation;
        }
    }
}
