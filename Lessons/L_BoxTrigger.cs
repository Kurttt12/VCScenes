using UnityEngine;

public class L_BoxTrigger : MonoBehaviour
{
    public bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("ScaleMarker"))
        {
            isTriggered = true;
            Debug.Log("L_BoxTrigger activated by: " + other.gameObject.name);
            // Optionally notify the parent Evidence (if available)
            Evidence ev = GetComponentInParent<Evidence>();
            if (ev != null)
            {
                // If you have a reference to Task3Manager, you could notify it:
                // task3ManagerInstance.OnMiniTaskTriggered(ev);
            }
        }
    }
}
