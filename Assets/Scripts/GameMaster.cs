using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton Class for handling game rules (Only for game scene)
/// </summary>
public class GameMaster : Singleton<GameMaster>
{
    public static event System.Action onTurnStart = () => { };
    public static event System.Action afterTurn = () => { };
    public static event System.Action onGameStart = () => { };

    int turnNumber = 0;

    bool gameStarted = false;

    public bool GameStarted { get => gameStarted; }

    public int TurnNumber { get => turnNumber; }

    private void Awake()
    {
        base.Awake();
        HexMapMaster.onMapCreated += StartGame;
    }

    private void OnDestroy()
    {
        HexMapMaster.onMapCreated -= StartGame;
    }

    public void NextTurn()
    {
        afterTurn();
        Myths();
        turnNumber++;
        onTurnStart();
        Debug.Log("Turn " + turnNumber);
    }

    void StartGame()
    {
        gameStarted = true;
        onGameStart();
        turnNumber++;
        Debug.Log("Turn " + turnNumber);
        onTurnStart();
    }

    void Myths()
    {

    }
}
