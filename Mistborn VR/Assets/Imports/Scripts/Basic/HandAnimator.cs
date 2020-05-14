using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class HandAnimator : MonoBehaviour
{
    public float speed = 5.0f;
    public XRController controller = null;

    private Animator animator = null;

    private readonly List<Finger> gripFingers = new List<Finger>() {
        new Finger(FingerType.Middle),
        new Finger(FingerType.Ring),
        new Finger(FingerType.Pinky)
    };

    private readonly List<Finger> pointFingers = new List<Finger>() {
        new Finger(FingerType.Index),
        new Finger(FingerType.Thumb)
    };

    private void Awake(){
        this.animator = GetComponent<Animator>();
    }

    private void Update(){
        // Store Input
        CheckGrip();
        CheckPointer();

        // Smooth input values
        SmoothFinger(pointFingers);
        SmoothFinger(gripFingers);

        // Apply smoothed values
        AnimateFinger(pointFingers);
        AnimateFinger(gripFingers);
    }

    private void CheckGrip(){
        if(controller.inputDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue)) {
            SetFingerTargets(gripFingers, gripValue);
        }
    }

    private void CheckPointer(){
        if (controller.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out float pointerValue)) {
            SetFingerTargets(pointFingers, pointerValue);
        }
    }

    private void SetFingerTargets(List<Finger> fingers, float value){
        foreach(Finger f in fingers) {
            f.target = value;
        }
    }

    private void SmoothFinger(List<Finger> fingers) {
        foreach(Finger f in fingers) {
            float time = speed * Time.unscaledDeltaTime;
            f.current = Mathf.MoveTowards(f.current, f.target, time);
        }
    }

    private void AnimateFinger(List<Finger> fingers) {
        foreach (Finger f in fingers){
            AnimateFinger(f.type.ToString(), f.current);
        }
    }

    private void AnimateFinger(string finger, float blend) {
        animator.SetFloat(finger, blend);
    }
}