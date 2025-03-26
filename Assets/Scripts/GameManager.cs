using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance => instance;

    [SerializeField] public TextAsset[] levels;
    private GameState currentState;
    private int savedLevel;

    public enum GameState
    {
        MainScene,
        LevelScene,
        GameWin,
        GameLose
    }

    public GameState CurrentState => currentState;
    void Start() => TriggerMainScene();

    private bool isTransitioning = false; 

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded; 
        savedLevel = PlayerPrefs.GetInt("SavedLevelNumber", 0);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; 
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainScene")
        {
            UIManager.Instance.ActivateMainSceneUI(savedLevel);
        }
        else if (scene.name == "LevelScene")
        {
            LoadLevel(savedLevel);
        }
    }

    public void SetState(GameState newState)
    {
        if (currentState == newState || isTransitioning) return;

        GridManager.Instance.ResetGrid();

        StartCoroutine(TransitionState(newState));
    }

    private IEnumerator TransitionState(GameState newState)
    {
        isTransitioning = true;

        // Exit current state
        switch (currentState)
        {
            case GameState.LevelScene:
                break;
        }

        currentState = newState;
        Debug.Log($"Transitioning to: {currentState}");

  
        AsyncOperation asyncLoad;
        switch (currentState)
        {
            case GameState.MainScene:
                asyncLoad = SceneManager.LoadSceneAsync("MainScene");
                break;

            case GameState.LevelScene:
                asyncLoad = SceneManager.LoadSceneAsync("LevelScene");
                break;

            case GameState.GameWin:
                HandleWinCondition();
                isTransitioning = false;
                yield break;

            case GameState.GameLose:
                HandleLoseCondition();
                isTransitioning = false;
                yield break;

            default:
                isTransitioning = false;
                yield break;
        }

        // Wait until scene is fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        isTransitioning = false;
    }

    private void LoadLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levels.Length)
        {
            GridManager.Instance.InitializeGrid(levels[levelIndex].text);
            UIManager.Instance.ActivateLevelSceneUI();
        }
    }

    private void HandleWinCondition()
    {
        Debug.Log("Level Completed!");
        PlayerPrefs.SetInt("SavedLevelNumber", Mathf.Min(savedLevel + 1, levels.Length - 1));
        UIManager.Instance.ActivateCelebrationPopup();
        Invoke(nameof(TriggerMainScene), 1.5f);
    }

    private void HandleLoseCondition()
    {
        Debug.Log("Level Failed");
        UIManager.Instance.ActivateFailScreenPopup();
    }

    public void CheckWinCondition()
    {
        if (GridManager.Instance.gridArray == null) return;

        int obstacleCount = 0;
        bool hasMovesLeft = GridManager.Instance.moveCount > 0;

        // Count all remaining obstacles
        foreach (ICellItem item in GridManager.Instance.gridArray)
        {
            if (item != null && (item.ItemCode == ItemCode.bo || item.ItemCode == ItemCode.s || item.ItemCode == ItemCode.v))
            {
                obstacleCount++;
            }
        }
        UIManager.Instance.UpdateRemainingObstacles(obstacleCount);
        if (obstacleCount == 0 && hasMovesLeft)
        {
            TriggerWin();
        }
        else if (!hasMovesLeft)
        {
            TriggerLose();
        }
    }

 
    public void TriggerMainScene() => SetState(GameState.MainScene);
    public void TriggerLevelScene() => SetState(GameState.LevelScene);
    public void TriggerWin() => SetState(GameState.GameWin);
    public void TriggerLose() => SetState(GameState.GameLose);

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Levels/Set Last Played Level")]
    private static void SetLastPlayedLevel()
    {
        var window = UnityEditor.EditorWindow.GetWindow<LevelEditorWindow>("Set Level");
        window.Show();
    }

    private class LevelEditorWindow : UnityEditor.EditorWindow
    {
        private int selectedLevel = 0;

        void OnGUI()
        {
            selectedLevel = UnityEditor.EditorGUILayout.IntSlider("Level Index", selectedLevel, 0, Instance.levels.Length - 1);

            if (GUILayout.Button("Set"))
            {
                PlayerPrefs.SetInt("SavedLevelNumber", selectedLevel);
                Instance.savedLevel = selectedLevel;
                Instance.TriggerMainScene();
                Close();
            }
        }
    }
#endif
}