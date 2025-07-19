using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class RoomSpawns : MonoBehaviour
{
    [SerializeField] private HealthComponent enemyType;
    [SerializeField] int enemyCountMax;
    [SerializeField] int enemyCountMin;
    [SerializeField] int waveCount;
    [SerializeField] float waveDuration;
    [SerializeField] float spawnDelay;
    [SerializeField] List<Transform> spawnLocations;
    [HideInInspector] public List<Door> doors;
    private List<HealthComponent> spawnedEnemies;
    [HideInInspector] public int index;
    [HideInInspector] public int value;
    public RoomStatus roomStatus;
    private void Awake()
    {
        roomStatus = RoomStatus.Inactive;
    }

    private void Update()
    {
        switch (roomStatus) {
            case RoomStatus.Primed:
                ShutDoors();
                HandleWave();
                break;
            case RoomStatus.Inactive:

                break;
            case RoomStatus.Completed:
                if (spawnedEnemies.Count <= 0)
                {
                    EndWaves();
                }
                break;

        }
    }
    private async UniTask HandleWave()
    {
        
        for (int i = 0; i < waveCount; i++)
        {
            int enemyCount = Random.Range(enemyCountMin, enemyCountMax);
            for (int j = 0; j < enemyCount; j++)
            {
                await UniTask.Delay((int)spawnDelay * 1000);
                Vector3 spawnLocation = spawnLocations[Random.Range(0, spawnLocations.Count-1)].position;
                HealthComponent enemy = Instantiate(enemyType, spawnLocation, Quaternion.identity);
                spawnedEnemies.Add(enemy);
            }
            await UniTask.Delay((int)waveDuration * 1000);
        }
        await UniTask.Yield();
    }

    private void EndWaves()
    {
        foreach (Door door  in doors)
        {
            door.active = true;
        }
    }
    private void ShutDoors()
    {
        foreach (Door door in doors)
        {
            door.active = false;
        }
    }

}
