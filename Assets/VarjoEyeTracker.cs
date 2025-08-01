using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Varjo.XR;

public class VarjoEyeTracker : EyeTracker
{
    private UnityEngine.InputSystem.InputAction calibrateGazeAction;

    [Header("Gaze data output frequency")]
    public VarjoEyeTracking.GazeOutputFrequency frequency;

    private GameObject leftEye;
    private GameObject rightEye;
    private KeyboardState currentContextState = KeyboardState.Initial;
    private KeyboardManager keyboardManager;
    private VarjoEyeTracking.GazeData latestGaze;
    private List<float> gazeDepthWindow = new List<float>();

    private void Start()
    {
        VarjoEyeTracking.SetGazeOutputFrequency(frequency);

        calibrateGazeAction = UnityEngine.InputSystem.InputSystem.actions.FindAction("Calibrate gaze");
        if (calibrateGazeAction == null)
        {
            Debug.LogError("Input action for gaze calibration not found. Please ensure it is defined in the Input System.");
            return;
        }

        keyboardManager = FindObjectOfType<KeyboardManager>();
        if (keyboardManager == null)
        {
            Debug.LogError("KeyboardManager not found in the scene. Please ensure it is present.");
            return;
        }

        leftEye = new GameObject("LeftEye");
        rightEye = new GameObject("RightEye");

        leftEye.transform.SetParent(Camera.main.transform);
        rightEye.transform.SetParent(Camera.main.transform);
    }

    private void Update()
    {
        if (calibrateGazeAction.triggered)
        {
            Debug.Log("Calibrate gaze action triggered.");
            bool success = VarjoEyeTracking.RequestGazeCalibration();
            if (success)
            {
                Debug.Log("Gaze calibration requested successfully.");
            }
            else
            {
                Debug.LogError("Failed to request gaze calibration.");
            }
        }

        // int ret = VarjoEyeTracking.GetGazeList(out List<VarjoEyeTracking.GazeData> gazeData, out List<VarjoEyeTracking.EyeMeasurements> eyeMeasurements);
        // Debug.Log($"Gaze data count: {gazeData.Count}, Eye measurements count: {eyeMeasurements.Count}");

        bool isAllowed = VarjoEyeTracking.IsGazeAllowed();
        bool isCalibrated = VarjoEyeTracking.IsGazeCalibrated();
        if (!isAllowed)
        {
            Debug.LogWarning("Gaze tracking is not allowed.");
        }
        else if (!isCalibrated)
        {
            Debug.LogWarning("Gaze tracking is not calibrated.");
        }
        else
        {
            int dataLength = VarjoEyeTracking.GetGazeList(out List<VarjoEyeTracking.GazeData> gazeDataList);
            if (dataLength == 0)
            {
                Debug.LogWarning("Gaze tracking is not active or gaze data is invalid.");
                return;
            }

            Debug.Log($"Gaze data count: {dataLength}");
            for (int i = 0; i < dataLength; i++)
            {
                VarjoEyeTracking.GazeData gazeData = gazeDataList[i];
                if (gazeData.status == VarjoEyeTracking.GazeStatus.Valid)
                {
                    Debug.Log($"Gaze {i}: Depth: {gazeData.focusDistance}, Stability: {gazeData.focusStability}");
                    latestGaze = gazeData;

                    if (gazeData.focusStability > 0.99f)
                    {
                        gazeDepthWindow.Add(gazeData.focusDistance);
                        if (gazeDepthWindow.Count > 10)
                        {
                            gazeDepthWindow.RemoveAt(0);
                        }
                        float averageDepth = gazeDepthWindow.Average();
                        Debug.Log($"Average gaze depth: {averageDepth}");
                    }
                }
            }

            VarjoEyeTracking.GazeRay leftEyeRay = latestGaze.left;
            VarjoEyeTracking.GazeRay rightEyeRay = latestGaze.right;
            leftEye.transform.localPosition = leftEyeRay.origin;
            leftEye.transform.rotation = Quaternion.LookRotation(leftEyeRay.forward);
            rightEye.transform.localPosition = rightEyeRay.origin;
            rightEye.transform.rotation = Quaternion.LookRotation(rightEyeRay.forward);
        }
    }

    public override Transform LeftEye => leftEye.transform;
    public override Transform RightEye => rightEye.transform;

    /// <summary>
    /// Estimates the current gaze point based on the controller position and the current plane.
    /// </summary>
    /// <returns>The current GazePoint</returns>
    public override GazePoint CurrentGazePoint()
    {
        if (latestGaze.status != (VarjoEyeTracking.GazeStatus) 2)
        {
            return GazePoint.Empty;
        }

        Vector3 position = Camera.main.transform.TransformPoint(latestGaze.gaze.origin);
        Vector3 forward = Camera.main.transform.TransformDirection(latestGaze.gaze.forward);
        Ray cyclopsRay = new Ray(position, forward);
        float averageDepth = gazeDepthWindow.Average();
        Vector3 gazePosition = cyclopsRay.GetPoint(averageDepth);
        Debug.Log($"Gaze depth: {averageDepth}");
        float error = latestGaze.focusStability;
        return new GazePoint(gazePosition, cyclopsRay, averageDepth, error);
    }
}
