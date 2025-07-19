using Unity.VisualScripting;
using UnityEngine;
using static Framework;

public class Door : MonoBehaviour
{
    public int indexLocation;
    public DoorOrientation orientation;
    public bool active = false;
    public Vector3 doorEndPosition;
    public Transform spawnLocation;
    private EventService EventService;
    public Vector3 roomCenter;
    public float roomDistX;
    public float roomDistY;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        EventService = Game.GetService<EventService>();
    }
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1f);
            foreach (Collider col in hitColliders)
            {
                if (col.CompareTag("Player"))
                {
                    Vector3 offset = new();
                    switch (orientation)
                    {
                        case DoorOrientation.Left:
                            offset = new Vector3(-roomDistX, 0, 0);
                            break;
                        case DoorOrientation.Right:
                            offset = new Vector3(roomDistX, 0, 0);
                            break;
                        case DoorOrientation.Up:
                            offset = new Vector3(0, 0, roomDistY);
                            break;
                        case DoorOrientation.Down:
                            offset = new Vector3(0, 0, -roomDistY);
                            break;
                    }
                    Vector3 pos = roomCenter + offset;
                    EventService.Fire("SetCameraPosition", pos);
                    col.transform.position = doorEndPosition;
                }
            }
        }
    }
}
public enum DoorOrientation
{
    Left,
    Right,
    Up,
    Down
}
