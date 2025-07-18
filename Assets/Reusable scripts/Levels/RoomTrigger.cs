using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class RoomTrigger : MonoBehaviour
{
    UnityEvent<Room> roomTriggerEvent;
    BoxCollider[] colliders;
    Room roomID;

    private void Awake()
    {
        colliders = GetComponentsInChildren<BoxCollider>();
    }
    private void Update()
    {
        CheckCollisions();
    }

    private void CheckCollisions()
    {
        foreach (BoxCollider col in colliders)
        {
            RaycastHit hit;
            if (Physics.BoxCast(col.bounds.center, col.gameObject.transform.localScale*0.5f, transform.forward, out hit, transform.rotation,1f, Physics2D.GetLayerCollisionMask(gameObject.layer)))
            {
                    roomTriggerEvent.Invoke(roomID);
                    break;            
            }
        }
        
    }

    public void AssignEvent(UnityAction<Room> action, Room newRoomID)
    {
        if (roomTriggerEvent == null)
        {
            roomTriggerEvent = new UnityEvent<Room>();
        }
        roomTriggerEvent.AddListener(action);
        roomID = newRoomID;
    }
}
