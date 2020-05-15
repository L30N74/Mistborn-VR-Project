using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInteract : MonoBehaviour
{
    Allomancer playerScript;
    string hand;


    public void SetMetal(string metalName) {
        playerScript.SetMetalBasedOnName(metalName, this.hand);
    }

    public void SetHand(Allomancer player, string hand) {
        this.playerScript = player;
        this.hand = hand;
    }
}
