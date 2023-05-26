using Netcode;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class UIHandler : MonoBehaviour
    {
        public GameObject debugPanel;
        public GameObject lobbyUI;
        public Button hostButton;
        public Button clientButton;
        public TMP_InputField playerName;

        private void Start()
        {
            hostButton.onClick.AddListener(OnHostButtonClicked);
            clientButton.onClick.AddListener(OnClientButtonClicked);
        }

        private void OnHostButtonClicked()
        {
            NetworkManager.Singleton.StartHost();
            debugPanel.SetActive(false);
            lobbyUI.SetActive(true);
        }

        private void OnClientButtonClicked()
        {
            NetworkManager.Singleton.StartClient();
            debugPanel.SetActive(false);
            lobbyUI.SetActive(true);
        }

        public void ChangeScene()
        {
            SceneManager.LoadScene("Lobby");
        }
    }
}