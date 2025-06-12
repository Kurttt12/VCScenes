using UnityEngine;

public class ReattachToolsToPlayer : MonoBehaviour
{
    [Tooltip("The attach point for the camera tool (should be a child of the XR rig).")]
    public Transform cameraAttachPoint;
    [Tooltip("The camera tool GameObject.")]
    public GameObject cameraTool;
    
    [Tooltip("The attach point for the scale marker tool (should be a child of the XR rig).")]
    public Transform scaleMarkerAttachPoint;
    [Tooltip("The scale marker tool GameObject.")]
    public GameObject scaleMarkerTool;

    public void ReattachTools()
    {
        ReattachTool(cameraTool, cameraAttachPoint, "Camera tool");
        ReattachTool(scaleMarkerTool, scaleMarkerAttachPoint, "Scale marker tool");
    }

    /// <summary>
    /// Reattaches a tool to an attach point and restores its local scale so that its world scale
    /// remains as set in the prefab.
    /// </summary>
    /// <param name="tool">The tool GameObject.</param>
    /// <param name="attachPoint">The attach point Transform.</param>
    /// <param name="toolName">A descriptive name for debug logging.</param>
    private void ReattachTool(GameObject tool, Transform attachPoint, string toolName)
    {
        if (tool != null && attachPoint != null)
        {
            // Store the tool's current world scale.
            Vector3 originalWorldScale = tool.transform.lossyScale;
            // Reparent without preserving world position, rotation or scale.
            tool.transform.SetParent(attachPoint, false);
            // Reset local position and rotation.
            tool.transform.localPosition = Vector3.zero;
            tool.transform.localRotation = Quaternion.identity;
            // Calculate the proper local scale required to preserve the original world scale.
            Vector3 newLocalScale = new Vector3(
                originalWorldScale.x / attachPoint.lossyScale.x,
                originalWorldScale.y / attachPoint.lossyScale.y,
                originalWorldScale.z / attachPoint.lossyScale.z);
            tool.transform.localScale = newLocalScale;
            Debug.Log($"[ReattachToolsToPlayer] {toolName} reattached. New local scale: {newLocalScale}");
        }
        else
        {
            Debug.LogWarning($"[ReattachToolsToPlayer] Missing {toolName} or attach point.");
        }
    }
}
