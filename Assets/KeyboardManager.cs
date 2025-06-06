using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

// Optionally, for XR Interaction Toolkit, use UnityEngine.XR.Interaction.Toolkit;

public class KeyboardMain : MonoBehaviour
{
    public Camera mainCamera; // Assign in Inspector or find in Start
    public float distanceInFront = 2f; // Distance in front of the camera
    public EyeTracker eyeTracker;
    public GameObject keyboardContextPrefab;

    // XR input variables
    public XRNode controllerNode = XRNode.RightHand; // Change to LeftHand if needed
    public InputHelpers.Button xrButton = InputHelpers.Button.PrimaryButton; // A/X button
    public List<KeyboardContext> keyboardContexts;
    private GameObject gazeDebugObject;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (eyeTracker == null)
        {
            eyeTracker = FindObjectOfType<EyeTracker>();
        }

        keyboardContexts = new List<KeyboardContext>
        {
            Instantiate(keyboardContextPrefab).GetComponent<KeyboardContext>(),
            Instantiate(keyboardContextPrefab).GetComponent<KeyboardContext>(),
            Instantiate(keyboardContextPrefab).GetComponent<KeyboardContext>(),
        };
        for (int i = 0; i < keyboardContexts.Count; i++)
        {
            keyboardContexts[i].transform.SetParent(transform);
        }
        keyboardContexts[0].Depth = -0.5f;
        keyboardContexts[1].Depth = 0.0f;
        keyboardContexts[2].Depth = 1.0f;

        // Instantiate a red sphere for gaze debugging
        gazeDebugObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gazeDebugObject.transform.localScale = Vector3.one * 0.1f; // Scale down the sphere
        gazeDebugObject.GetComponent<Renderer>().material.color = Color.red; // Set color to red
        gazeDebugObject.SetActive(false); // Initially hide the debug object
    }

    void Update()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(controllerNode);
        bool buttonPressed = false;
        if (device.isValid)
        {
            device.TryGetFeatureValue(CommonUsages.primaryButton, out buttonPressed);
        }

        if (buttonPressed)
        {
            Vector3 inFront = mainCamera.transform.position + mainCamera.transform.forward * distanceInFront;
            transform.SetPositionAndRotation(inFront, Quaternion.LookRotation(mainCamera.transform.forward, mainCamera.transform.up));
        }

        GazePoint gazePoint = eyeTracker.GetCurrentGazePoint(keyboardContexts);
        if (gazePoint != null && gazePoint.Context != null)
        {
            gazeDebugObject.transform.position = gazePoint.Position;
            gazeDebugObject.SetActive(true);
        }
    }
}

