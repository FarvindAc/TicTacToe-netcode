using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager :NetworkBehaviour
{
    public NetworkVariable<int> currentTurn=new NetworkVariable<int>(0);

    public static GameManager Instance;
    [SerializeField] GameObject boardPrefab;
    GameObject newBoard;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else 
        {
            Instance = this;
        }
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (clienId) => 
        {
            Debug.Log(clienId);
            if (NetworkManager.Singleton.IsHost&&NetworkManager.Singleton.ConnectedClients.Count == 2) 
            {
                //Spawn Board
                Spawnboard();
            }
        };
    }
    void Spawnboard() 
    {
        newBoard = Instantiate(boardPrefab);
        newBoard.GetComponent<NetworkObject>().Spawn();
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();  
    }
    public void StartClient() 
    {
        NetworkManager.Singleton.StartClient();
    }

    [SerializeField] private GameObject gameEndPanel;
    [SerializeField] private TextMeshProUGUI msgText;

    public void ShowMsg(string msg)
    {
        if (msg.Equals("won"))
        {
            msgText.text = "You Won";
            gameEndPanel.SetActive(true);
            // Show Panel with text that Opponent Won
            ShowOpponentMsg("You Lose");
        }
        else if (msg.Equals("draw"))
        {
            msgText.text = "Game Draw";
            gameEndPanel.SetActive(true);
            ShowOpponentMsg("Game Draw");
        }
    }

    private void ShowOpponentMsg(string msg)
    {
        if (IsHost)
        {
            // Then use ClientRpc to show Message at Client Side
            OpponentMsgClientRpc(msg);
        }
        else
        {
            // Use ServerRpc to show message at Server Side
            OpponentMsgServerRpc(msg);
        }
    }

    [ClientRpc]
    private void OpponentMsgClientRpc(string msg)
    {
        if (IsHost) return;
        msgText.text = msg;
        gameEndPanel.SetActive(true);
    }


    [ServerRpc(RequireOwnership = false)]
    private void OpponentMsgServerRpc(string msg)
    {
        msgText.text = msg;
        gameEndPanel.SetActive(true);
    }



    public void Restart()
    {
        // If this is client, then call SererRpc to destroy current board and create new board
        // If this is client then Client will also call ServerRpc to hide result panel on host side

        if (!IsHost)
        {
           // RestartServerRpc();
            gameEndPanel.SetActive(false);
        }
        else
        {
            Destroy(newBoard);
            Spawnboard();
            RestartClientRpc();
        }

        // Destroy the current Game Board
        // Spawn a new board
        // Hide the Result Panel
    }
    [ServerRpc(RequireOwnership = false)]
    private void RestartServerRpc()
    {
        Destroy(newBoard);
        Spawnboard();
        gameEndPanel.SetActive(false);
    }


    [ClientRpc]
    private void RestartClientRpc()
    {
        gameEndPanel.SetActive(false);
    }
}
