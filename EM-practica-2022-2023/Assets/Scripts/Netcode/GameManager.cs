using JetBrains.Annotations;
using Lobby.UI;
using Movement.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static List<string> names = new List<string>();
    public static List<Text> playerNames = new List<Text>();

    [SerializeField] Text p1Name;
    [SerializeField] Text p2Name;
    [SerializeField] Text p3Name;
    [SerializeField] Text p4Name;

    static List<GameObject> livingPlayers = new List<GameObject>();
    static List<GameObject> currentPlayers = new List<GameObject>();

    public GameObject wincanvas;
    public Text winnerName;

    [SerializeField] Text timerText;
    [SerializeField] Button restartButton;
    static float timeStart = 180f;
    static float time;
    static bool matchStarted;
    public static Action onGameRestart;

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
        livingPlayers.Add(player);
        currentPlayers.Add(player);
    }

    public static void RemoveDisconectedPlayer(GameObject player)
    {
        int aux = names.IndexOf(player.name);
        names[aux] = null;
        livingPlayers.Remove(player);
        currentPlayers.Remove(player);
    }

    public static void RemoveDeadPlayer(GameObject player)
    {
        livingPlayers.Remove(player);
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
        foreach (GameObject go in livingPlayers) 
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
        if (IsHost && currentPlayers.Count > 1) { restartButton.interactable = true; }
        else { restartButton.interactable = false; }
        UpdateTimer();
        UpdateWin();

    }

    private void UpdateTimer()
    {

        if (!IsServer || !matchStarted) return;
        time -= Time.deltaTime;

        UpdateTimerClientRpc(time);

        foreach (GameObject player in livingPlayers)
        {
            if (player.GetComponent<FighterMovement>().dead == true)
            {
                RemoveDisconectedPlayer(player);
            }
        }
        
        for (int i = 0; i < names.Count; i++)
        {
            string name = names[i];
            SetNamesClientRpc(i, name);
        }
    }


    private void UpdateWin()
    {
        if (time <= timeStart - 10)
        {
            if (livingPlayers.Count == 1 || time <= 0)
            {
                string player;
                if (livingPlayers.Count == 1)
                {
                    player = livingPlayers[0].name;
                }
                else
                {
                    player = EscogerGanador();
                }

                WinClientRpc(player);
            }
        }
    }


    public void OnGameRestart()
    {
        time = timeStart;
        onGameRestart?.Invoke();
        RestartClientRpc();
        RestartServerRpc();
    }

    [ServerRpc]
    public void RestartServerRpc()
    {
        livingPlayers = currentPlayers;
    }

    [ClientRpc]
    public void RestartClientRpc()
    {
        wincanvas.SetActive(false);
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