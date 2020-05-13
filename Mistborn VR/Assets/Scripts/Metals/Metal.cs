using UnityEngine;

public abstract class Metal
{
    public bool isBurning { get; set; }
    public bool isFlaring { get; set; }

    public float reserves;

    protected float drainRate;
    protected float drainRate_flaring;
    protected Transform player;

    public Metal(Transform player) {
        this.player = player;
    }

    public abstract void Burn();

}
