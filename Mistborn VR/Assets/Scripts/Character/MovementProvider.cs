using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class MovementProvider : LocomotionProvider
{
    public List<XRController> controllers = null;
    public float gravityMultiplier = 1.0f;
    
    private CharacterController charController = null;
    private GameObject head = null;

    private Allomancer characterScript;

    private void Awake() {
        characterScript = GetComponentInChildren<Allomancer>();
        charController = GetComponent<CharacterController>();
        head = GetComponent<XRRig>().cameraGameObject;
    }

    // Start is called before the first frame update
    void Start()
    {
        PositionController();
    }

    // Update is called once per frame
    void Update()
    {
        PositionController();
        CheckForInput();
        ApplyGravity();
    }

    private void PositionController() {

        // Get the head in local, playspace ground
        float headHeight = Mathf.Clamp(head.transform.localPosition.y, 1, 2);
        charController.height = headHeight;

        // Cut in half, add skin
        Vector3 newCenter = Vector3.zero;
        newCenter.y = charController.height / 2;
        newCenter.y = charController.skinWidth;

        // Move capsule in local space as well
        newCenter.x = head.transform.localPosition.x;
        newCenter.z = head.transform.localPosition.z;

        // Apply
        charController.center = newCenter;
    }

    private void CheckForInput() {
        foreach(XRController controller in controllers) {
            if (controller.enableInputActions)
                CheckForMovement(controller.inputDevice);
        }
    }

    private void CheckForMovement(InputDevice device) {
        if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 position))
            StartMove(position);
    }

    private void StartMove(Vector2 position) {
        Vector3 direction = new Vector3(position.x, 0, position.y);
        Vector3 headRotation = new Vector3(0, head.transform.eulerAngles.y, 0);

        direction = Quaternion.Euler(headRotation) * direction;

        Vector3 movement = direction * characterScript.speed;
        charController.Move(movement * Time.deltaTime);
    }

    private void ApplyGravity() {
        Vector3 gravity = new Vector3(0, Physics.gravity.y * gravityMultiplier, 0);
        gravity *= Time.deltaTime;
        charController.Move(gravity * Time.deltaTime);
    }
}
