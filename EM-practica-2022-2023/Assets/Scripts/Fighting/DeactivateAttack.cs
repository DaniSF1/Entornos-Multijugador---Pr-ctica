using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DeactivateAttack : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(!IsServer)
        {
            this.gameObject.SetActive(false);
        }
        
    }
}
