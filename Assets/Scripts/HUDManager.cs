using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public TextMeshProUGUI buildMode;
    public GameObject buildGrid;

    private void OnEnable() {
        GameStateManager.OnGameStateChange += ChangeHUD;
    }

    private void OnDisable() {
        GameStateManager.OnGameStateChange -= ChangeHUD;
    }

    public void ChangeHUD() {
        if(GameStateManager.instance.gameStates.Contains(GameStateManager.GameState.BuildMode)) {
            buildMode.text = "BUILD MODE: ON";
            buildGrid.SetActive(true);
        } else {
            buildMode.text = "BUILD MODE: OFF";
            buildGrid.SetActive(false);
        }
    }

}
