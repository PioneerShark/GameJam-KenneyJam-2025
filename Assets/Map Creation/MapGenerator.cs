using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
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

    public RoomSpawns cellPrefab;
    public Door doorPrefab;
    public GameObject wallPrefab;
    private float cellSizeX;
    private float cellSizeY;
    private float cellGapX;
    private float cellGapY;
    private Queue<int> cellQueue;
    private List<RoomSpawns> spawnedCells;
    private List<GameObject> spawnedAddons;
    private List<Image> cellImages;
    [SerializeField] private Image cellImage;
    [SerializeField] private Transform gridObj;
    [SerializeField] private Sprite inLoc;
    [SerializeField] private Sprite outLoc;


    [SerializeField]
    bool reset = false;

    private void Start()
    {
        minRooms = 7;
        maxRooms = 15;

        cellSizeX = 32f;
        cellSizeY = 24f;

        cellGapX = 20f;
        cellGapY = 20f;

        spawnedCells = new();
        spawnedAddons = new();
        cellImages = new();
        for (int i = 0; i < 100; i++)
        {
            Image sprite = Instantiate(cellImage, gridObj);
            cellImages.Add(sprite);
        }

        SetupDungeon();
        _ = grid.GenerateNewGrid();

    }
    private void Update()
    {
        if (reset)
        {
            reset = false;
            SetupDungeon();

            _ = grid.GenerateNewGrid();

        }
    }

    void SetupDungeon()
    {

        for (int i = 0; i < spawnedCells.Count; i++)
        {
            Destroy(spawnedCells[i].gameObject);
        }
        for (int j = 0; j < spawnedAddons.Count; j++)
        {
            Destroy(spawnedAddons[j]);
        }
        spawnedAddons.Clear();
        spawnedCells.Clear();

        floorPlan = new int[100];
        floorPlanCount = default;
        cellQueue = new Queue<int>();
        endRooms = new List<int>();

        VisitCell(55);

        GenerateDungeon();
    }

    public void RevealLocation(int index, bool entered)
    {
        cellImages[index].color = Color.white;
        if (entered) cellImages[index].sprite = inLoc;
        else cellImages[index].sprite = outLoc;
        foreach (RoomSpawns cell in spawnedCells)
        {
            if (index == cell.index)
            {
                cell.SetLights(entered);
            }
        }
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

        RoomSpawns newCell = Instantiate(cellPrefab, position, Quaternion.identity);

        newCell.value = 1;
        newCell.index = index;

        spawnedCells.Add(newCell);
    }

    private void SetupDoors()
    {
        
        foreach (RoomSpawns cell in spawnedCells)
        {
            int x = cell.index % 10;
            int y = cell.index / 10;
                Vector3 position = new Vector3(x * (cellSizeX + cellGapX), 0f, y * (cellSizeY + cellGapY));
            position = position - new Vector3((cellSizeX + cellGapX) * 5, 0, (cellSizeY + cellGapY) * 5);
            //RevealLocation(cell.index);
            if (floorPlan[cell.index + 1] == 1)
            {
                
                Vector3 endPos = position + new Vector3(cellSizeX/2,0,0);
                Door door = Instantiate(doorPrefab, endPos, Quaternion.identity);
                door.indexLocation = cell.index;
                door.orientation = DoorOrientation.Right;
                door.transform.Rotate(0, 90, 0);
                door.doorEndPosition = endPos + new Vector3(cellGapX+4, 0, 0);
                door.roomCenter = position;
                door.roomDistX = cellSizeX + cellGapX;
                door.roomDistY = cellSizeY + cellGapY;
                spawnedAddons.Add(door.gameObject);
                cell.doors.Add(door);
                cell.spawnLocations.Add(door.spawnLocation);
                if (cell.index == 55) door.active = true;
            }
            else
            {
                Vector3 endPos = position + new Vector3(cellSizeX / 2, 0, 0);
                GameObject wall = Instantiate(wallPrefab, endPos, Quaternion.identity);
                wall.transform.Rotate(0, 270, 0);
                spawnedAddons.Add(wall);
            }

            if (floorPlan[cell.index - 1] == 1)
            {

                Vector3 endPos = position - new Vector3(cellSizeX / 2, 0, 0);
                Door door = Instantiate(doorPrefab, endPos, Quaternion.identity);
                door.indexLocation = cell.index;
                door.orientation = DoorOrientation.Left;
                door.transform.Rotate(0, 270, 0);
                door.doorEndPosition = endPos - new Vector3(cellGapX + 4, 0, 0);
                door.roomCenter = position;
                door.roomDistX = cellSizeX + cellGapX;
                door.roomDistY = cellSizeY + cellGapY;
                spawnedAddons.Add(door.gameObject);
                cell.doors.Add(door);
                cell.spawnLocations.Add(door.spawnLocation);
                if (cell.index == 55) door.active = true;

            }
            else
            {
                Vector3 endPos = position - new Vector3(cellSizeX / 2, 0, 0);
                GameObject wall = Instantiate(wallPrefab, endPos, Quaternion.identity);
                wall.transform.Rotate(0, 90, 0);
                spawnedAddons.Add(wall);

            }

            if (floorPlan[cell.index + 10] == 1)
            {

                Vector3 endPos = position + new Vector3(0, 0, cellSizeY / 2);
                Door door = Instantiate(doorPrefab, endPos, Quaternion.identity);
                door.indexLocation = cell.index;
                door.orientation = DoorOrientation.Up;
                door.doorEndPosition = endPos + new Vector3(0, 0, cellGapY + 4);
                door.roomCenter = position;
                door.roomDistX = cellSizeX + cellGapX;
                door.roomDistY = cellSizeY + cellGapY;
                spawnedAddons.Add(door.gameObject);
                cell.doors.Add(door);
                cell.spawnLocations.Add(door.spawnLocation);
                if (cell.index == 55) door.active = true;
            }
            else
            {
                Vector3 endPos = position + new Vector3(0, 0, cellSizeY / 2);
                GameObject wall = Instantiate(wallPrefab, endPos, Quaternion.identity);
                wall.transform.Rotate(0, 180, 0);
                spawnedAddons.Add(wall);
            }

            if (floorPlan[cell.index - 10] == 1)
            {

                Vector3 endPos = position - new Vector3( 0, 0, cellSizeY / 2);
                Door door = Instantiate(doorPrefab, endPos, Quaternion.identity);
                door.indexLocation = cell.index;
                door.orientation = DoorOrientation.Down;
                door.transform.Rotate(0, 180, 0);
                door.doorEndPosition = endPos - new Vector3(0, 0, cellGapY + 4);
                door.roomCenter = position;
                door.roomDistX = cellSizeX + cellGapX;
                door.roomDistY = cellSizeY + cellGapY;
                spawnedAddons.Add(door.gameObject);
                cell.doors.Add(door);
                cell.spawnLocations.Add(door.spawnLocation);
                if (cell.index == 55) door.active = true;
            }
            else
            {
                Vector3 endPos = position - new Vector3(0, 0, cellSizeY / 2);
                GameObject wall = Instantiate(wallPrefab, endPos, Quaternion.identity);
                spawnedAddons.Add(wall);

            }
            if (cell.index == 55)
            {
                cell.MakeSafe();
                RevealLocation(55, true);
            }
        }
    }
}
