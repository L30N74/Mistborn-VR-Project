using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

public abstract class Allomancer : MonoBehaviour {
    
    #region Stats

    public float currentHealth;
    public float maxHealth;

    protected List<Metal> metals = new List<Metal>();
    protected Metal selectedMetal_Left;
    protected Metal selectedMetal_Right;

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

    private XRRayInteractor left_rayInteractor;
    private XRRayInteractor right_rayInteractor;

    [SerializeField]
    private LayerMask metalLayer;

    //For checking of continuous presses (holding the button)
    //private bool pressingLeftPrimary = false;
    private bool pressingLeftSecondary = false;
    //private bool pressingRightPrimary = false;
    private bool pressingRightSecondary = false;
    //private bool pressingRightJoystick = false;
    //private bool pressingLeftJoystick = false;

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

    [SerializeField]
    private Image leftMetalImage;
    [SerializeField]
    private Image rightMetalImage;

    private bool leftMetalWheelOpen;
    private bool rightMetalWheelOpen;

    public Text metalText;

    private Rigidbody rb;

    private Sprite[] metalImages;


    public void Start() {
        this.rb = GetComponentInParent<Rigidbody>();

        this.metalImages = Resources.LoadAll<Sprite>("wheel_proto");

        leftMetalImage.sprite = metalImages[0];
        rightMetalImage.sprite = metalImages[1];

        selectedMetal_Left = metals[0];
        selectedMetal_Right = metals[1];
    }

    protected void GetDevices() {
        leftHand = InputDevices.GetDeviceAtXRNode(n_left);
        rightHand = InputDevices.GetDeviceAtXRNode(n_right);

        this.left_rayInteractor = t_leftHand.GetComponent<XRRayInteractor>();
        this.right_rayInteractor = t_rightHand.GetComponent<XRRayInteractor>();
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

        //Go through all available metals and handle burning
        foreach (Metal m in this.metals) {
            if (m.isBurning)
                m.Burn();
        }
    }

    protected void FixedUpdate() {
        CheckForJump();
    }

    protected void CheckForButtonPress() {
        HandleMetalToggle();

        HandleMetalSwitching();

        //HandleMetalFlaring();

        HandleAiming();
    }

    private void HandleMetalToggle() {
        //Toggle burning of selected metal in left hand
        if (GetButtonDown(buttonInputs.Left_Primary)) {
            selectedMetal_Left.isBurning = !selectedMetal_Left.isBurning;
        }

        //Toggle burning of selected metal in right hand
        if (GetButtonDown(buttonInputs.Right_Primary)) {
            selectedMetal_Right.isBurning = !selectedMetal_Right.isBurning;
        }

        //Player stopped burning the currently selected metal (left hand)
        if (!selectedMetal_Left.isBurning) {
            this.selectedMetal_Left.StopBurning();
        }

        //Player stopped burning the currently selected metal (right hand)
        if (!selectedMetal_Right.isBurning) {
            this.selectedMetal_Right.StopBurning();
        }
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
    }

    private void HandleMetalSwitching() {

        // Cycle through available metals
        if (leftHand.TryGetFeatureValue(CommonUsages.secondaryButton, out pressingLeftSecondary) && pressingLeftSecondary) {
            if (!leftMetalWheelOpen)
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
            if (!rightMetalWheelOpen)
                OpenMetalSelectionWheel("Right");
            else {
                // Move wheel when hand gets too far away
                WheelFollow("Right");
            }
        }
        else if (!pressingRightSecondary) {
            if (rightMetalWheelOpen) {
                CloseMetalSelectionWheel("Right");
            }
        }
    }  

    private void HandleAiming() {
        if (rightHand.TryGetFeatureValue(CommonUsages.trigger, out float rightTriggerPressAmount) && rightTriggerPressAmount > 0.0f) {
            if (selectedMetal_Right.isBurning && !rightMetalWheelOpen) {
                Vector3 pos = t_rightHand.position;
                float aimColliderRadius = 1.0f;

                Vector3 impactEnd = pos + t_rightHand.forward * this.selectedMetal_Right.influence;
                Collider[] collisions = Physics.OverlapCapsule(pos, impactEnd, aimColliderRadius, this.metalLayer);
                
                this.selectedMetal_Right.Aim(collisions, rightTriggerPressAmount, t_rightHand);
            }
        }

        if (leftHand.TryGetFeatureValue(CommonUsages.trigger, out float leftTriggerPressAmount) && leftTriggerPressAmount > 0.0f) {
            if (selectedMetal_Left.isBurning && !leftMetalWheelOpen) {
                Vector3 pos = t_leftHand.position;
                float aimColliderRadius = 1.0f;

                Vector3 impactEnd = pos + t_leftHand.forward * this.selectedMetal_Left.influence;
                Collider[] collisions = Physics.OverlapCapsule(pos, impactEnd, aimColliderRadius, this.metalLayer);
                
                this.selectedMetal_Left.Aim(collisions, leftTriggerPressAmount, t_leftHand);
            }
        }
    }

    private void OpenMetalSelectionWheel(string hand) {
        if (hand.Equals("Left")) {
            if (leftMetalWheel != null) return;
            
            leftMetalWheel = Instantiate(metalWheelCanvasPrefab, t_leftHand.position, Quaternion.identity);
            leftMetalWheel.GetComponent<Canvas>().worldCamera = Camera.main;
            leftMetalWheel.GetComponent<UIInteract>().SetHand(this, "Left");
            left_rayInteractor.enabled = true;
            leftMetalWheelOpen = true;
        }
        else if (hand.Equals("Right")) {
            if (rightMetalWheel != null) return;

            rightMetalWheel = Instantiate(metalWheelCanvasPrefab, t_rightHand.position, Quaternion.identity);
            rightMetalWheel.GetComponent<Canvas>().worldCamera = Camera.main;
            rightMetalWheel.GetComponent<UIInteract>().SetHand(this, "Right");
            right_rayInteractor.enabled = true;
            rightMetalWheelOpen = true;
        }
    }

    private void CloseMetalSelectionWheel(string hand) {
        if (hand.Equals("Left")) {
            if (leftMetalWheel == null) return;

            Destroy(leftMetalWheel, 0.5f);
            left_rayInteractor.enabled = false;
            leftMetalWheelOpen = false;
        }
        else if (hand.Equals("Right")) {
            if (rightMetalWheel == null) return;

            Destroy(rightMetalWheel, 0.5f);
            right_rayInteractor.enabled = false;
            rightMetalWheelOpen = false;
        }
    }

    private void WheelFollow(string hand) {
        GameObject wheel = hand.Equals("Left") ? leftMetalWheel : hand.Equals("Right") ? rightMetalWheel : null;
        Transform hand_ref = hand.Equals("Left") ? t_leftHand : hand.Equals("Right") ? t_rightHand : null;

        if (wheel == null || hand_ref == null)
            return;

        float moveThreshhold = 0.4f;
        float moveSpeed = 30f;

        //Rotate wheel to look at player
        wheel.transform.rotation = Quaternion.LookRotation(wheel.transform.position - Camera.main.transform.position, Vector3.up);

        // Calculate distance between metalWheel and the player's hand
        float dist = Vector3.Distance(wheel.transform.position, hand_ref.position);

        if(dist > moveThreshhold) {
            Vector3 wheelPos = wheel.transform.position;
            wheel.transform.position = Vector3.Lerp(wheelPos, hand_ref.position, moveSpeed * Time.deltaTime);
        }
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

    protected void CheckForJump() {
        if (GetButtonDown(buttonInputs.Right_Joystick)) {
            Jump();
        }
    }

    private void Jump(){
        Vector3 jumpVector = this.transform.up * this.strength * 1000;
        isJumping = true;
        this.metalText.text = "Jumping";

        PushPlayer(jumpVector);
    }

    public void SetMetalBasedOnName(string metalName, string hand) {
        Metal chosenMetal = getMetalFromName(metalName);
        Sprite chosenMetal_image = getMetalImageFromName(metalName);

        if (chosenMetal == null) return;
        if (chosenMetal_image == null) return;

        if (hand.Equals("Left")) {
            selectedMetal_Left = chosenMetal;
            leftMetalImage.sprite = chosenMetal_image;
        }
        else {
            selectedMetal_Right = chosenMetal;
            rightMetalImage.sprite = chosenMetal_image;
        }
    }

    private Metal getMetalFromName(string metalName) {
        foreach(Metal metal in this.metals) {
            if (metal.GetType().ToString().Equals(metalName)) {
                return metal;
            }
        }

        return null;
    }

    private Sprite getMetalImageFromName(string metalName) {
        foreach(Sprite metalImage in this.metalImages) {
            string[] nameParts = metalImage.name.Split('_');
            if (nameParts[2].Equals(metalName))
                return metalImage;
        }
        
        return null;
    }
}
