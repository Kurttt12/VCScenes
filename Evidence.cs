using System.Collections.Generic;
using UnityEngine;

public class Evidence : MonoBehaviour
{
    public bool isCaptured = false;
    
    // Mini tasks: e.g. scale marker placements (using BoxTrigger components)
    public List<BoxTrigger> miniTaskTriggers;
    
    // Final capture detector (attached to a BoxCollider that is NOT set as a trigger)
    public FinalCaptureDetector finalCaptureDetector;
    
    public void Capture()
    {
        isCaptured = true;
        Debug.Log("Evidence captured: " + gameObject.name);
    }
}
