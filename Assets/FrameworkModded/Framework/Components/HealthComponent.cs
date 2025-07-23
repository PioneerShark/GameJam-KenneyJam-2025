using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using static Framework;
using static UnityEngine.Rendering.DebugUI;

public class HealthComponent : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve animationCurve;
    [SerializeField]
    private Color flashColour = Color.gray;
    [SerializeField]
    private int maxDamageFlashThreshold = 10;
    private float maxFlashTime = 0.25f;

    private SpriteRenderer[] spriteRenderers;
    private Material[] materials;

    EventService EventService;

    UnityEvent<Room> OnDeathEvent;
    Room roomID;

    [SerializeField]
    float healthMax;
    
    float healthCurrent = 100f;

    private void Awake()
    {
        EventService = Game.GetService<EventService>();
        healthCurrent = healthMax;
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        InitialiseMaterials();
    }
    private void Start()
    {
        if (gameObject.CompareTag("Player"))
        {
            KennyGameManager.Instance.UpdateHealth((int)healthCurrent);
        }
    }

    public void TakeDamage(int value)
    {
        healthCurrent -= value;
        StartCoroutine(DamageFlash(value));
        if (healthCurrent <= 0)
        {
            Die();
        }
        if (gameObject.CompareTag("Player"))
        {
            KennyGameManager.Instance.UpdateHealth((int)healthCurrent);
            EventService.Fire("PlaySFX", new PlaySFXInfo("PlayerHurt", transform.position));
        }
        else
        {
            EventService.Fire("PlaySFX", new PlaySFXInfo("EnemyHurt", transform.position));
        }
    }
    public void SetMaxHealth(int value)
    {
        healthMax = value;
        healthCurrent = value;
    }

    public void Heal(int value)
    {
        healthCurrent += value;
    }

    public float GetHealth() {
        return healthCurrent;
    }

    public float GetHealthAsPercent()
    {
        return healthCurrent / healthMax;
    }

    public void Die()
    {
        if (OnDeathEvent != null)
        {
            OnDeathEvent.Invoke(roomID);
        }
        if (gameObject.tag != "Player")
        {
            EventService.Fire("AddPower");
            EventService.Fire("PlaySFX", new PlaySFXInfo("EnemyDied", transform.position));
        }
        else
        {
            _ = ToMainMenu();
        }
            Destroy(this.gameObject);
    }

    public void AssignDeathEvent(UnityAction<Room> deathEvent, Room newRoomID){
        if (OnDeathEvent == null)
        {
            OnDeathEvent = new UnityEvent<Room>();
        }
        OnDeathEvent.AddListener(deathEvent);
        roomID = newRoomID;
    }

    private void InitialiseMaterials()
    {
        materials = new Material[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            materials[i] = spriteRenderers[i].material;
        }
    }

    private IEnumerator DamageFlash(float damage)
    {
        float maxFlashValue = Mathf.Min(1f, (float)(damage / maxDamageFlashThreshold));
        float flashTime = maxFlashValue * maxFlashTime;
        float currentFlashValue = 0f;
        //Debug.Log("damage Taken(Enumerator");
        SetFlashColour();
        
        float elapsedTime = 0f;
        while (elapsedTime < flashTime)
        {
            elapsedTime += Time.deltaTime;

            currentFlashValue = Mathf.Lerp(maxFlashValue, 0f, (elapsedTime/flashTime));
            SetFlashValue(currentFlashValue);
            
            yield return null;
        }

    }

    private void SetFlashColour()
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetColor("_FlashColour", flashColour);
        }
    }

    private void SetFlashValue(float value)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetFloat("_FlashValue", value);
        }
    }
    async UniTask ToMainMenu()
    {
        await UniTask.Yield();
        await UniTask.Delay(3000);
        SceneManager.LoadScene("SCN_Menu");
    }
}
