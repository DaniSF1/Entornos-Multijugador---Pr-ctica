using Lobby.UI;
using Movement.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    static List<GameObject> jugadores = new List<GameObject>();
    public GameObject wincanvas;
    public Text winnerName;
    [SerializeField] Text timerText;
    static float timeStart = 180f;
    static float time;
    static bool matchStarted;

    private void Awake()
    {
        time = timeStart;
        matchStarted = false;
        LobbyPlayer.OnGameStart += (a) => matchStarted = true;
    }

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

    public static void VictoryCondition(string player)
    {
        Debug.Log($"Ha ganado {player}");

    }

    public void Start()
    {
       // winnerName = wincanvas.GetComponent<Text>();
        wincanvas.SetActive(false);
    }

    public static string EscogerGanador()
    {
        int aux = 0;
        GameObject ganador = null;
        foreach (GameObject go in jugadores) 
        { 
            if(go.GetComponent<FighterMovement>().health.Value >= aux)
            {
                aux = (int) go.GetComponent<FighterMovement>().health.Value;
                ganador = go;
            }
        }
        return ganador.name;
    }

    public void Update()
    {
        if (!IsServer || !matchStarted) return;
        time -= Time.deltaTime;

        UpdateTimerClientRpc(time);

        foreach (GameObject player in jugadores)
        {
            if (player.GetComponent<FighterMovement>().dead == true)
            {
                RemovePlayer(player);
            }
        }

        if (time <= timeStart - 10)
        {
            if (jugadores.Count == 1 || time <= 0)
            {
                string player;
                if (jugadores.Count == 1)
                {
                    player = jugadores[0].name;
                }
                else
                {
                    player = EscogerGanador();
                }

                WinClientRpc(player);
            }
        }
    }

    [ClientRpc]
    public void WinClientRpc(string win)
    {
        VictoryCondition(win);
        string winner = win;
        winnerName.text = winner;
        wincanvas.SetActive(true);
    }

    [ClientRpc]
    public void UpdateTimerClientRpc(float timeRemaining)
    {
        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
        }
        timerText.text = timeRemaining.ToString("0.0");
    }
}