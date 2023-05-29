using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickRecognizer : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnMouseDown()
    {
        if(gameObject.GetComponent<Renderer>().material.color == Color.white)
        {
            gameObject.GetComponent<Renderer>().material.color = Color.black;
        }
        else
        {
            gameObject.GetComponent<Renderer>().material.color = Color.white;
        }
    }
}
