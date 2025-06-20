using System;
using System.Collections.Generic;
using UnityEngine;

public class ContextGazeInteraction : MonoBehaviour
{
    private EyeTracker eyeTracker;
    private void Start()
    {
        eyeTracker = GetComponent<EyeTracker>();
    }

    public KeyboardContext GetCurrentContext(List<KeyboardContext> keyboardContexts, out Vector3 gazeInContext)
    {
        gazeInContext = Vector3.zero;
        if (keyboardContexts == null || keyboardContexts.Count == 0)
            return null;

        GazePoint gaze = eyeTracker.CurrentGazePoint();
        Vector3 cameraPos = Camera.main.transform.position;
        KeyboardContext bestContext = GetClosestContext(keyboardContexts, cameraPos, gaze.GazeDepth);
        if (bestContext == null)
        {
            return null;
        }
        Plane contextPlane = bestContext.TargetPlane;
        if (!contextPlane.Raycast(gaze.CyclopsRay, out float gazeInPlaneEnter))
        {
            return null;
        }
        gazeInContext = gaze.CyclopsRay.GetPoint(gazeInPlaneEnter);

        return bestContext;
    }

    /// <summary>
    /// Finds the closest keyboard context based on the camera position and estimated gaze depth.
    /// </summary>
    /// <param name="keyboardContexts"></param>
    /// <param name="cameraPos"></param>
    /// <param name="gazeDepth"></param>
    /// <returns></returns>
    private static KeyboardContext GetClosestContext(List<KeyboardContext> keyboardContexts, Vector3 cameraPos, float gazeDepth)
    {
        KeyboardContext bestContext = null;
        float minDepthDiff = float.MaxValue;
        foreach (var context in keyboardContexts)
        {
            if (context == null || !context.State.IsActive())
                continue;

            Plane contextPlane = context.TargetPlane;
            // Get distance from camera position
            float distance = Vector3.Dot(contextPlane.normal, cameraPos - contextPlane.ClosestPointOnPlane(cameraPos));
            float depthDiff = Mathf.Abs(distance - gazeDepth);
            if (depthDiff < minDepthDiff)
            {
                minDepthDiff = depthDiff;
                bestContext = context;
            }
        }

        return bestContext;
    }
}