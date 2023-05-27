using Movement.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static List<GameObject> jugadores = new List<GameObject>();
    public static NetworkVariable<float> time = new NetworkVariable<float>();

    private void Awake()
    {
        time.Value = 60f;
        time.OnValueChanged += TimeVictoryCondition;
    }

    public static void AddPlayer(GameObject player)
    {
        Debug.Log("A�adido un jugador");
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

    public static void TimeVictoryCondition(float previousValue, float newValue)
    {
        if (newValue > 0) return;
        GameObject ganador = EscogerGanador();
        VictoryCondition(ganador);
    }

    public static GameObject EscogerGanador()
    {
        int aux;
        GameObject ganador = null;
        foreach (GameObject go in jugadores) 
        { 
            if(go.GetComponent<FighterMovement>().health.Value >= 0)
            {
                aux = (int) go.GetComponent<FighterMovement>().health.Value;
                ganador = go;
            }
        }
        return ganador;
    }


    public void Update()
    {
        time.Value -= Time.deltaTime;
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