using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public TextMeshProUGUI BuildMode;

    private void OnEnable() {
        GameStateManager.OnGameStateChange += ChangeHUD;
    }

    private void OnDisable() {
        GameStateManager.OnGameStateChange -= ChangeHUD;
    }

    public void ChangeHUD() {
        if(GameStateManager.instance.gameStates.Contains(GameStateManager.GameState.BuildMode)) {
            BuildMode.text = "BUILD MODE: ON";
        } else {
            BuildMode.text = "BUILD MODE: OFF";
        }
    }

}
