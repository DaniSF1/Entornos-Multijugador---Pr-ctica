using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class DisplayName : MonoBehaviour
{
    [SerializeField] private TMP_Text displayName;      //Caja de texto que contendra el nombre del jugador
    GameObject jugador;                                 //GameObject del jugador (esta variable asi como la siguiente funcion fueron descartados al final)

    public void getPlayer(GameObject player)
    {
        jugador = player;
        SetNamesClientRpc(jugador.name);
    }

    [ClientRpc]
    public void SetNamesClientRpc(string name)          //Mostramos en el cliente el nombre del jugador
    {
        displayName.text = name;
    }
}
