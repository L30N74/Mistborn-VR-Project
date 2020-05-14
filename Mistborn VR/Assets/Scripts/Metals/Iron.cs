using System.Collections.Generic;
using UnityEngine;

public class Iron : Metal
{    
    private List<GameObject> nearbySources = new List<GameObject>();

    public Iron(Transform player) : base(player) {
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
        
        //Remove 
        foreach(GameObject g in nearbySources) {
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
    }


    public override void Aim(List<GameObject> objects, float amountPressed) {
        foreach (GameObject g in objects) {
            Rigidbody objRigidb = g.GetComponent<Rigidbody>();

            Vector3 pullVector = -(g.transform.position - player.position);

            // Determine force with which to push the object and player
            //Get masses
            float p_mass = this.player.GetComponentInParent<Rigidbody>().mass;
            float pullForce_Player = Mathf.Clamp(objRigidb.mass/ p_mass * amountPressed, -80, 80);
            float pullForce_Object = Mathf.Clamp(p_mass/objRigidb.mass * amountPressed, -80, 80);
            
            
            player.SendMessage("PushPlayer", -pullVector * pullForce_Player);

            pullForce_Object = this.isFlaring ? 1.5f * pullForce_Object : pullForce_Object;
            pullVector *= pullForce_Object;
            objRigidb.AddForce(pullVector, ForceMode.Impulse);
        }
    }
}
