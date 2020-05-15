using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Rigidbody))]
public class MetallicObject : MonoBehaviour
{
    private bool showLine;

    LineRenderer lineRenderer;

    // Start is called before the first frame update
    void Start(){
        this.lineRenderer = GetComponent<LineRenderer>();
        this.lineRenderer.startWidth = 0.2f;
        this.lineRenderer.endWidth = 0.1f;
    }

    public void Update() {
        if (showLine) {
            this.lineRenderer.enabled = true;
            this.lineRenderer.positionCount = 2;


            this.lineRenderer.SetPosition(0, this.transform.position);
            Vector3 targetPos = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y / 2, Camera.main.transform.position.z);
            this.lineRenderer.SetPosition(1, targetPos);
        }
        else
            this.lineRenderer.enabled = false;
    }

    public void DrawAllomanticLine(bool value) {
        this.showLine = value;
    }
}
