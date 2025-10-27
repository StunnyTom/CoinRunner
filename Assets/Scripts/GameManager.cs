using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int coinCounter = 0;
    public int coinsInLevel = 0;

    public GameObject winScreen;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterCoins(int count)
    {
        coinsInLevel = count;
        coinCounter = 0;
        Debug.Log("Coins in level: " + coinsInLevel);
    }

    public void AddCoin()
    {
        coinCounter++;
        Debug.Log($"Coins: {coinCounter}/{coinsInLevel}");

        if (coinCounter >= coinsInLevel)
            ShowWinScreen();
    }

    private void ShowWinScreen()
    {
        if (winScreen != null)
            winScreen.SetActive(true);

        Time.timeScale = 0f;
        Debug.Log("Level Completed!");
    }
}
