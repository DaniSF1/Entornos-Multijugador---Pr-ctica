using Movement.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    static List<GameObject> jugadores = new List<GameObject>();

    public static void AddPlayer(GameObject player)
    {
        Debug.Log("Añadido un jugador");
        jugadores.Add(player);
    }

    public static void RemovePlayer(GameObject player)
    {
        Debug.Log("Jugador sacado");
        jugadores.Remove(player);
    }

    public static void VictoryCondition(GameObject player)
    {
        Debug.Log($"Ha ganado {player.name}");
    }

    public void Update()
    {
        foreach (GameObject player in jugadores) 
        { 
            if(player.GetComponent<FighterMovement>().dead == true)
            {
                RemovePlayer(player);
            }
        }

        if (jugadores.Count == 1)
        {
            VictoryCondition(jugadores.First());
        }
    }
}
