using TMPro;
using UnityEngine;
using static Framework;
using Cysharp.Threading.Tasks;

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
	private AudioLowPassFilter _AudioLowPassFilter;

	// Variables
	public static KennyGameManager Instance;
	[SerializeField] private KennyPlayerController _playerController;
	[SerializeField] private MapGenerator _mapGenerator;
	[SerializeField] private TextMeshProUGUI healthText, powerText;

	[SerializeField] private AudioClip[] audioClips;
	[SerializeField] private AudioSource musicPlayerMelodic;
	[SerializeField] private AudioSource musicPlayerPercussion;
	[SerializeField] private bool lowPassCutoffFrequency = true;
	[SerializeField] private float lowPassCutoffFrequencyMax = 22000f;
	[SerializeField] private float lowPassCutoffFrequencyMin = 2000f;
	[SerializeField] private float lowPassCutoffFrequencyLoseSpeed = 1.5f;
	[SerializeField] private float lowPassCutoffFrequencyGainSpeed = 0.5f;
	[SerializeField] private float lowPassCutoffFrequencyLoseDelay = 1.25f;
	[SerializeField] private float lowPassCutoffFrequencyGainDelay = 0.5f;
	[SerializeField] private float volumeMax = 0.333f;
	[SerializeField] private float volumeLoseSpeed = 0.5f;
	[SerializeField] private float volumeGainSpeed = 0.5f;

	private int loopLengthMelodic;
	private int loopPositionMelodic;
	private int loopPositionPercussion;
	private int sampleStart = 0; //(int) (5953500 * 0.25f);

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
		TryGetComponent<AudioLowPassFilter>(out _AudioLowPassFilter);

		// Events
		EventService.Connect("SetCameraPosition", OnSetCameraPosition);
		EventService.Connect("AddPower", AddPower);
		EventService.Connect("CombatStatus", CombatStatus);
		EventService.Connect("TeleportPlayer", OnTeleportPlayer);
		EventService.Connect("PlaySFX", OnPlaySFX);

		powerText.text = "Power: " + power;

		Cursor.visible = false;
	}

	void Start()
	{
		EventService.Fire("SetCameraPosition", Vector3.zero);

		loopLengthMelodic = musicPlayerMelodic.clip.samples;

		OSTMatchPitch();
		_ = TryOSTPlay();
		musicPlayerMelodic.timeSamples = sampleStart;
		musicPlayerPercussion.timeSamples = sampleStart;

		if (_playerController == null)
		{
			GameObject.FindGameObjectWithTag("Player").TryGetComponent<KennyPlayerController>(out _playerController);
		}
	}

	void Update()
	{
		musicPlayerMelodic.volume = Mathf.Lerp(musicPlayerMelodic.volume, volumeMax, volumeGainSpeed * Time.deltaTime);

		if (lowPassCutoffFrequency)
		{
			// In Combat
			_AudioLowPassFilter.cutoffFrequency = Mathf.Lerp(_AudioLowPassFilter.cutoffFrequency, lowPassCutoffFrequencyMax, lowPassCutoffFrequencyGainSpeed * Time.deltaTime);
			musicPlayerPercussion.volume = Mathf.Lerp(musicPlayerPercussion.volume, volumeMax, volumeGainSpeed * Time.deltaTime);
		}
		else
		{
			// Out of Combat
			_AudioLowPassFilter.cutoffFrequency = Mathf.Lerp(_AudioLowPassFilter.cutoffFrequency, lowPassCutoffFrequencyMin, lowPassCutoffFrequencyLoseSpeed * Time.deltaTime);
			musicPlayerPercussion.volume = Mathf.Lerp(musicPlayerPercussion.volume, 0, volumeLoseSpeed * Time.deltaTime);
		}

		OSTMatchPitch();

		int melodicSamples = musicPlayerMelodic.timeSamples;
		int percussionSamples = musicPlayerPercussion.timeSamples;

		loopPositionMelodic = melodicSamples;
		loopPositionPercussion = percussionSamples;

		// Check if the melodic loop has completed
		if (loopPositionMelodic == 0)
		{
			musicPlayerPercussion.timeSamples = 0;
		}

		// Update pitch
		OSTMatchPitch();
	}

	async UniTask TryOSTPlay(int maxAttempts = 64)
	{
		int currentAttempts = 0;
		while ((musicPlayerMelodic.isPlaying == false || musicPlayerPercussion.isPlaying == false) && currentAttempts <= maxAttempts)
		{
			//Debug.Log("Trying to play OST!");
			currentAttempts++;
			musicPlayerMelodic.Play();
			musicPlayerPercussion.Play();
			await UniTask.Yield();
		}

		if (currentAttempts >= maxAttempts)
		{
			Debug.Log("Failed to play OST!");
			return;
		}

		Debug.Log($"Playing OST after {currentAttempts} attempts.");
	}

	void OSTMatchPitch()
	{
		if (musicPlayerPercussion.pitch != musicPlayerMelodic.pitch)
		{
			Debug.Log("Percussion Pitch matched to Melodic Pitch!");
			musicPlayerPercussion.pitch = musicPlayerMelodic.pitch;
			musicPlayerPercussion.timeSamples = musicPlayerMelodic.timeSamples;
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
			switch (playSFXInfo.SFXName)
			{
				case "RogueAttack":
					PlaySFX(audioClips[Random.Range(0, 5)], _playerController.transform.position, 0.2f, 0.3f, 1, 5);
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

	void OnTeleportPlayer(object data)
	{
		if (EventService.ReadValue(data, out Vector3 teleportPosition))
		{
			//Debug.Log($"Teleport player to {teleportPosition}");
			_playerController.gameObject.transform.position = teleportPosition;
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
			//Debug.Log($"Combat Status: {status}");
			_ = QueueCombatStatusChange(status, status == true ? lowPassCutoffFrequencyGainDelay : lowPassCutoffFrequencyLoseDelay);
		}
		else
		{
			Debug.Log("Failed");
		}
	}

	async UniTask QueueCombatStatusChange(bool status, float delay = 1)
	{
		if (lowPassCutoffFrequency == status) return;
		float startTime = Time.time;
		while (Time.time < startTime + delay)
		{
			await UniTask.Yield();
		}
		lowPassCutoffFrequency = status;
	}

	public int GetPower()
	{
		return power;
	}

	void AddPower()
	{
		power += 10;
		_playerController.UpdatePower(power);
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

	public void UpdateWorld(int index, bool enter)
	{
		_mapGenerator.RevealLocation(index, enter);
		if (enter)
		{
			//Debug.Log("Open Door Sound");
			PlaySFX(audioClips[13], _playerController.gameObject.transform.position, 0.5f, 0.6f, 1, 3);
		}
	}

	public void UpdateHealth(int value)
	{
		healthText.text = "Health: " + value;
	}

    void OnDestroy()
    {
        // Events
		EventService.Disconnect("SetCameraPosition", OnSetCameraPosition);
		EventService.Disconnect("AddPower", AddPower);
		EventService.Disconnect("CombatStatus", CombatStatus);
		EventService.Disconnect("TeleportPlayer", OnTeleportPlayer);
		EventService.Disconnect("PlaySFX", OnPlaySFX);
    }
}
