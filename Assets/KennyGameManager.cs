using Unity.VisualScripting;
using UnityEngine;
using static Framework;

public class KennyGameManager : MonoBehaviour
{
	// Services
	private EventService EventService;

	// Components
	private CameraComponent _CameraComponent;

	void Awake()
	{
		EventService = Game.GetService<EventService>();
		TryGetComponent<CameraComponent>(out _CameraComponent);

		EventService.Connect("SetCameraPosition", OnSetCameraPosition);
	}

    void Start()
    {
		EventService.Fire("SetCameraPosition", Vector3.zero);
    }

    void OnSetCameraPosition(object data)
    {
		if (EventService.ReadValue(data, out Vector3 position))
		{
			//Debug.Log($"Success: {position}");
			this.transform.position = position;
		}
		else
		{
			Debug.Log("Failed");
		}
    }
}
