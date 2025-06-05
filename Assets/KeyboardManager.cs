using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

// Optionally, for XR Interaction Toolkit, use UnityEngine.XR.Interaction.Toolkit;

public class KeyboardMain : MonoBehaviour
{
    public Camera mainCamera; // Assign in Inspector or find in Start
    public float distanceInFront = 2f; // Distance in front of the camera
    public GameObject keyPrefab; // Prefab for the keys

    // XR input variables
    public XRNode controllerNode = XRNode.RightHand; // Change to LeftHand if needed
    public InputHelpers.Button xrButton = InputHelpers.Button.PrimaryButton; // A/X button
    public List<KeyboardContext> keyboardContexts;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        keyboardContexts = new List<KeyboardContext>
        {
            new(0.0f, InstantiateKey),
            new(1.0f, InstantiateKey),
        };
    }

    private GameObject InstantiateKey()
    {
        GameObject key = Instantiate(keyPrefab);
        key.transform.SetParent(transform);
        return key;
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
    }
}

