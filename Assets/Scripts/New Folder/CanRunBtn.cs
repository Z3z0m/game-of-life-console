using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanRunBtn : MonoBehaviour
{
    // Start is called before the first frame update
   public void btn()
   {
     FindObjectOfType<GameOfLife>().CanRun = true;
     FindObjectOfType<GameOfLife>().updateAlive();
     gameObject.SetActive(false);
   }
}
