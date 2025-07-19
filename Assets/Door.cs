using Unity.VisualScripting;
using UnityEngine;

public class Door : MonoBehaviour
{
    public int indexLocation;
    public DoorOrientation orientation;
    public bool active = false;
    private BoxCollider triggerCollider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            RaycastHit hit;
            if (Physics.BoxCast(triggerCollider.bounds.center, 
                triggerCollider.gameObject.transform.localScale * 0.5f,
                transform.forward, out hit, transform.rotation,
                1f, Physics2D.GetLayerCollisionMask(gameObject.layer))){

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
