using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pewter : Metal
{
    private float burnFactor;
    private float flareFactor;
    private Allomancer playerScript;

    public Pewter(Transform player) : base(player){
        this.burnFactor = 2.0f;
        this.flareFactor = this.burnFactor * 3.0f;

        this.playerScript = player.GetComponent<Allomancer>();
    }

    public override void Burn(){
        this.playerScript.speed *= isFlaring ? flareFactor : burnFactor;
        this.playerScript.strength *= isFlaring ? flareFactor : burnFactor;
        // this.player.GetComponentInParent<Rigidbody>().mass *= isFlaring ? 2f : 1.3f;
        this.playerScript.regeneration *= isFlaring ? flareFactor : burnFactor;
    }

    public override void StopBurning() {
        this.playerScript.speed /= isFlaring ? flareFactor : burnFactor;
        this.playerScript.strength /= isFlaring ? flareFactor : burnFactor;
        // this.player.GetComponentInParent<Rigidbody>().mass /= isFlaring ? 2f : 1.3f;
        this.playerScript.regeneration /= isFlaring ? flareFactor : burnFactor;
    }

    public override void Aim(Collider[] objects, float amountPressed, Transform hand){
        //Ahm, Pewter doesn't need aiming... 
    }
}
