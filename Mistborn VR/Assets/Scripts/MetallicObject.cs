using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Rigidbody))]
public class MetallicObject : MonoBehaviour
{
    public bool showLine { get; set; }

    LineRenderer lr;
    private Transform player;

    // Start is called before the first frame update
    void Start(){
        this.lr = GetComponent<LineRenderer>();
        this.lr.startWidth = 0.2f;
        this.lr.endWidth = 0.1f;
    }

    public void Update() {
        if (showLine) {
            this.lr.enabled = true;
            this.lr.positionCount = 2;

            this.lr.SetPosition(0, this.transform.position);
            this.lr.SetPosition(1, player.transform.position);
        }
        else
            this.lr.enabled = false;
    }

    public void DrawAllomanticLine(Transform player, bool value) {
        this.player = player;
        this.showLine = value;
    }
}
