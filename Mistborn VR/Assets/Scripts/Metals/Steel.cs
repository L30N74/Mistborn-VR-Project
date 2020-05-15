using UnityEngine;
using System.Collections.Generic;

public class Steel : Metal {
    public LayerMask metalLayer = LayerMask.GetMask("Metallic");

    private List<GameObject> nearbySources = new List<GameObject>();

    public Steel(Transform player) : base(player) {
        this.metalType = MetalType._physical;

        this.drainRate = 0.3f;
        this.drainRate_flaring = 1.5f * this.drainRate;

        this.influence = 30.0f;
    }

    public override void Burn() {
        float searchRadius = this.isFlaring ? influence * 2 : influence;

        //look for nearby sources
        List<Collider> sphere = new List<Collider>(Physics.OverlapSphere(player.position, searchRadius, metalLayer));
        foreach (Collider col in sphere) {
            if (!nearbySources.Contains(col.gameObject)) {
                this.nearbySources.Add(col.gameObject);
                col.gameObject.GetComponent<MetallicObject>().DrawAllomanticLine(true);
            }
        }

        foreach (GameObject g in nearbySources) {
            if (!sphere.Contains(g.GetComponent<Collider>())) {
                nearbySources.Remove(g);
                g.GetComponent<MetallicObject>().DrawAllomanticLine(false);
            }
        }
    }

    public override void StopBurning() {
        foreach (GameObject g in nearbySources) {
            g.GetComponent<MetallicObject>().DrawAllomanticLine(false);
        }

        //Empty the list
        this.nearbySources.Clear();
    }

    public override void Aim(Collider[] objects, float amountPressed, Transform hand) {
        foreach (Collider g in objects) {
            Rigidbody objRigidb = g.GetComponent<Rigidbody>();

            Vector3 pushVector = g.transform.position - hand.position;

            // Determine force with which to push object
            //Get player's mass
            float p_mass = this.player.GetComponentInParent<Rigidbody>().mass;
            float pushForce_Player = Mathf.Clamp(objRigidb.mass/p_mass * amountPressed, -80, 80);
            float pushForce_Object = Mathf.Clamp(p_mass/objRigidb.mass * amountPressed, -80, 80);
            
            player.SendMessage("PushPlayer", -(pushVector * pushForce_Player));

            pushForce_Object = this.isFlaring ? 1.5f * pushForce_Object : pushForce_Object;
            pushVector *= pushForce_Object;
            objRigidb.AddForce(pushVector, ForceMode.Impulse);
        }
    }
}
