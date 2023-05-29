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
    public static List<GameObject> jugadores = new List<GameObject>();
    public static List<string> names = new List<string>();
    public static List<Text> playerNames = new List<Text>();

    [SerializeField] Text p1Name;
    [SerializeField] Text p2Name;
    [SerializeField] Text p3Name;
    [SerializeField] Text p4Name;

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

        playerNames.Add(p1Name);
        playerNames.Add(p2Name);
        playerNames.Add(p3Name);
        playerNames.Add(p4Name);

        p1Name.gameObject.SetActive(false);
        p2Name.gameObject.SetActive(false);
        p3Name.gameObject.SetActive(false);
        p4Name.gameObject.SetActive(false);
    }

    public static void AddPlayer(GameObject player)
    {
        Debug.Log("Añadido un jugador");
        jugadores.Add(player);
        names.Add(player.name);
    }

    public static void RemovePlayer(GameObject player)
    {
        Debug.Log("Jugador sacado");
        jugadores.Remove(player);
        int aux = names.IndexOf(player.name);
        names[aux] = null;
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
        
        for (int i = 0; i < names.Count; i++)
        {
            string name = names[i];
            SetNamesClientRpc(i, name);
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
    public void SetNamesClientRpc(int i, string name)
    {
        if(name != null)
        {
            playerNames[i].gameObject.SetActive(true);
            playerNames[i].text = name;
        }
        else
        {
            playerNames[i].gameObject.SetActive(false);
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