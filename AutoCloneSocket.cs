using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class AutoCloneSocket : MonoBehaviour
{
    // Reference to the XR Socket Interactor (can be assigned via Inspector)
    public XRSocketInteractor socketInteractor;
    // The prefab to clone when an object is removed
    public GameObject objectPrefab;

    private void Awake()
    {
        // If not assigned in the Inspector, try to get the component on the same GameObject.
        if (socketInteractor == null)
            socketInteractor = GetComponent<XRSocketInteractor>();

        // Listen for when an object is removed (grabbed) from the socket.
        socketInteractor.selectExited.AddListener(OnSelectExited);
    }

    private void OnDestroy()
    {
        socketInteractor.selectExited.RemoveListener(OnSelectExited);
    }

    // Called when an object is removed from the socket
    private void OnSelectExited(SelectExitEventArgs args)
    {
        // Ensure we have a prefab assigned
        if (objectPrefab != null)
        {
            // Instantiate a new object at the socket's position and rotation
            Instantiate(objectPrefab, socketInteractor.transform.position, socketInteractor.transform.rotation);
        }
        else
        {
            Debug.LogWarning("No prefab assigned to clone!");
        }
    }
}
