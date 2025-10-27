using UnityEngine;

[System.Serializable]
public class PrefabSpawnData
{
    public GameObject prefab;
    public Vector3 position;
    public Vector3 rotation; // Euler angles
}

[CreateAssetMenu(fileName = "NewLevel", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelName;

    [Header("Spawn du joueur")]
    public Vector3 spawnPosition;
    public Vector3 spawnRotation; // Euler angles

    [Header("Éléments du niveau")]
    public PrefabSpawnData[] walls;
    public PrefabSpawnData[] enemies;
    public PrefabSpawnData[] items;
}
