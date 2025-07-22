using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Framework;

public class RoomSpawns : MonoBehaviour
{
    [SerializeField] private HealthComponent enemyType;
    [SerializeField] int enemyCountMax;
    [SerializeField] int enemyCountMin;
    [SerializeField] int waveCount;
    [SerializeField] float waveDuration;
    [SerializeField] float spawnDelay;
    public List<Transform> spawnLocations;
    public List<Light> lights;
    [HideInInspector] public List<Door> doors;
    private List<HealthComponent> spawnedEnemies;
    [HideInInspector] public int index;
    [HideInInspector] public int value;
    public RoomStatus roomStatus;
    //private bool running = false;
    private EventService EventService;

    private CancellationTokenSource _ownerSceneTokenSource;
    public CancellationToken ownerSceneToken => _ownerSceneTokenSource.Token;
    private Scene _ownerScene;

    private void Awake()
    {
        EventService = Game.GetService<EventService>();
        roomStatus = RoomStatus.Inactive;
        spawnedEnemies = new();
        spawnLocations = new();
        doors = new();

        _ownerScene = SceneManager.GetActiveScene();
        _ownerSceneTokenSource = new CancellationTokenSource();

        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    public void SetLights(bool setActive)
    {
        foreach (Light light in lights)
        {
            light.enabled = setActive;
        }
    }

    private void Update()
    {
        switch (roomStatus)
        {
            case RoomStatus.Primed:
                ShutDoors();
                _ = HandleWave(ownerSceneToken);
                break;
            case RoomStatus.Inactive:
                CheckCollisions();
                break;
            case RoomStatus.Completed:
                List<HealthComponent> tempSpawned = new();
                for (int i = 0; i < spawnedEnemies.Count; i++)
                {
                    if (spawnedEnemies[i] != null)
                    {
                        tempSpawned.Add(spawnedEnemies[i]);
                    }
                }
                spawnedEnemies = tempSpawned;
                if (spawnedEnemies.Count <= 0)
                {
                    EndWaves();
                }
                break;
            case RoomStatus.InProgress:
                tempSpawned = new();
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

    private async UniTask HandleWave(CancellationToken token)
    {
        roomStatus = RoomStatus.InProgress;
        int enemyCountScaleMin = KennyGameManager.Instance.GetPower() / 500;
        int enemyCountScale = KennyGameManager.Instance.GetPower() / 300;
        int waveScale = KennyGameManager.Instance.GetPower() / 500;
        int trueWaveCount = Random.Range(waveCount, waveCount + waveScale);
        for (int i = 0; i < waveCount; i++)
        {

            int enemyCount = Random.Range(enemyCountMin + enemyCountScaleMin, enemyCountMax + 1 + enemyCountScale);
            for (int j = 0; j < enemyCount; j++)
            {
                await UniTask.Delay((int)(spawnDelay * 1000f), cancellationToken: token);
                token.ThrowIfCancellationRequested();
                HealthComponent enemy = Instantiate(enemyType, spawnLocations[Random.Range(0, spawnLocations.Count)]);
                int healthValue = (KennyGameManager.Instance.GetPower() + 100) / 100;
                enemy.SetMaxHealth(healthValue);
                spawnedEnemies.Add(enemy);
            }

            //Debug.Log($"Spawned {enemyCount} enemies in Scene: {SceneManager.GetActiveScene().name}");
            await UniTask.Delay((int)(waveDuration * 1000f), cancellationToken: token);
        }
        await UniTask.Yield(PlayerLoopTiming.Update, token);
        roomStatus = RoomStatus.Completed;
    }

    public void MakeSafe()
    {
        roomStatus = RoomStatus.Completed;
        EndWaves();
    }

    private void EndWaves()
    {
        foreach (Door door in doors)
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

    private void OnSceneUnloaded(Scene scene)
    {
        if (scene == _ownerScene)
        {
            _ownerSceneTokenSource.Cancel();
            _ownerSceneTokenSource.Dispose();
        }
    }

    private void Oestroy()
    {
        _ownerSceneTokenSource?.Cancel();
        _ownerSceneTokenSource?.Dispose();
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
}
