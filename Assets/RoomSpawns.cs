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
    public List<Transform> spawnLocations;
    [HideInInspector] public List<Door> doors;
    private List<HealthComponent> spawnedEnemies;
    [HideInInspector] public int index;
    [HideInInspector] public int value;
    public RoomStatus roomStatus;
    private bool running = false;
    private void Awake()
    {
        roomStatus = RoomStatus.Inactive;
        spawnedEnemies = new();
        spawnLocations = new();
        doors = new();
    }

    private void Update()
    {
        switch (roomStatus) {
            case RoomStatus.Primed:
                
                ShutDoors();
                HandleWave();
                break;
            case RoomStatus.Inactive:
                CheckCollisions();
                break;
            case RoomStatus.Completed:
                if (spawnedEnemies.Count <= 0)
                {
                    EndWaves();
                }
                break;
            case RoomStatus.InProgress:
                List<HealthComponent> tempSpawned = new();
                for (int i = 0; i < spawnedEnemies.Count; i++)
                {
                    if (spawnedEnemies[i] != null)
                    {
                        tempSpawned.Add(spawnedEnemies[i]);
                    }
                }
                spawnedEnemies = tempSpawned;
                break;

        }
    }
    private async UniTask HandleWave()
    {
        roomStatus = RoomStatus.InProgress;
        for (int i = 0; i < waveCount; i++)
        {
            int enemyCount = Random.Range(enemyCountMin, enemyCountMax+1);
            for (int j = 0; j < enemyCount; j++)
            {
                await UniTask.Delay((int)(spawnDelay * 1000f));
                HealthComponent enemy = Instantiate(enemyType, spawnLocations[Random.Range(0, spawnLocations.Count)]);
                spawnedEnemies.Add(enemy);
            }
            await UniTask.Delay((int)(waveDuration * 1000f));
        }
        await UniTask.Yield();
        roomStatus = RoomStatus.Completed;
    }

    public void MakeSafe()
    {
        roomStatus = RoomStatus.Completed;
        EndWaves();
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
    private void CheckCollisions()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 20f);
        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                roomStatus = RoomStatus.Primed;
            }
        }
    }

}
