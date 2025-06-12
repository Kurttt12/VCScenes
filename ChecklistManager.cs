using UnityEngine;
using TMPro;

public class ChecklistManager : MonoBehaviour
{
    public TMP_Text[] checklistItems; // Array of TMP_Text elements representing checklist items
    private bool[] itemsCaptured; // Array to track which items have been captured

    void Start()
    {
        // Initialize the captured items array
        itemsCaptured = new bool[checklistItems.Length];
    }

    // Method to mark an item as captured
    public void MarkItemAsCaptured(int itemIndex)
    {
        if (itemIndex >= 0 && itemIndex < itemsCaptured.Length)
        {
            itemsCaptured[itemIndex] = true;
            UpdateChecklistUI();
        }
    }

    // Update the checklist UI to reflect the captured items
    private void UpdateChecklistUI()
    {
        for (int i = 0; i < checklistItems.Length; i++)
        {
            if (itemsCaptured[i])
            {
                checklistItems[i].text = checklistItems[i].text + " Captured";
                checklistItems[i].color = Color.green; // Change color to indicate completion
            }
        }
    }
}
