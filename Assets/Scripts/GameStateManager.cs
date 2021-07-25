using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameStateManager : MonoBehaviour
{
    public enum GameState {
        InMenu,
        InGame,
        Paused,
        BuildMode

    }

    public List<GameState> gameStates;

    public static GameStateManager instance;

    private void Awake() {
        if(instance != null && instance != this) {
            Destroy(this.gameObject);
            return;
        } else {
            instance = this;
        }
    }

    private void Start() {
        gameStates.Add(GameState.InGame);
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleGameState(GameState gameState) {

        if(gameStates.Contains(gameState)) {
            gameStates.Remove(gameState);
        } else {
            gameStates.Add(gameState);
        }
    }
}
