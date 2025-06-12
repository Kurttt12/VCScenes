using System.Collections.Generic;
using UnityEngine;

public class L_Evidence : MonoBehaviour
{
    public bool isCaptured = false;
    
    // Mini tasks: e.g. scale marker placements (using BoxTrigger components)
    public List<L_BoxTrigger> miniTaskTriggers;
    
    // Final capture detector (attached to a BoxCollider that is NOT set as a trigger)
    public L_FinalCaptureDetector finalCaptureDetector;
    
    public void Capture()
    {
        isCaptured = true;
        Debug.Log("L_Evidence captured: " + gameObject.name);
    }
}
