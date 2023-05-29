using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WeaponFX : NetworkBehaviour
{
    public Animator effectsPrefab;     //Animación de las particulas de ataque (la pasamos desde el inspector en los prefabs para mayor facilidad)

    [ClientRpc]
    public void ColisionParticulaClientRpc(int Hit03, Vector3 hitpoint)
    {
        Debug.Log($"{Hit03}, {hitpoint}");
        Animator effect = Instantiate(effectsPrefab);               //Instanciamos el prefab del efecto que se genera cuando el arma choca contra algo
        effect.transform.position = hitpoint;                       //Ponemos en el lugar de la colisión dicho efecto
        effect.SetTrigger(Hit03);                                   //Lanzamos el efecto
    }
}
