using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance => instance;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI moveCountText;
    [SerializeField] private Button levelButton;
    [SerializeField] private TextMeshProUGUI levelButtonText;
    [SerializeField] private TextMeshProUGUI obstacleCountText;

    [Header("Panels")]
    [SerializeField] private GameObject celebrationPanel;
    [SerializeField] private GameObject failedPanel;
    [SerializeField] private GameObject mainScenePanel;
    [SerializeField] private GameObject levelScenePanel;


    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void InitializeUI()
    {
        UpdateLevelButtonText();
    }
    
    public void UpdateLevelButtonText()
    {
        if (levelButtonText == null) return;

        int savedLevel = PlayerPrefs.GetInt("SavedLevelNumber", 0);
        levelButtonText.text = savedLevel >= GameManager.Instance.levels.Length ?
            "Finished" : $"Level {savedLevel + 1}";
    }

    public void ActivateMainSceneUI(int currentLevel)
    {
        // Ensure we're in the main scene
        if (SceneManager.GetActiveScene().name != "MainScene")
            return;

        StartCoroutine(DelayedUIActivation(true, false, false, false));
        UpdateLevelButtonText();
    }

    public void ActivateLevelSceneUI()
    {
        // Ensure we're in the level scene
        if (SceneManager.GetActiveScene().name != "LevelScene")
            return;

        StartCoroutine(DelayedUIActivation(false, true, false, false));
    }
    public void ActivateCelebrationPopup() => SetPanelStates(celebration: true);

    public void ActivateFailScreenPopup() => SetPanelStates(failed: true);

    public void UpdateMoveCountUI(int movesLeft)
    {
        if (moveCountText != null)
        {
            moveCountText.text = movesLeft.ToString();
        }
      
    }

    public void UpdateRemainingObstacles(int obstacleCount)
    {
        if (obstacleCountText != null)
        {
            obstacleCountText.text = obstacleCount.ToString();
        }
    }
    private void SetPanelStates(bool main = false, bool level = false,
                              bool celebration = false, bool failed = false)
    {
        mainScenePanel.SetActive(main);
        levelScenePanel.SetActive(level);
        celebrationPanel.SetActive(celebration);
        failedPanel.SetActive(failed);
    }

    private IEnumerator DelayedUIActivation(bool main, bool level, bool celebration, bool failed)
    {
        // Wait one frame to ensure everything is initialized
        yield return null;

        mainScenePanel.SetActive(main);
        levelScenePanel.SetActive(level);
        celebrationPanel.SetActive(celebration);
        failedPanel.SetActive(failed);
    }
}