using UnityEngine;
using UnityEngine.InputSystem;

public class ChecklistUI : MonoBehaviour
{
    public InputActionAsset inputActions;

    private Canvas _checklistCanvas;
    private InputAction _leftHandChecklist;
    private InputAction _rightHandChecklist;

    private void Start()
    {
        _checklistCanvas = GetComponent<Canvas>();

        // Start with the checklist hidden
        _checklistCanvas.enabled = false;

        // Get the LeftHand action for the checklist
        _leftHandChecklist = inputActions.FindActionMap("XRI LeftHand").FindAction("Checklist");
        _leftHandChecklist.Enable();
        _leftHandChecklist.performed += ToggleChecklist;

        // Get the RightHand action for the checklist
        _rightHandChecklist = inputActions.FindActionMap("XRI RightHand").FindAction("Checklist");
        _rightHandChecklist.Enable();
        _rightHandChecklist.performed += ToggleChecklist;
    }

    private void OnDestroy()
    {
        _leftHandChecklist.performed -= ToggleChecklist;
        _rightHandChecklist.performed -= ToggleChecklist;
    }

    public void ToggleChecklist(InputAction.CallbackContext context)
    {
        // Toggle the checklist visibility
        _checklistCanvas.enabled = !_checklistCanvas.enabled;
    }
}

/*    incase of bug on destroy
    private void OnDestroy()
    {
        // Unsubscribe from the input action events when this object is destroyed
        if (_leftHandChecklist != null) 
            _leftHandChecklist.performed -= ToggleChecklist;
        if (_rightHandChecklist != null) 
            _rightHandChecklist.performed -= ToggleChecklist;
    }
*/