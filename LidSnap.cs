using UnityEngine;

public class LidSnap : MonoBehaviour
{
    public Transform attachPointContainer; // AttachPoint on the container
    public float snapThreshold = 0.1f; // Distance threshold for snapping
    public float snapSpeed = 5f; // How fast it snaps

    private Transform lidTransform;
    private bool isSnapped = false;

    void Start()
    {
        lidTransform = transform;
    }

    void Update()
    {
        if (!isSnapped && Vector3.Distance(lidTransform.position, attachPointContainer.position) < snapThreshold)
        {
            SnapLid();
        }
    }

    void SnapLid()
    {
        lidTransform.position = Vector3.Lerp(lidTransform.position, attachPointContainer.position, Time.deltaTime * snapSpeed);
        lidTransform.rotation = Quaternion.Lerp(lidTransform.rotation, attachPointContainer.rotation, Time.deltaTime * snapSpeed);
        isSnapped = true;
    }

    public void ReleaseLid()
    {
        isSnapped = false;
    }
}
