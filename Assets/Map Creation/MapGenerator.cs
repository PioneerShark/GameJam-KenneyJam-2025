using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;
public class MapGenerator : MonoBehaviour
{
    [SerializeField]
    private GridPi grid;
    private int[] floorPlan;

    private int floorPlanCount;
    private int minRooms;
    private int maxRooms;
    private List<int> endRooms;

    private int exitRoomIndex;

    public Cell cellPrefab;
    public Door doorPrefab;
    public GameObject wallPrefab;
    private float cellSizeX;
    private float cellSizeY;
    private float cellGapX;
    private float cellGapY;
    private Queue<int> cellQueue;
    private List<Cell> spawnedCells;

    [SerializeField]
    bool reset = false;

    private void Start()
    {
        minRooms = 7;
        maxRooms = 15;

        cellSizeX = 32f;
        cellSizeY = 24f;

        cellGapX = 32f;
        cellGapY = 24f;

        spawnedCells = new();

        SetupDungeon();
        grid.GenerateNewGrid();

    }
    private void Update()
    {
        if (reset)
        {
            reset = false;
            SetupDungeon();

            grid.GenerateNewGrid();

        }
    }

    void SetupDungeon()
    {

        for (int i = 0; i < spawnedCells.Count; i++)
        {
            Destroy(spawnedCells[i].gameObject);
        }
        spawnedCells.Clear();

        floorPlan = new int[100];
        floorPlanCount = default;
        cellQueue = new Queue<int>();
        endRooms = new List<int>();

        VisitCell(55);

        GenerateDungeon();
    }
    void GenerateDungeon()
    {
        while (cellQueue.Count > 0)
        {
            int index = cellQueue.Dequeue();
            int x = index % 10;

            bool created = false;
            if (x > 1) created |= VisitCell(index - 1);
            if (x < 9) created |= VisitCell(index + 1);
            if (index > 20) created |= VisitCell(index - 10);
            if (index < 70) created |= VisitCell(index + 10);

            if (!created)
            {
                endRooms.Add(index);
            }

        }
        if (floorPlanCount < minRooms)
        {
            SetupDungeon();
            return;
        }
        SetupDoors();
        SetupSpecialRooms();
    }

    void SetupSpecialRooms()
    {

    }

    private int GetNeighbourCount(int index)
    {
        return floorPlan[index + 1] + floorPlan[index - 1] + floorPlan[index + 10] + floorPlan[index - 10];
    }

    private bool VisitCell(int index)
    {
        if (floorPlan[index] != 0 || GetNeighbourCount(index) > 1 || floorPlanCount > maxRooms || UnityEngine.Random.value < 0.5f)
            return false;

        cellQueue.Enqueue(index);
        floorPlan[index] = 1;
        floorPlanCount++;

        SpawnRoom(index);

        return true;
    }

    private void SpawnRoom(int index)
    {
        /*foreach (Cell cell in spawnedCells)
        {
            if (index == cell.index) return;
        }*/
        int x = index % 10;
        int y = index / 10;
        Vector3 position = new Vector3(x * (cellSizeX + cellGapX), 0f, y * (cellSizeY + cellGapY));
        position = position - new Vector3((cellSizeX + cellGapX) * 5, 0, (cellSizeY + cellGapY) * 5);

        Cell newCell = Instantiate(cellPrefab, position, Quaternion.identity);

        newCell.value = 1;
        newCell.index = index;

        spawnedCells.Add(newCell);
    }

    private void SetupDoors()
    {
        
        foreach (Cell cell in spawnedCells)
        {
            int x = cell.index % 10;
            int y = cell.index / 10;
            Vector3 position = new Vector3(x * (cellSizeX + cellGapX), 0f, y * (cellSizeY + cellGapY));
            position = position - new Vector3((cellSizeX + cellGapX) * 5, 0, (cellSizeY + cellGapY) * 5);
            if (floorPlan[cell.index + 1] == 1)
            {
                
                Vector3 endPos = position + new Vector3(cellSizeX/2+2,0,-2);
                Door door = Instantiate(doorPrefab, endPos, Quaternion.identity);
                door.orientation = DoorOrientation.Right;
                door.transform.Rotate(0, 90, 0);
            }
            else
            {
                Vector3 endPos = position + new Vector3(cellSizeX / 2 + 2, 0, -2);
                GameObject wall = Instantiate(wallPrefab, endPos, Quaternion.identity);
                wall.transform.Rotate(0, 90, 0);
            }

            if (floorPlan[cell.index - 1] == 1)
            {

                Vector3 endPos = position - new Vector3(cellSizeX / 2 - 2, 0, 2);
                Door door = Instantiate(doorPrefab, endPos, Quaternion.identity);
                door.orientation = DoorOrientation.Left;
                door.transform.Rotate(0, 90, 0);
            }
            else
            {
                Vector3 endPos = position - new Vector3(cellSizeX / 2 - 2, 0, 2);
                GameObject wall = Instantiate(wallPrefab, endPos, Quaternion.identity);
                wall.transform.Rotate(0, 90, 0);
            }

            if (floorPlan[cell.index + 10] == 1)
            {

                Vector3 endPos = position + new Vector3(2, 0, cellSizeY / 2 - 2);
                Door door = Instantiate(doorPrefab, endPos, Quaternion.identity);
                door.orientation = DoorOrientation.Up;
            }
            else
            {
                Vector3 endPos = position + new Vector3(2, 0, cellSizeY / 2 - 2);
                GameObject wall = Instantiate(wallPrefab, endPos, Quaternion.identity);
            }

            if (floorPlan[cell.index - 10] == 1)
            {

                Vector3 endPos = position - new Vector3( - 2, 0, cellSizeY / 2 +2);
                Door door = Instantiate(doorPrefab, endPos, Quaternion.identity);
                door.orientation = DoorOrientation.Down;
            }
            else
            {
                Vector3 endPos = position - new Vector3( -2, 0, cellSizeY / 2 + 2);
                GameObject wall = Instantiate(wallPrefab, endPos, Quaternion.identity);
            }
        }
    }
}
