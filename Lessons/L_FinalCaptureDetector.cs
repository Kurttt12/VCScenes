using UnityEngine;

public class L_FinalCaptureDetector : MonoBehaviour
{
    public bool isFinalCaptured = false;
    
    // This method can be called by your camera or another script when the final capture is valid.
    public void SetFinalCaptured()
    {
        isFinalCaptured = true;
        Debug.Log("Final capture detected on: " + gameObject.name);
    }
}
