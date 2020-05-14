using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR;
using UnityEngine.UI;

public abstract class Allomancer : MonoBehaviour {
    
    #region Stats

    public float currentHealth;
    public float maxHealth;

    protected List<Metal> metals = new List<Metal>();
    protected Metal selectedMetal_Left;
    protected Metal selectedMetal_Right;
    private int metalIndex = 0;

    public float speed { get; set; } = 10.0f;
    public float strength { get; set; } = 5.0f;   //Maybe also determines jump-height?

    public float regeneration {get; set; } = 3.0f;

    #endregion

    #region VR Input

    private XRNode n_left = XRNode.LeftHand;
    private XRNode n_right = XRNode.RightHand;

    private InputDevice leftHand;
    private InputDevice rightHand;
    public Transform t_leftHand;
    public Transform t_rightHand;

    //For checking of continuous presses (holding the button)
    private bool pressingLeftPrimary = false;
    private bool pressingLeftSecondary = false;
    private bool pressingRightPrimary = false;
    private bool pressingRightSecondary = false;
    private bool pressingRightJoystick = false;
    private bool pressingLeftJoystick = false;

    //For short presses
    private bool isPressingLeftPrimary = false;
    private bool isPressingLeftSecondary = false;
    private bool isPressingRightPrimary = false;
    private bool isPressingRightSecondary = false;
    private bool isPressingRightJoystick = false;
    private bool isPressingLeftJoystick = false;
    private bool isJumping = false;

    #endregion

    public GameObject metalWheelCanvasPrefab;

    private GameObject leftMetalWheel;
    private GameObject rightMetalWheel;

    private bool leftMetalWheelOpen;
    private bool rightMetalWheelOpen;

    public Text metalText;

    private Rigidbody rb;


    public void Start() {
        this.rb = GetComponentInParent<Rigidbody>();

        selectedMetal_Left = metals[metalIndex];
    }

    protected void GetDevices() {
        leftHand = InputDevices.GetDeviceAtXRNode(n_left);
        rightHand = InputDevices.GetDeviceAtXRNode(n_right);
    }

    protected void OnEnable() {
        if(!leftHand.isValid || !rightHand.isValid)
            GetDevices();
    }

    protected void Update() {
        //Check if a both hands are connected and if not, try reconnecting them
        if (!leftHand.isValid || !rightHand.isValid) {
            GetDevices();
            return;
        }
        
        CheckForButtonPress();

        //Go trhough all available metals and handle burning
        foreach(Metal m in this.metals) {
            if (m.isBurning)
                m.Burn();
        }
    }

    protected void FixedUpdate() {
        CheckForJump();
    }

    protected void CheckForJump() {
        if (GetButtonDown(buttonInputs.Right_Joystick)) {
            Jump();
        }
    }

    protected void CheckForButtonPress() {

        //Toggle burning of selected metal
        if (GetButtonDown(buttonInputs.Left_Primary)) {   
            selectedMetal_Left.isBurning = !selectedMetal_Left.isBurning;
            if(selectedMetal_Left.isBurning)
                metalText.text = "Burning " + selectedMetal_Left;
        }

        // Cycle through available metals
        if (leftHand.TryGetFeatureValue(CommonUsages.secondaryButton, out pressingLeftSecondary) && pressingLeftSecondary) {
            //CycleThroughMetals();
            if(!leftMetalWheelOpen)
                OpenMetalSelectionWheel("Left");
            else {
                // Move wheel when hand gets too far away
                WheelFollow("Left");
            }
        }
        else if (!pressingLeftSecondary) {
            if (leftMetalWheelOpen) {
                CloseMetalSelectionWheel("Left");
            }
        }

        if (rightHand.TryGetFeatureValue(CommonUsages.secondaryButton, out pressingRightSecondary) && pressingRightSecondary) {
            //CycleThroughMetals();
            if(!rightMetalWheelOpen)
                OpenMetalSelectionWheel("Right");
            else {
                // Move wheel when hand gets too far away
                WheelFollow("Right");
            }
        }
        else if(!pressingRightSecondary) {
            if (rightMetalWheelOpen) {
                CloseMetalSelectionWheel("Right");
            }
        }

        /*FLARING A METAL*/
        HandleMetalFlaring();

        /*AIMING AN EXTERNAL METAL*/
        HandleAiming();
    }

    private void HandleMetalFlaring() {
        //Activate flaring of metal, when player holds the left Trigger
        if (leftHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool leftTriggerPress) && leftTriggerPress) {
            if (selectedMetal_Left.isBurning) {
                selectedMetal_Left.isFlaring = true;
                metalText.text = "Flaring " + selectedMetal_Left;
            }
        }

        //Stop flaring upon Trigger-release
        if (selectedMetal_Left.isFlaring && !leftTriggerPress) {
            selectedMetal_Left.isFlaring = false;
            metalText.text = "Burning " + selectedMetal_Left;
        }

        //Player stopped burning the currently selected metal
        if (!selectedMetal_Left.isBurning && !selectedMetal_Left.isFlaring) {
            this.selectedMetal_Left.StopBurning();
            metalText.text = "Burning stopped";
        }
    }

    private void HandleAiming() {
        if (rightHand.TryGetFeatureValue(CommonUsages.trigger, out float rightTriggerPressAmount) && rightTriggerPressAmount > 0.0f) {
            if (selectedMetal_Left.isBurning) {
                Vector3 pos = t_rightHand.position;

                Vector3 impactEnd = pos + t_rightHand.forward * this.selectedMetal_Left.influence;
                Collider[] cols = Physics.OverlapCapsule(pos, impactEnd, 1.0f);
                List<GameObject> objectsAimedAt = new List<GameObject>();
                foreach (Collider col in cols) {
                    if (!objectsAimedAt.Contains(col.gameObject))
                        if (col.gameObject.layer == 8)
                            objectsAimedAt.Add(col.gameObject);
                }

                this.selectedMetal_Left.Aim(objectsAimedAt, rightTriggerPressAmount);
            }
        }
    }

    private void OpenMetalSelectionWheel(string hand) {

        if (hand.Equals("Left")) {
            if (leftMetalWheel == null) {
                leftMetalWheel = Instantiate(metalWheelCanvasPrefab, t_leftHand.position, Quaternion.identity);
                leftMetalWheelOpen = true;
            }
        }
        else if (hand.Equals("Right")) {
            if (rightMetalWheel == null) {
                rightMetalWheel = Instantiate(metalWheelCanvasPrefab, t_rightHand.position, Quaternion.identity);
                rightMetalWheelOpen = true;
            }
        }
    }

    private void CloseMetalSelectionWheel(string hand) {
        if (hand.Equals("Left")) {
            if (leftMetalWheel != null) {
                Destroy(leftMetalWheel, 0.5f);
                leftMetalWheelOpen = false;
            }
        }
        else if (hand.Equals("Right")) {
            if (rightMetalWheel != null) {
                Destroy(rightMetalWheel, 0.5f);
                rightMetalWheelOpen = false;
            }
        }
    }

    private void WheelFollow(string hand) {
        GameObject wheel = hand.Equals("Left") ? leftMetalWheel : hand.Equals("Right") ? rightMetalWheel : null;
        Transform hand_ref = hand.Equals("Left") ? t_leftHand : hand.Equals("Right") ? t_rightHand : null;

        if (wheel == null || hand_ref == null)
            return;

        //Rotate wheel to look at player
        wheel.transform.rotation = Quaternion.LookRotation(Camera.main.transform.position - wheel.transform.position, Vector3.up);

        // Calculate distance between metalWheel and the player's hand
        float dist = Vector3.Distance(wheel.transform.position, hand_ref.position);

        if(dist >= 0.2f) {
            Vector3 old_wheelPos = wheel.transform.position;
            wheel.transform.position = Vector3.Lerp(old_wheelPos, hand_ref.position, 30 * Time.deltaTime);
        }
    }

    private void CycleThroughMetals() {
        if (this.metalIndex < this.metals.Count - 1)
            this.metalIndex++;
        else
            this.metalIndex = 0;

        selectedMetal_Left = this.metals[metalIndex];

        metalText.text = "Selected " + selectedMetal_Left;
    }

    private enum buttonInputs {
        Left_Primary,
        Left_Secondary,
        Right_Primary,
        Right_Secondary,
        Right_Joystick,
        Left_Joystick
    }

    private bool GetButtonDown(buttonInputs input) {
        bool value = false;

        switch (input) {
            case buttonInputs.Left_Primary:
                if (leftHand.TryGetFeatureValue(CommonUsages.primaryButton, out bool pressedLeftPrimary)) {
                    if (!isPressingLeftPrimary && pressedLeftPrimary) {
                        value = true;
                        isPressingLeftPrimary = true;
                    }
                    else if (!pressedLeftPrimary) {
                        isPressingLeftPrimary = false;
                    }
                }
                break;
            case buttonInputs.Left_Secondary:
                if (leftHand.TryGetFeatureValue(CommonUsages.secondaryButton, out bool pressedLeftSecondary)) {
                    if (!isPressingLeftSecondary && pressedLeftSecondary) {
                        value = true;
                        isPressingLeftSecondary = true;
                    }
                    else if (!pressedLeftSecondary) {
                        isPressingLeftSecondary = false;
                    }
                }
                break;
            case buttonInputs.Right_Primary:
                if (rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out bool pressedRightPrimary)) {
                    if (!isPressingRightPrimary && pressedRightPrimary) {
                        value = true;
                        isPressingRightPrimary = true;
                    }
                    else if (!pressedRightPrimary) {
                        isPressingRightPrimary = false;
                    }
                }
                break;
            case buttonInputs.Right_Secondary:
                if (rightHand.TryGetFeatureValue(CommonUsages.secondaryButton, out bool pressedRightSecondary)) {
                    if (!isPressingRightSecondary && pressedRightSecondary) {
                        value = true;
                        isPressingRightSecondary = true;
                    }
                    else if (!pressedRightSecondary)
                        isPressingRightSecondary = false;
                }
                break;
            case buttonInputs.Right_Joystick:
                if (rightHand.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool pressedRightJoystick)) {
                    if (!isPressingRightJoystick && pressedRightJoystick) {
                        value = true;
                        isPressingRightJoystick = true;
                    }
                    else if (!pressedRightJoystick)
                        isPressingRightJoystick = false;
                }
                break;
            case buttonInputs.Left_Joystick:
                if (rightHand.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool pressedLeftJoystick)) {
                    if (!isPressingLeftJoystick && pressedLeftJoystick) {
                        value = true;
                        isPressingRightJoystick = true;
                    }
                    else if (!pressedLeftJoystick)
                        isPressingRightJoystick = false;
                }
                break;
        }

        return value;
    } 

    public void PushPlayer(Vector3 force){
        this.rb.AddForce(force, ForceMode.Impulse);
    }

    private void Jump(){
        Vector3 jumpVector = this.transform.up * this.strength * 1000;
        isJumping = true;
        this.metalText.text = "Jumping";

        PushPlayer(jumpVector);
    }
}
