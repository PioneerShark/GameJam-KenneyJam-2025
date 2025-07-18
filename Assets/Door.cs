using UnityEngine;

public class Door : MonoBehaviour
{
    public int indexLocation;
    public DoorOrientation orientation;
    public bool active;
    private Collider collider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
public enum DoorOrientation
{
    Left,
    Right,
    Up,
    Down
}
