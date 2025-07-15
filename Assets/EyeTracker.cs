using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class EyeTracker : MonoBehaviour
{
    abstract public Transform LeftEye { get; }
    abstract public Transform RightEye { get; }
    abstract public GazePoint CurrentGazePoint();
}

public class GazePoint
{
    public Vector3 Position { get; }
    public Ray CyclopsRay { get; }
    public float GazeDepth { get; }
    public float Error { get; }

    public static GazePoint Empty { get; } = new GazePoint(Vector3.zero, new Ray(), 0, Mathf.Infinity);

    public GazePoint(Vector3 position, Ray cyclopsRay, float gazeDepth, float error)
    {
        Position = position;
        CyclopsRay = cyclopsRay;
        Error = error;
        GazeDepth = gazeDepth;
    }
}