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
        GameManager.Instance?.ResetGameState();

        // Détruire immédiatement les anciens objets
        foreach (var obj in spawnedObjects)
            if (obj != null)
                DestroyImmediate(obj);
        spawnedObjects.Clear();

        // Nettoyer le parent s'il garde des enfants
        foreach (Transform child in levelRoot)
            DestroyImmediate(child.gameObject);

        SpawnPrefabs(level.walls);
        SpawnPrefabs(level.enemies, "Enemy");

        // Répositionner explicitement les ennemis après la génération
        RepositionEnemies(level.enemies);

        SpawnPrefabs(level.items);

        // Initialiser la position de départ des ennemis (mémoire pour le retour)
        foreach (var enemy in FindObjectsOfType<EnemyAI>())
        {
            enemy.ResetStartPosition();
        }

        // Déplacer le joueur 
        if (player != null)
        {
            var controller = player.GetComponent<CharacterController>();
            if (controller != null)
                controller.enabled = false; // désactive le CharacterController avant le move

            player.position = level.spawnPosition;
            player.rotation = Quaternion.Euler(level.spawnRotation);

            if (controller != null)
                controller.enabled = true; // réactive après déplacement
        }

        // Mise à jour des pièces
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterCoins(level.items.Length);

        currentLevel = level;

    }


    public void RepositionEnemies(PrefabSpawnData[] enemiesData)
    {
        if (enemiesData == null || enemiesData.Length == 0) return;

        int enemyLayer = LayerMask.NameToLayer("Enemy");

        var spawnedEnemyObjs = new System.Collections.Generic.List<GameObject>();
        foreach (var obj in spawnedObjects)
        {
            if (obj == null) continue;
            if (obj.layer == enemyLayer)
                spawnedEnemyObjs.Add(obj);
        }

        int count = Mathf.Min(spawnedEnemyObjs.Count, enemiesData.Length);
        for (int i = 0; i < count; i++)
        {
            var target = spawnedEnemyObjs[i];
            var data = enemiesData[i];
            if (target == null || data == null) continue;

            // Désactiver temporairement le CharacterController pour éviter les déplacements
            var cc = target.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            target.transform.SetPositionAndRotation(data.position, Quaternion.Euler(data.rotation));

            if (cc != null) cc.enabled = true;
        }

        if (spawnedEnemyObjs.Count != enemiesData.Length)
            Debug.LogWarning($"RepositionEnemies: mismatch between spawned enemy objects ({spawnedEnemyObjs.Count}) and level data ({enemiesData.Length}).");
    }

    // Surcharge de SpawnPrefabs pour assigner un layer spécifique
    private void SpawnPrefabs(PrefabSpawnData[] prefabsData, string layerName = null)
    {
        foreach (var data in prefabsData)
        {
            if (data == null || data.prefab == null) continue;

            // Crée une copie de position/rotation
            Vector3 pos = data.position;
            Vector3 rot = data.rotation;

            var instance = Instantiate(data.prefab);
            instance.transform.SetPositionAndRotation(
                pos,
                Quaternion.Euler(rot)
            );
            instance.transform.SetParent(levelRoot, true); // true => conserve la position monde


            if (!string.IsNullOrEmpty(layerName))
            {
                int layer = LayerMask.NameToLayer(layerName);
                if (layer != -1)
                    SetLayerRecursively(instance, layer);
            }

            spawnedObjects.Add(instance);
        }
    }

    // Fonction utilitaire pour appliquer un layer à tous les enfants
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    public void ResetCurrentLevel()
    {
        if (currentLevel != null)
            LoadLevel(currentLevel);
    }
}
