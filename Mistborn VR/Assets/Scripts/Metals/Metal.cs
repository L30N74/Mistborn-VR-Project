using UnityEngine;
using System.Collections.Generic;

public abstract class Metal
{
    public bool isBurning { get; set; }
    public bool isFlaring { get; set; }

    public float influence { get; protected set; }  //The metal's radius of influence

    public float reserves;

    public MetalType metalType { get; protected set; }

    protected float drainRate;
    protected float drainRate_flaring;
    protected Transform player;
    protected Vector3 playerCOG;    // Center of gravity

    public Metal(Transform player) {
        this.player = player;
    }

    public abstract void Burn();
    public abstract void StopBurning();
    public abstract void Aim(List<GameObject> objects, float pressAmount);

    public enum MetalType {
        _physical,
        _mental,
        _temporal,
        _enhancement
    }
}


