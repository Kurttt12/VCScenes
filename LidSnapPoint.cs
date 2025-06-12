using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LidSnapPoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Lid"))
        {
            other.transform.position = transform.position;
            other.transform.rotation = transform.rotation;
            other.GetComponent<Rigidbody>().isKinematic = true;
        }
    }
}
