using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.ComponentModel;
public class BoardMAnager : NetworkBehaviour
{
    Button[,] buttons = new Button[3, 3];
    [SerializeField] Sprite X, O;
    public override void OnNetworkSpawn()
    {
        var cells=GetComponentsInChildren<Button>();
        int n = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++) 
            {
                buttons[i, j] = cells[n];
                n++;

                int r = i;
                int k = j;
                buttons[i, j].onClick.AddListener(delegate{ OnCellClick(r, k); });
            }
        }
    }

    void OnCellClick(int r,int k )
    {
        //Button sprite change for host X for Client O and make button uninteractable after the click
        if (NetworkManager.Singleton.IsHost &&GameManager.Instance.currentTurn.Value==0) 
        {
            buttons[r, k].GetComponent<Image>().sprite = X;
            buttons[r, k].interactable = false;
            ChangeSpriteClientRPC(r,k);
            CheckResult(r, k);
            GameManager.Instance.currentTurn.Value=1;
        }
        else if (!NetworkManager.Singleton.IsHost && GameManager.Instance.currentTurn.Value == 1)
        {
            buttons[r, k].GetComponent<Image>().sprite = O;
            buttons[r, k].interactable = false;
            ChangeSpriteServerRPC(r,k);
            CheckResult(r, k);
            // GameManager.Instance.currentTurn.Value = 0;
        }
      
    }

    [ClientRpc]
    void ChangeSpriteClientRPC(int r,int k) 
    {
        buttons[r, k].GetComponent<Image>().sprite = X;
        buttons[r, k].interactable = false;
    }
    [ServerRpc(RequireOwnership = false)]
    void ChangeSpriteServerRPC(int r, int k)
    {
        buttons[r, k].GetComponent<Image>().sprite = O;
        buttons[r, k].interactable = false;
        GameManager.Instance.currentTurn.Value = 0;
    }

    private void CheckResult(int r, int c)
    {
        if (IsWon(r, c))
        {
            GameManager.Instance.ShowMsg("won");
        }
        else
        {
            if (IsGameDraw())
            {
                GameManager.Instance.ShowMsg("draw");
            }
        }
    }





    public bool IsWon(int r, int c)
    {
        Sprite clickedButtonSprite = buttons[r, c].GetComponent<Image>().sprite;
        // Checking Column
        if (buttons[0, c].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
            buttons[1, c].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
            buttons[2, c].GetComponentInChildren<Image>().sprite == clickedButtonSprite)
        {
            return true;
        }

        // Checking Row

        else if (buttons[r, 0].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
            buttons[r, 1].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
            buttons[r, 2].GetComponentInChildren<Image>().sprite == clickedButtonSprite)
        {
            return true;
        }

        // Checking First Diagonal

        else if (buttons[0, 0].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
            buttons[1, 1].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
            buttons[2, 2].GetComponentInChildren<Image>().sprite == clickedButtonSprite)
        {
            return true;
        }

        // Checking 2nd Diagonal
        else if (buttons[0, 2].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
        buttons[1, 1].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
        buttons[2, 0].GetComponentInChildren<Image>().sprite == clickedButtonSprite)
        {
            return true;
        }

        return false;
    }


    private bool IsGameDraw()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (buttons[i, j].GetComponent<Image>().sprite != X &&
                    buttons[i, j].GetComponent<Image>().sprite !=O)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
