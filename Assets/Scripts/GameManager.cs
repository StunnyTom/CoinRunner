using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int coinCounter = 0;
    public int coinsInLevel = 0;
    // Indique si le niveau a déjà été gagné
    public bool levelWon = false;

    public static event Action OnLevelWon;
    public static event Action OnLevelLost;

    public GameObject winScreen;
    public GameObject loseScreen;

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
        // Marquer le niveau comme gagné pour empêcher une défaite après coup
        levelWon = true;
        if (winScreen != null)
            winScreen.SetActive(true);

        Time.timeScale = 0f;
        OnLevelWon?.Invoke();
        Debug.Log("Level Completed!");
    }

    // Affichage de la défaite
    public void ShowLoseScreen()
    {
        if (loseScreen != null)
            loseScreen.SetActive(true);

        Time.timeScale = 0f;
        OnLevelLost?.Invoke();
        Debug.Log("Player Defeated!");
    }

    // Appelé quand un nouveau niveau est lancé
    public void ResetGameState()
    {
        Time.timeScale = 1f;

        // Réinitialiser le drapeau de victoire
        levelWon = false;

        if (winScreen != null) winScreen.SetActive(false);
        if (loseScreen != null) loseScreen.SetActive(false);
    }
}
