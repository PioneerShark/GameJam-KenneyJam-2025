using Unity.VisualScripting;
using UnityEngine;

public class Door : MonoBehaviour
{
    public int indexLocation;
    public DoorOrientation orientation;
    public bool active = false;
    public Vector3 doorEndPosition;
    public Transform spawnLocation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
