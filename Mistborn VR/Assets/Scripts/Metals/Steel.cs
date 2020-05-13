﻿using UnityEngine;
using System.Collections.Generic;

public class Steel : Metal {

    private List<GameObject> nearbySources = new List<GameObject>();
    private float radius = 10.0f;

    public Steel(Transform player) : base(player) {
        this.drainRate = 0.3f;
        this.drainRate_flaring = 1.5f * this.drainRate;
    }

    public override void Burn() {

        float searchRadius = this.isFlaring ? radius * 2 : radius;

        //look for nearby sources
        foreach (Collider col in Physics.OverlapSphere(player.position, searchRadius)) {
            this.nearbySources.Add(col.gameObject);
        }

        //Draw a thin, blue line to all of them, originating from the player's chest
    }
}
