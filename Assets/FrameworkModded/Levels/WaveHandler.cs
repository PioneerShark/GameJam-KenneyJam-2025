using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;


public class WaveHandler : MonoBehaviour
{
    public List<Room> rooms;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (Room room in rooms)
        {
            room.roomTrigger.AssignEvent(TriggerRoom, room);
            room.walls.SetActive(false);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Room room in rooms)
        {
            UpdateRoom(room);
        }
    }

    public void DecrementWaveCount(Room room)
    {
        room.enemyCount--;
    }

    public void TriggerRoom(Room room)
    {
        if (room.roomStatus == RoomStatus.Primed)
        {
            room.roomStatus = RoomStatus.InProgress;
            room.walls.SetActive(true);
        }
        
    }
    void UpdateRoom(Room room)
    {
        if (room.roomStatus != RoomStatus.InProgress)
        {
            return;
        }

        if (room.enemyCount<=0 && room.currentWave >= room.waves.Count)
        {
            
            RoomCleared(room);

        }

        // spawn enemy and tie gameobj to it;
        if (room.enemyCount <= 0 && room.currentWave < room.waves.Count)
        {
            room.currentWave++;
            room.enemyCount = room.waves[room.currentWave].units.Count;
            StartCoroutine(StartWave(room));
            
        }
    }
    void RoomCleared(Room room)
    {
        room.roomStatus = RoomStatus.Completed;
        room.walls.SetActive(false);
    }
    private IEnumerator StartWave(Room room)
    {
        yield return new WaitForSeconds(room.waveDelay);
        for (int i = 0; i < room.waves[room.currentWave].units.Count ; i++)
        {
            int y = i % room.spawnLocations.Count;
            HealthComponent enemy = Instantiate(room.waves[room.currentWave].units[i], room.spawnLocations[y].position, room.spawnLocations[y].rotation);
            enemy.AssignDeathEvent(DecrementWaveCount, room);
            yield return new WaitForSeconds(0.5f);
        }
        
        yield break;
    }
}
public enum RoomStatus
{
    Inactive,
    Primed,
    InProgress,
    Completed
}
[System.Serializable]
public struct Wave<T>
{
    public List<T> units;
}
[System.Serializable]
public class Room
{
    bool randomSpawns;
    public RoomStatus roomStatus;
    public List<Wave<HealthComponent>> waves;
    public RoomTrigger roomTrigger;
    public List<Transform> spawnLocations;
    public GameObject walls;
    [HideInInspector]
    public int enemyCount = 0;
    [HideInInspector]
    public int currentWave = -1;
    public float waveDelay = 1f;
}