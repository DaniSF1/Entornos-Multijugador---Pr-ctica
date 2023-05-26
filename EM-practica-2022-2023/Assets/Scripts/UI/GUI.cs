using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUI : MonoBehaviour
{
    GameObject gui;
    // Start is called before the first frame update
    void Start()
    {
        gui = this.gameObject;
        gui.SetActive(false);
    }
}
