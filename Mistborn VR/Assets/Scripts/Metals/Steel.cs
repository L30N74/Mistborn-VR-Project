using UnityEngine;
using System.Collections.Generic;

public class Steel : Metal {

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
        List<Collider> sphere = new List<Collider>(Physics.OverlapSphere(player.position, searchRadius));
        foreach (Collider col in sphere) {
            if (col.gameObject.layer == 8) {
                if (!nearbySources.Contains(col.gameObject)) {
                    this.nearbySources.Add(col.gameObject);
                    col.gameObject.GetComponent<MetallicObject>().DrawAllomanticLine(player, true);
                }
            }
        }

        // Remove sources that are too far away
        foreach (GameObject g in nearbySources) {
            if (!sphere.Contains(g.GetComponent<Collider>())) {
                nearbySources.Remove(g);
                g.GetComponent<MetallicObject>().showLine = false;
            }
        }
    }

    public override void StopBurning() {
        foreach (GameObject g in nearbySources) {
            g.GetComponent<MetallicObject>().showLine = false;
        }

        //Empty the list
        this.nearbySources.Clear();
    }

    public override void Aim(List<GameObject> objects, float amountPressed) {
        this.playerCOG = new Vector3(player.position.x, player.position.y + player.GetComponentInParent<CharacterController>().height / 2, player.position.z);









      foreach (GameObject g in objects) {
            Rigidbody objRigidb = g.GetComponent<Rigidbody>();

            Vector3 pushVector = g.transform.position - this.playerCOG;

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
