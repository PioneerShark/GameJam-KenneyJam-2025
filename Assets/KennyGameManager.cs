using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static Framework;
using static UnityEngine.Rendering.DebugUI;

public class KennyGameManager : MonoBehaviour
{
	// Services
	private EventService EventService;
	public static KennyGameManager Instance;
	[SerializeField] KennyPlayerController playerCon;
	[SerializeField] private TextMeshProUGUI healthText, powerText;

	// Components
	private CameraComponent _CameraComponent;

	int power = 0;

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		EventService = Game.GetService<EventService>();
		TryGetComponent<CameraComponent>(out _CameraComponent);

		EventService.Connect("SetCameraPosition", OnSetCameraPosition);
        EventService.Connect("AddPower", AddPower);
        powerText.text = "Power: " + power;
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
	public int GetPower()
	{
		return power;
	}

	void AddPower()
	{
		power += 10;
		Debug.Log(power);
        playerCon.UpdatePower(power);
        powerText.text = "Power: " + power;
		switch (power)
		{
			case > 999:
				powerText.color = Color.cyan;
				break;
            case > 799:
                powerText.color = Color.blue;
                break;
            case > 599:
                powerText.color = Color.red;
                break;
            case > 399:
                powerText.color = Color.yellow;
                break;
            case > 199:
                powerText.color = Color.green;
                break;

        }
    }
	public void UpdateHealth(int value)
	{
		healthText.text = "Health: " + value;
	}
	
}
