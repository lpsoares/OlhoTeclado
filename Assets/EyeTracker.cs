using System;
using System.Collections.Generic;
using UnityEngine;

public class EyeTracker : MonoBehaviour
{
    public Transform leftEye;
    public Transform rightEye;
    private readonly float INFINITY_DEPTH = 100.0f;

    private void Start()
    {
        if (leftEye == null || rightEye == null)
        {
            Debug.LogError("Left and right eye transforms must be assigned in the inspector.");
            return;
        }
    }

    /// <summary>
    /// Estimates the current gaze point based on the eye positions and gaze depth.
    /// </summary>
    /// <param name="keyboardContexts"></param>
    /// <returns></returns>
    public GazePoint GetCurrentGazePoint(List<KeyboardContext> keyboardContexts)
    {
        if (keyboardContexts == null || keyboardContexts.Count == 0)
            return null;
        Vector3 cameraPos = Camera.main.transform.position; ;
        EstimateGazeDepth(out float gazeDepth);
        EstimateGazePointFromDepth(cameraPos, gazeDepth, out Vector3 gazePosition, out float error);
        KeyboardContext bestContext = GetClosestContext(keyboardContexts, cameraPos, gazeDepth);
        if (bestContext == null)
        {
            return GazePoint.Empty;
        }
        Vector3 cyclopsEyePosition = (leftEye.position + rightEye.position) / 2.0f;
        Ray cyclopsRay = new Ray(cyclopsEyePosition, gazePosition - cyclopsEyePosition);
        Plane contextPlane = bestContext.TargetPlane;
        if (!contextPlane.Raycast(cyclopsRay, out float gazeInPlaneEnter))
        {
            return GazePoint.Empty;
        }
        Vector3 gazeInContext = cyclopsRay.GetPoint(gazeInPlaneEnter);

        return new GazePoint(gazePosition, cyclopsRay, gazeInContext, bestContext, error);
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

    /// <summary>
    /// Estimates the gaze point in 3D space based on the camera position and gaze depth.
    /// This method uses the gaze rays from both eyes to find the intersection point at the specified depth.
    /// It also calculates the error as the average distance between the two gaze points.
    /// </summary>
    /// <param name="cameraPos"></param>
    /// <param name="gazeDepth"></param>
    /// <param name="gazePosition"></param>
    /// <param name="error"></param>
    private void EstimateGazePointFromDepth(Vector3 cameraPos, float gazeDepth, out Vector3 gazePosition, out float error)
    {
        Plane depthPlane = new Plane(Camera.main.transform.forward, cameraPos + Camera.main.transform.forward * gazeDepth);
        Ray leftGazeRay = new(leftEye.position, leftEye.forward);
        Ray rightGazeRay = new(rightEye.position, rightEye.forward);
        depthPlane.Raycast(leftGazeRay, out float leftEnter);
        depthPlane.Raycast(rightGazeRay, out float rightEnter);
        Vector3 leftGazePoint = leftGazeRay.GetPoint(leftEnter);
        Vector3 rightGazePoint = rightGazeRay.GetPoint(rightEnter);
        gazePosition = (leftGazePoint + rightGazePoint) / 2.0f;
        error = Vector3.Distance(leftGazePoint, rightGazePoint) / 2.0f;
    }

    /// <summary>
    /// Estimates the gaze depth based on the intersection of the gaze rays from both eyes.
    /// This method projects the gaze rays onto a plane defined by the camera's up vector and calculates the depth
    /// from the camera position to the intersection point of the rays.
    /// If the intersection is behind the camera, it sets the gaze depth to a predefined infinity value.
    /// </summary>
    /// <param name="gazeDepth"></param>
    private void EstimateGazeDepth(out float gazeDepth)
    {
        // Project the gaze rays into plane with normal equal to camera up vector
        Vector3 cameraUp = Camera.main.transform.up;
        Vector3 cameraPos = Camera.main.transform.position;

        // Project left and right eye rays onto the gazePlane
        Vector3 leftEyePos = Vector3.ProjectOnPlane(leftEye.position - cameraPos, cameraUp) + cameraPos;
        Vector3 rightEyePos = Vector3.ProjectOnPlane(rightEye.position - cameraPos, cameraUp) + cameraPos;
        Vector3 leftDirection = Vector3.ProjectOnPlane(leftEye.forward, cameraUp);
        Vector3 rightDirection = Vector3.ProjectOnPlane(rightEye.forward, cameraUp);
        LineLineIntersection(out Vector3 intersection, leftEyePos, leftDirection, rightEyePos, rightDirection);
        gazeDepth = Vector3.Dot(intersection - cameraPos, Camera.main.transform.forward);
        if (gazeDepth < 0)
        {
            gazeDepth = INFINITY_DEPTH;
        }
    }

    private static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1,
        Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2) {
        // Source: https://stackoverflow.com/questions/59449628/check-when-two-vector3-lines-intersect-unity3d

        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parallel
        if( Mathf.Abs(planarFactor) < 0.0001f 
                && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) 
                    / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }
}

public class GazePoint
{
    public Vector3 Position { get; }
    public Vector3 PositionInContext { get; }
    public Ray CyclopsRay { get; }
    public float CyclopsGazeDepth => CyclopsRay.direction.magnitude;
    public KeyboardContext Context { get; }
    public float Error { get; }

    public static GazePoint Empty { get; } = new GazePoint(Vector3.zero, new Ray(), Vector3.zero, null, Mathf.Infinity);

    public GazePoint(Vector3 position, Ray cyclopsRay, Vector3 positionInContext, KeyboardContext context, float error)
    {
        Position = position;
        CyclopsRay = cyclopsRay;
        PositionInContext = positionInContext;
        Context = context;
        Error = error;
    }
}