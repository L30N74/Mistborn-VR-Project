using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

public abstract class Allomancer : MonoBehaviour {

    public GameObject test;

    #region Stats

    protected List<Metal> metals = new List<Metal>();
    protected Metal selectedMetal;
    private int metalIndex = 0;

    public float speed { get; protected set; } = 10.0f;
    public float strength { get; protected set; } = 5.0f;   //Maybe also determines jump-height?
    public float mass = 60; //Pewter may increase this?

    #endregion

    #region VR Input

    private XRNode n_left = XRNode.LeftHand;
    private XRNode n_right = XRNode.RightHand;

    private InputDevice leftHand;
    private InputDevice rightHand;
    public Transform t_leftHand;
    public Transform t_rightHand;

    private bool isPressingLeftPrimary = false;
    private bool isPressingLeftSecondary = false;
    private bool isPressingRightPrimary = false;
    private bool isPressingRightSecondary = false;

    #endregion

    public Text metalText;

    public void Start() {
        selectedMetal = metals[metalIndex];
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
        if (!leftHand.isValid || !rightHand.isValid) {
            GetDevices();
            return;
        }

        CheckForButtonPress();

        if (this.selectedMetal.isBurning)
            this.selectedMetal.Burn();
    }

    protected void CheckForButtonPress() {

        if (GetButtonDown(buttonInputs.Left_Primary)) {   
            //Toggle burning of selected metal
            selectedMetal.isBurning = !selectedMetal.isBurning;
            if(selectedMetal.isBurning)
                metalText.text = "Burning " + selectedMetal;
        }

        if (GetButtonDown(buttonInputs.Left_Secondary)) {
            // Cycle through available metals
            CycleThroughMetals();
        }

        /*FLARING A METAL*/

        if (leftHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool leftTriggerPress) && leftTriggerPress) {
            if (selectedMetal.isBurning) {
                selectedMetal.isFlaring = true;
                metalText.text = "Flaring " + selectedMetal;
            }
        }

        if (selectedMetal.isFlaring && !leftTriggerPress) {
            selectedMetal.isFlaring = false;
            metalText.text = "Burning " + selectedMetal;
        }

        if (!selectedMetal.isBurning && !selectedMetal.isFlaring)
            metalText.text = "Burning stopped";


        /*AIMING DIRECTION*/

        if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool rightTriggerPress) && rightTriggerPress) {
            if (selectedMetal.isBurning) {
                Vector3 pos = t_rightHand.position;

                Vector3 impactEnd = pos + t_rightHand.forward * 10;
                Collider[] cols = Physics.OverlapCapsule(pos, impactEnd, 1.0f);
                List<GameObject> objectsAimedAt = new List<GameObject>();
                foreach (Collider col in cols) {
                    if (!objectsAimedAt.Contains(col.gameObject))
                        if (col.gameObject.layer == 8)
                            objectsAimedAt.Add(col.gameObject);
                }

                this.selectedMetal.Aim(objectsAimedAt);
            }
        }
    }

    private void CycleThroughMetals() {
        if (this.metalIndex < this.metals.Count - 1)
            this.metalIndex++;
        else
            this.metalIndex = 0;

        selectedMetal = this.metals[metalIndex];

        metalText.text = "Selected " + selectedMetal;
    }

    
    private enum buttonInputs {
        Left_Primary,
        Left_Secondary,
        Right_Primary,
        Right_Secondary,
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
        }

        return value;
    } 
}
