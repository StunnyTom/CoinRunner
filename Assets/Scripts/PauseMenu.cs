using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public LevelManager levelManager;
    public LevelData[] levels;

    public void SelectLevel(int index)
    {
        var level = levels[index];
        if (level == levelManager.currentLevel)
            levelManager.ResetCurrentLevel();
        else
            levelManager.LoadLevel(level);
    }
}
