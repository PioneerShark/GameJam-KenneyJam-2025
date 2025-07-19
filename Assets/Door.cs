using Unity.VisualScripting;
using UnityEngine;

public class Door : MonoBehaviour
{
    public int indexLocation;
    public DoorOrientation orientation;
    public bool active = true;
    public Vector3 doorEndPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        active = false;
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
