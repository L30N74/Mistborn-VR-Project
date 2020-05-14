using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mistborn : Allomancer
{
    // Start is called before the first frame update
    void Awake()
    {
        this.metals.Add(new Iron(this.transform));
        this.metals.Add(new Steel(this.transform));
        //this.metals.Add(new Pewter(this.transform));
    }
}
