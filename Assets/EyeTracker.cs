using System;
using System.Collections.Generic;
using UnityEngine;

public class EyeTracker : MonoBehaviour
{
    public Transform leftEye;
    public Transform rightEye;
    private GameObject leftEyeDebug;
    private GameObject rightEyeDebug;

    private void Start()
    {
        if (leftEye == null || rightEye == null)
        {
            Debug.LogError("Left and right eye transforms must be assigned in the inspector.");
            return;
        }

        leftEyeDebug = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        leftEyeDebug.transform.localScale = new Vector3(0.02f, 0.1f, 0.02f);
        leftEyeDebug.transform.position = leftEye.position;
        leftEyeDebug.transform.rotation = Quaternion.LookRotation(leftEye.forward);
        leftEyeDebug.GetComponent<Renderer>().material.color = Color.blue;

        rightEyeDebug = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rightEyeDebug.transform.localScale = new Vector3(0.02f, 0.1f, 0.02f);
        rightEyeDebug.transform.position = rightEye.position;
        rightEyeDebug.transform.rotation = Quaternion.LookRotation(rightEye.forward);
        rightEyeDebug.GetComponent<Renderer>().material.color = Color.red;
    }

    private void Update()
    {
        print(leftEye.position + " " + rightEye.position + " -- " + leftEye.forward + " " + rightEye.forward);
        // Update debug objects to match eye positions and orientations
        leftEyeDebug.transform.position = leftEye.position;
        leftEyeDebug.transform.rotation = Quaternion.LookRotation(leftEye.forward);
        
        rightEyeDebug.transform.position = rightEye.position;
        rightEyeDebug.transform.rotation = Quaternion.LookRotation(rightEye.forward);
    }

    public GazePoint GetCurrentGazePoint(List<KeyboardContext> keyboardContexts)
    {
        if (keyboardContexts == null || keyboardContexts.Count == 0)
            return null;

        GazePoint bestPoint = null;
        float minDist = float.MaxValue;

        foreach (var context in keyboardContexts)
        {
            GazePoint gazePoint = GazeInContext(context);
            if (gazePoint.Context == null)
                continue;

            float dist = gazePoint.Error;
            if (dist < minDist)
            {
                minDist = dist;
                bestPoint = gazePoint;
            }

            // Debugging: Draw the gaze point
            Debug.DrawRay(gazePoint.Position, Vector3.up * 0.1f, Color.yellow);
        }

        return bestPoint;
    }

    private GazePoint GazeInContext(KeyboardContext context)
    {
        if (context == null)
            return GazePoint.Empty;

        Plane plane = context.Plane;
        Ray leftRay = new Ray(leftEye.position, leftEye.forward);
        Ray rightRay = new Ray(rightEye.position, rightEye.forward);

        float leftEnter, rightEnter;
        bool intersectLeft = plane.Raycast(leftRay, out leftEnter);
        bool intersectRight = plane.Raycast(rightRay, out rightEnter);
        if (intersectLeft && intersectRight)
        {
            Vector3 leftHitPoint = leftRay.GetPoint(leftEnter);
            Vector3 rightHitPoint = rightRay.GetPoint(rightEnter);
            Debug.DrawLine(leftHitPoint, rightHitPoint, Color.green);
            Vector3 gazePosition = (leftHitPoint + rightHitPoint) / 2.0f;
            float error = Vector3.Distance(leftHitPoint, rightHitPoint) / 2.0f;
            return new GazePoint(gazePosition, context, error);
        }
        else if (intersectLeft)
        {
            Vector3 hitPoint = leftRay.GetPoint(leftEnter);
            Debug.DrawRay(hitPoint, Vector3.up * 0.1f, Color.green);
            return new GazePoint(hitPoint, context, 0.0f);
        }
        else if (intersectRight)
        {
            Vector3 hitPoint = rightRay.GetPoint(rightEnter);
            Debug.DrawRay(hitPoint, Vector3.up * 0.1f, Color.green);
            return new GazePoint(hitPoint, context, 0.0f);
        }
        return GazePoint.Empty;
    }
}

public class GazePoint
{
    public Vector3 Position { get; set; }
    public KeyboardContext Context { get; set; }
    public float Error { get; set; }

    public static GazePoint Empty { get; } = new GazePoint(Vector3.zero, null, Mathf.Infinity);

    public GazePoint(Vector3 position, KeyboardContext context, float error)
    {
        Position = position;
        Context = context;
        Error = error;
    }
}