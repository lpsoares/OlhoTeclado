using System;
using System.Collections.Generic;
using UnityEngine;

public class BinocularEyeTracker : EyeTracker
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

    public override Transform LeftEye => leftEye;
    public override Transform RightEye => rightEye;

    /// <summary>
    /// Estimates the current gaze point based on the eye positions and gaze depth.
    /// </summary>
    /// <returns>The current GazePoint</returns>
    public override GazePoint CurrentGazePoint()
    {
        Vector3 cameraPos = Camera.main.transform.position;
        float gazeDepth = EstimateGazeDepth();
        Vector3 gazePosition = EstimateGazePointFromDepth(cameraPos, gazeDepth, out float error);
        Vector3 cyclopsEyePosition = (leftEye.position + rightEye.position) / 2.0f;
        Ray cyclopsRay = new(cyclopsEyePosition, gazePosition - cyclopsEyePosition);
        return new GazePoint(gazePosition, cyclopsRay, gazeDepth, error);
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
    private Vector3 EstimateGazePointFromDepth(Vector3 cameraPos, float gazeDepth, out float error)
    {
        Plane depthPlane = new(Camera.main.transform.forward, cameraPos + Camera.main.transform.forward * gazeDepth);
        Ray leftGazeRay = new(leftEye.position, leftEye.forward);
        Ray rightGazeRay = new(rightEye.position, rightEye.forward);
        depthPlane.Raycast(leftGazeRay, out float leftEnter);
        depthPlane.Raycast(rightGazeRay, out float rightEnter);
        Vector3 leftGazePoint = leftGazeRay.GetPoint(leftEnter);
        Vector3 rightGazePoint = rightGazeRay.GetPoint(rightEnter);
        Vector3 gazePosition = (leftGazePoint + rightGazePoint) / 2.0f;
        error = Vector3.Distance(leftGazePoint, rightGazePoint) / 2.0f;

        return gazePosition;
    }

    /// <summary>
    /// Estimates the gaze depth based on the intersection of the gaze rays from both eyes.
    /// This method projects the gaze rays onto a plane defined by the camera's up vector and calculates the depth
    /// from the camera position to the intersection point of the rays.
    /// If the intersection is behind the camera, it sets the gaze depth to a predefined infinity value.
    /// </summary>
    /// <param name="gazeDepth"></param>
    private float EstimateGazeDepth()
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
        float gazeDepth = Vector3.Dot(intersection - cameraPos, Camera.main.transform.forward);
        if (gazeDepth < 0)
        {
            gazeDepth = INFINITY_DEPTH;
        }
        return gazeDepth;
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
