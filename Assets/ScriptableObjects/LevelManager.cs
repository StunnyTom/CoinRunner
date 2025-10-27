using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public Transform levelRoot;
    public Transform player;
    public LevelData currentLevel;

    private readonly System.Collections.Generic.List<GameObject> spawnedObjects = new();

    void Start()
    {
        if (currentLevel != null)
            LoadLevel(currentLevel);
    }

    public void LoadLevel(LevelData level)
    {
        // Reset les anciens objets
        foreach (var obj in spawnedObjects)
            Destroy(obj);
        spawnedObjects.Clear();

        SpawnPrefabs(level.walls);
        SpawnPrefabs(level.enemies);
        SpawnPrefabs(level.items);

        // Déplacer le joueur
        if (player != null)
        {
            player.position = level.spawnPosition;
            player.rotation = Quaternion.Euler(level.spawnRotation);
        }

        // Mise à jour des pièces
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterCoins(level.items.Length);

        currentLevel = level;
    }

    private void SpawnPrefabs(PrefabSpawnData[] prefabsData)
    {
        foreach (var data in prefabsData)
        {
            if (data == null || data.prefab == null) continue;

            // Instanciation avec la position & rotation définies dans le LevelData
            var instance = Instantiate(data.prefab, levelRoot);
            instance.transform.SetPositionAndRotation(
                data.position,
                Quaternion.Euler(data.rotation)
            );

            spawnedObjects.Add(instance);
        }
    }

    public void ResetCurrentLevel()
    {
        if (currentLevel != null)
            LoadLevel(currentLevel);
    }
}
