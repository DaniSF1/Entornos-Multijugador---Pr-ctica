using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WeaponFX : NetworkBehaviour
{
    public Animator effectsPrefab;          //Animación del ataque (hay que pasarle cual es)
    [ClientRpc]
    public void ColisionParticulaClientRpc(int Hit03, Vector3 hitpoint)
    {
        Animator effect = Instantiate(effectsPrefab);               //Creamos el efecto que se genera cuando el arma choca contra algo
        effect.transform.position = hitpoint;                       //Ponemos en el lugar de la colisión dicho efecto

        effect.SetTrigger(Hit03);                                   //Lanzamos el efecto
    }
}
