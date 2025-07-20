using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static Framework;
using static UnityEngine.Rendering.DebugUI;

struct PlaySFXInfo
{
	public string SFXName;
	public Vector3 SFXPosition;

	public PlaySFXInfo(string SetSFXName, Vector3 SetSFXPosition = default)
	{
		SFXName = SetSFXName;
		SFXPosition = SetSFXPosition;
	}
}

public class KennyGameManager : MonoBehaviour
{
	// Services
	private EventService EventService;

	// Components
	private CameraComponent _CameraComponent;

	// Variables
	public static KennyGameManager Instance;
	[SerializeField] KennyPlayerController playerCon;
	[SerializeField] MapGenerator map;
	[SerializeField] private TextMeshProUGUI healthText, powerText;

	[SerializeField] private AudioClip[] audioClips;
	[SerializeField] private AudioClip musicClip;
	[SerializeField] private AudioSource musicPlayer;

	int power = 0;

	void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
		}
		else
		{
			Instance = this;
		}
		EventService = Game.GetService<EventService>();
		TryGetComponent<CameraComponent>(out _CameraComponent);
		TryGetComponent<AudioSource>(out musicPlayer);

		EventService.Connect("SetCameraPosition", OnSetCameraPosition);
		EventService.Connect("AddPower", AddPower);
		EventService.Connect("CombatStatus", CombatStatus);
		powerText.text = "Power: " + power;

		// Audio Events
		EventService.Connect("PlaySFX", OnPlaySFX);
	}

	void Start()
	{
		EventService.Fire("SetCameraPosition", Vector3.zero);

		if (musicClip != null)
		{
			musicPlayer.clip = musicClip;
			musicPlayer.Play();
		}
	}

	void PlaySFX(AudioClip audioClip, Vector3 position, float minVolume = 1, float maxVolume = 1, float pitch = 1, int maxPitchVariance = 0)
	{
		AudioSource audioSource = new GameObject(audioClip.name, typeof(AudioSource)).GetComponent<AudioSource>();
		audioSource.clip = audioClip;
		audioSource.transform.parent = null;
		audioSource.transform.position = position;
		audioSource.playOnAwake = false;

		audioSource.pitch = pitch;
		int pitchVariance = Random.Range(0, maxPitchVariance + 1);
        for (int i = 0; i < pitchVariance; i++)
		{
			audioSource.pitch *= 1.059463f;
		}

		audioSource.volume = Random.Range(minVolume, maxVolume);

		audioSource.Play();
		Destroy(audioSource.gameObject, audioClip.length);
	}

	void OnPlaySFX(object data)
	{
		if (EventService.ReadValue(data, out PlaySFXInfo playSFXInfo))
		{
			Debug.Log($"Success: {playSFXInfo.SFXName}");

			switch (playSFXInfo.SFXName)
			{
				case "RogueAttack":
					PlaySFX(audioClips[Random.Range(0, 5)], playerCon.transform.position, 0.4f, 0.6f, 1, 5);
					break;
				case "PlayerHurt":
					PlaySFX(audioClips[Random.Range(6, 9)], playSFXInfo.SFXPosition, 0.3f, 0.5f, 1, 5);
					PlaySFX(audioClips[9], playSFXInfo.SFXPosition, 0.1f, 0.2f, 0.9f, 5);
					break;
				case "EnemyAttack":
					PlaySFX(audioClips[5], playSFXInfo.SFXPosition, 0.4f, 0.6f, 1, 3);
					break;
				case "EnemyHurt":
					PlaySFX(audioClips[9], playSFXInfo.SFXPosition, 0.1f, 0.2f, 1.333f, 5);
					break;
				case "EnemyDied":
					PlaySFX(audioClips[Random.Range(10, 13)], playSFXInfo.SFXPosition, 0.1f, 0.3f, 1, 5);
					break;
				case "DoorInteract":
					PlaySFX(audioClips[Random.Range(10, 13)], playSFXInfo.SFXPosition, 0.1f, 0.3f, 1, 5);
					break;
			}
		}
		else
		{
			Debug.Log("Failed");
		}
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

	void CombatStatus(object data)
	{
		if (EventService.ReadValue(data, out bool status))
		{
			if (status)
			{
				this.GetComponent<AudioLowPassFilter>().enabled = false;
			}
			else
			{
				this.GetComponent<AudioLowPassFilter>().enabled = true;
			}
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

<<<<<<< Updated upstream
        }
    }
    public void UpdateWorld(int index, bool enter)
    {
		map.RevealLocation(index, enter);
    }
    public void UpdateHealth(int value)
=======
		}
	}
	public void UpdateHealth(int value)
>>>>>>> Stashed changes
	{
		healthText.text = "Health: " + value;
	}

}
