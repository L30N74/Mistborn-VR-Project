using UnityEngine;
using System.Collections.Generic;

public class Steel : Metal {

    private List<GameObject> nearbySources = new List<GameObject>();
    public float radius { get; private set; } = 10.0f;

    public Steel(Transform player) : base(player) {
        this.metalType = MetalType._physical;

        this.drainRate = 0.3f;
        this.drainRate_flaring = 1.5f * this.drainRate;
    }

    public override void Burn() {

        float searchRadius = this.isFlaring ? radius * 2 : radius;

        //look for nearby sources
        foreach (Collider col in Physics.OverlapSphere(player.position, searchRadius)) {
            if (!nearbySources.Contains(col.gameObject))
                if (col.gameObject.layer == 8) {
                    
                    this.nearbySources.Add(col.gameObject);
                }
        }

        foreach (GameObject g in nearbySources) {
            if (!isFlaring)
                g.GetComponent<MeshRenderer>().material.SetColor("_Color", RandomColor());
        }

        //Draw a thin, blue line to all of them, originating from the player's chest
    }

    private Color RandomColor() {

        return new Color(Random.Range(0f,1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    }

    public override void Aim(List<GameObject> objects) {
        foreach (GameObject g in objects) {
            Rigidbody objRigidb = g.GetComponent<Rigidbody>();

            Vector3 pushVector = g.transform.position - player.position;
            float pushForce = 1.0f;
            pushVector *= pushForce;
            objRigidb.AddForce(pushVector, ForceMode.Impulse);
        }
    }
}
