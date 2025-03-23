using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance => instance;

    [SerializeField] private TextMeshProUGUI moveCount;

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

    public void UpdateUI(int moveLeft)
    {
        moveCount.text = moveLeft.ToString(); 
    }
  
}
