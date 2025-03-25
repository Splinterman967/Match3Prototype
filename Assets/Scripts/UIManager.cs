using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance => instance;

    [SerializeField] private TextMeshProUGUI moveCount;
    [SerializeField] private Button levelButton;


    [SerializeField] private GameObject CelebrationPanel;
    [SerializeField] private GameObject FailedPanel;
    [SerializeField] private GameObject MainScenePanel;
    [SerializeField] private GameObject LevelScenePanel;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Optional
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void ActivateMainSceneUI()
    {
        MainScenePanel.SetActive(true);
        LevelScenePanel.SetActive(false);

        CelebrationPanel.SetActive(false);
        FailedPanel.SetActive(false);

        int savedLevel = PlayerPrefs.GetInt("SavedLevelNumber", 0);
        levelButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Level {savedLevel +1}";

    }


    public void ActivateLevelSceneUI()
    {
        LevelScenePanel.SetActive(true);
        MainScenePanel.SetActive(false);

        CelebrationPanel.SetActive(false);
        FailedPanel.SetActive(false);
    }

    public void ActivateCelebrationPopup()
    {
        CelebrationPanel.SetActive(true);
        FailedPanel.SetActive(false);
    }


    public void ActivateFailScreenPopup()
    {
        FailedPanel.SetActive(true);
        CelebrationPanel.SetActive(false);
    }

    public void UpdateMoveCountUI(int moveLeft)
    {
        if (moveCount != null) { moveCount.text = moveLeft.ToString(); }

    }
 
  
   



}
