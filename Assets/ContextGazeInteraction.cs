using System;
using System.Collections.Generic;
using UnityEngine;

public interface IGazeDataListener
{
    void OnGaze(Vector2 gaze2D, Vector3 gaze3D, Vector3 leftEyePosition, Vector3 rightEyePosition, Vector3 leftEyeDirection, Vector3 rightEyeDirection);
}

public class ContextGazeInteraction : MonoBehaviour
{
    private List<IGazeDataListener> gazeDataListeners = new();
    private EyeTracker eyeTracker;
    private void Start()
    {
        eyeTracker = GetComponent<EyeTracker>();
        GetComponents(gazeDataListeners);
        if (gazeDataListeners.Count == 0)
        {
            Debug.LogWarning("No gaze data listeners found. Please add IGazeDataListener components to the scene.");
        }
    }

    public KeyboardContext GetCurrentContext(List<KeyboardContext> keyboardContexts, out Vector3 gaze3DInContext, out Vector2 gaze2DInContext)
    {
        gaze3DInContext = Vector3.zero;
        gaze2DInContext = Vector2.zero;
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
        gaze3DInContext = gaze.CyclopsRay.GetPoint(gazeInPlaneEnter);
        gaze2DInContext = bestContext.transform.InverseTransformPoint(gaze3DInContext);

        NotifyGazeDataListeners(
            gaze2DInContext,
            gaze.Position,
            eyeTracker.LeftEye.position,
            eyeTracker.RightEye.position,
            eyeTracker.LeftEye.forward,
            eyeTracker.RightEye.forward
        );

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

    private void NotifyGazeDataListeners(Vector2 gaze2D, Vector3 gaze3D, Vector3 leftEyePosition, Vector3 rightEyePosition, Vector3 leftEyeDirection, Vector3 rightEyeDirection)
    {
        foreach (var listener in gazeDataListeners)
        {
            listener.OnGaze(gaze2D, gaze3D, leftEyePosition, rightEyePosition, leftEyeDirection, rightEyeDirection);
        }
    }
}