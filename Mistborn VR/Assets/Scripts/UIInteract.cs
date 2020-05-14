using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInteract : MonoBehaviour
{
    public void Start() {
        //for(int i = 0; i < transform.childCount; i++) {
        //    Image image = transform.GetChild(i).GetComponent<Image>();
            
        //}
    }

    public void Test(Image image) {
        image.type = Image.Type.Filled;
        image.fillAmount = 0.3f;
        image.color = Color.black;
    }

    public void Test2() {
        Debug.Log("Hallo Test");
    }
}
