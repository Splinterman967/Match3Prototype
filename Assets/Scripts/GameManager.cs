using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance => instance;

    [SerializeField] private TextAsset[] levels;

    private int savedLevel;

    // State machine enum
    public enum GameState
    {
        MainScene,
        LevelScene,
        GameWin,
        GameLose
    }

    // Current state
    private GameState currentState;
    public GameState CurrentState => currentState;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
         PlayerPrefs.SetInt("SavedLevelNumber", 0);
        SetState(GameState.MainScene); 
    }

    void Update()
    {
        //// Handle state-specific logic
        //switch (currentState)
        //{
        //    case GameState.MainScene:
        //        // Main scene specific update logic if needed
        //        break;

        //    case GameState.LevelScene:
        //        // Level scene specific update logic if needed
        //        break;

        //    case GameState.GameWin:
        //        // Automatically transition back to MainScene after win
        //        break;

        //    case GameState.GameLose:
        //        // Handle lose state logic if needed
        //        break;
        //}
    }

    // Method to change states
    public void SetState(GameState newState)
    {
        if (currentState == newState) return;

        // Exit current state
        switch (currentState)
        {
            case GameState.MainScene:
                OnMainSceneExit();
                break;
            case GameState.LevelScene:
                OnLevelSceneExit();
                break;
            case GameState.GameWin:
                OnGameWinExit();
                break;
            case GameState.GameLose:
                OnGameLoseExit();
                break;
        }

        currentState = newState;

        // Enter new state
        switch (currentState)
        {
            case GameState.MainScene:
                OnMainSceneEnter();
                break;
            case GameState.LevelScene:
                OnLevelSceneEnter();
                break;
            case GameState.GameWin:
                OnGameWinEnter();
                break;
            case GameState.GameLose:
                OnGameLoseEnter();
                break;
        }

        Debug.Log(currentState);
    }

    // State enter methods
    private void OnMainSceneEnter()
    {
        SceneManager.LoadScene("MainScene");

        UIManager.Instance.ActivateMainSceneUI();

        GridManager.Instance.ResetGrid();
    }

    private void OnLevelSceneEnter()
    {
        SceneManager.LoadScene("LevelScene");
        int savedLevel = PlayerPrefs.GetInt("SavedLevelNumber", 0);
        string json = levels[savedLevel].text;

        UIManager.Instance.ActivateLevelSceneUI();
        GridManager.Instance.InitializeGrid(json);
    }

    private void OnGameWinEnter()
    {
        Debug.Log("Congratulations Level Finished");
        PlayerPrefs.SetInt("SavedLevelNumber", PlayerPrefs.GetInt("SavedLevelNumber", 0) + 1);
        GridManager.Instance.ResetGrid();
        UIManager.Instance.ActivateCelebrationPopup();
        Invoke(nameof(TriggerMainScene), 1.5f);
    }

    private void OnGameLoseEnter()
    {
        Debug.Log("Failed to Finish the Level");
        GridManager.Instance.ResetGrid();
        UIManager.Instance.ActivateFailScreenPopup();
    }


    // Public methods to trigger state changes

    private void TriggerMainScene()
    {
        SetState(GameState.MainScene);
    }
    public void TriggerLevelScene()
    {
        SetState(GameState.LevelScene);
    }


    // Public methods to trigger win/lose states
    public void TriggerWin()
    {
        SetState(GameState.GameWin);
    }

    public void TriggerLose()
    {
        SetState(GameState.GameLose);
    }




    // State exit methods (optional, add logic as needed)
    private void OnMainSceneExit() { }
    private void OnLevelSceneExit() { }
    private void OnGameWinExit() { }
    private void OnGameLoseExit() { }
}