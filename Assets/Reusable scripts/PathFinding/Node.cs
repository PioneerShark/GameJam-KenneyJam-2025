using UnityEngine;

public class Node : IHeapItem<Node>
{
    public bool walkable;
    public Vector3 worldPosition;

    public int gCost;
    public int hCost;
    public int movementPenalty;

    public int gridX;
    public int gridY;
    public Node parent;
    int heapIndex;

    public Node(bool _walkable, Vector3 _worldPosition, int _gridX,  int _gridY, int _movementPenalty)
    {
        walkable = _walkable;
        worldPosition = _worldPosition;
        gridX = _gridX;
        gridY = _gridY;
        movementPenalty = _movementPenalty;
    }

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }
    public int HeapIndex
    {
        get 
        {
            return heapIndex;
        }
        set 
        {
            heapIndex = value;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;
    }
}
