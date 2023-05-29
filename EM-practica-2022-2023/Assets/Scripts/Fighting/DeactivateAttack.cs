using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DeactivateAttack : NetworkBehaviour    //Script que sirve para desactivar los ataques en el cliente
{
    void Start()
    {
        if(!IsServer)
        {
            this.gameObject.GetComponent<PolygonCollider2D>().isTrigger = true;     //Lo que hacemos es hacer que la hitbox del ataque sea un trigger. Asi el objeto no colisione con nada
        }
    }
}
