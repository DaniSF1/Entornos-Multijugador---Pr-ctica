using Movement.Components;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Fighting
{
    public class Weapon : NetworkBehaviour
    {
        private static readonly int Hit03 = Animator.StringToHash("hit03");     //Cogemos la animación con el hash "Hit03"

        private void OnCollisionEnter2D(Collision2D collision)      //Cuando el arma choca con algo...
        {
            GameObject otherObject = collision.gameObject;              //Guardamos el objeto con el que colisiona
            //Debug.Log($"Sword collision with {otherObject.name}");
            Vector3 hitpoint = collision.GetContact(0).point;           //Ponemos en el lugar de la colisión dicho efecto

            //ColisionParticulaClientRpc(effect);
            new WeaponFX().ColisionParticulaClientRpc(Hit03, hitpoint);

            // TODO: Review if this is the best way to do this
            IFighterReceiver enemy = otherObject.GetComponent<IFighterReceiver>();  //Cargamos el enemigo contra el que choca el ataque
            if (enemy != null)       //Si hay enemigo...
                enemy.TakeHit();     //el enemigo recibe daño
            //No funciona
        }
    }
}
