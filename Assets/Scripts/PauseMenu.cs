using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public LevelManager levelManager;
    public LevelData[] levels;

    [Header("UI")]
    public GameObject winScreen; // référence au panneau de victoire
    public GameObject loseScreen; // référence au panneau de défaite

    public void SelectLevel(int index)
    {
        var level = levels[index];
        if (level == levelManager.currentLevel)
            levelManager.ResetCurrentLevel();
        else
            levelManager.LoadLevel(level);

        // Si on relance un niveau, on s’assure que le menu de victoire disparaît
        if (winScreen != null)
            winScreen.SetActive(false);

        if (loseScreen != null)
            loseScreen.SetActive(false);

        // Masquer le menu de sélection
        gameObject.SetActive(false);

        Time.timeScale = 1f; // reprend le jeu si le temps était en pause
    }

    public void OnBackButtonPressed()
    {
        // Masquer le menu de victoire si affiché
        if (winScreen != null)
            winScreen.SetActive(false);

        if (loseScreen != null)
            loseScreen.SetActive(false);

        // Reprendre le jeu normalement
        Time.timeScale = 1f;
    }
}
