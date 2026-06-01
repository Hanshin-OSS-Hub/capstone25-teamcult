using UnityEngine;
using System.IO;
using System.Collections;
[System.Serializable]
public class RunSaveData
{
    // PlayerHealth
    public float maxHealth;
    // ElementalManager
    public bool hasFireHeart;
    public bool hasIceHeart;
    public bool hasLightningHeart;
    public bool isAbilityActive;
    public string currentType;
    public float abilityTimer;
    public int lightningHitCounter;
}
public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;
    private string savePath => Application.persistentDataPath + "/run_save.json";
    public PlayerHealth playerHealth;
    public ElementalManager elementalManager;
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    void Start()
    {
        int isContinue = PlayerPrefs.GetInt("IsContinue", 0);
        if (isContinue == 1 && HasSavedRun())
        {
            StartCoroutine(LoadAfterFrame());
        }
    }
    IEnumerator LoadAfterFrame()
    {
        yield return null;
        LoadRun();
    }
    public void SaveRun()
    {
        RunSaveData data = new RunSaveData
        {
            maxHealth = playerHealth.maxHealth,
            hasFireHeart = elementalManager.hasFireHeart,
            hasIceHeart = elementalManager.hasIceHeart,
            hasLightningHeart = elementalManager.hasLightningHeart,
            isAbilityActive = elementalManager.isAbilityActive,
            currentType = elementalManager.currentType,
            abilityTimer = elementalManager.abilityTimer,
            lightningHitCounter = elementalManager.lightningHitCounter
        };
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("[SaveManager] 저장 완료");
    }
    public void LoadRun()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("[SaveManager] 저장 파일 없음");
            return;
        }
        string json = File.ReadAllText(savePath);
        RunSaveData data = JsonUtility.FromJson<RunSaveData>(json);

        // maxHealth는 복원하되, 시작 체력은 3칸(24)으로 고정
        playerHealth.maxHealth = data.maxHealth;
        playerHealth.currentHealth = 24f;
        playerHealth.UpdateUI();

        // ElementalManager 복원
        elementalManager.lightningHitCounter = data.lightningHitCounter;
        if (data.isAbilityActive && !string.IsNullOrEmpty(data.currentType))
        {
            elementalManager.ActivateAbility(data.currentType);
            elementalManager.abilityTimer = data.abilityTimer;
        }
        Debug.Log("[SaveManager] 불러오기 완료 - 3칸으로 시작");
    }
    public void DeleteRun()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("[SaveManager] 런 데이터 삭제");
        }
    }
    public bool HasSavedRun() => File.Exists(savePath);
}
