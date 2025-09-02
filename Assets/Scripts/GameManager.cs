using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Coin Settings")]
    [SerializeField] private int coin = 1000;
    [SerializeField] private Text coinText;
    public int Coin => coin;

    [Header("Score Settings")]
    [SerializeField] private int currentScore = 0;
    [SerializeField] private Text scoreText;
    public int Score => currentScore;

    [Header("Selected Item")]
    public string selectedItemID;

    [Header("Player Reference")]
    public CharacterCustomizer characterCustomizer;
    public GameObject[] hatPrefabs;
    private SoundManager SoundManager;

    private string purchasedItemsKey = "PurchasedItems";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
        SoundManager = FindObjectOfType<SoundManager>();
    }

    private void Start()
    {
        if (characterCustomizer == null)
            characterCustomizer = FindObjectOfType<CharacterCustomizer>();

        UpdateCoinUI();
        UpdateScoreUI();
        ApplySelectedItemToPlayer();

        Character.OnPlayerDied += OnPlayerDied;
        EnemySpawner.OnAllBotsDead += OnAllBotsDead;
    }

    private void OnDestroy()
    {
        Character.OnPlayerDied -= OnPlayerDied;
        EnemySpawner.OnAllBotsDead -= OnAllBotsDead;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #region Game Events
    private void OnPlayerDied()
    {
        // Cộng thưởng khi thua (ví dụ: +20 coin an ủi)
        AddCoin(20);

        // Hiện panel thua
        UIManager.Instance?.ShowEndStagePanel(true, false);
        Time.timeScale = 0f;
        BotMovement.isGameStarted = false;
        SoundManager.PlayLose();
        Debug.Log("Player Died - Game Over");
    }

    private void OnAllBotsDead()
    {
        // Cộng thưởng khi thắng (ví dụ: +100 coin)
        AddCoin(100);

        // Hiện panel thắng
        UIManager.Instance?.ShowEndStagePanel(false, true);
        Time.timeScale = 0f;
        BotMovement.isGameStarted = false;
    }
    #endregion

    #region Game Control
    public void RestartGame()
    {
        Debug.Log("Restart Game...");

        // Khôi phục thời gian game
        Time.timeScale = 1f;
        BotMovement.isGameStarted = false;

        // Lưu dữ liệu coin/item
        SaveData();

        // Reset điểm số
        currentScore = 0;

        // Load lại scene hiện tại
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene Loaded: " + scene.name);

        // Sau khi load scene, tìm lại UI mới
        if (coinText == null)
        {
            var coinObj = GameObject.FindWithTag("CoinText");
            if (coinObj != null)
                SetCoinTextReference(coinObj.GetComponent<Text>());
        }

        if (scoreText == null)
        {
            var scoreObj = GameObject.FindWithTag("ScoreText");
            if (scoreObj != null)
                SetScoreTextReference(scoreObj.GetComponent<Text>());
        }

        // Reset score mỗi khi restart
        currentScore = 0;

        // Cập nhật lại UI
        UpdateCoinUI();
        UpdateScoreUI();

        // Áp dụng lại item đã chọn
        if (characterCustomizer == null)
            characterCustomizer = FindObjectOfType<CharacterCustomizer>();
        ApplySelectedItemToPlayer();
    }

    public void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Time.timeScale = 1f;
            BotMovement.isGameStarted = true;
            SaveData();
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("Không có level tiếp theo, quay về Main Menu!");
            LoadMainMenu();
        }
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        BotMovement.isGameStarted = false;
        SaveData();
        SceneManager.LoadScene("MainMenu");
       
    }
    #endregion

    #region Coin & Score
    public void AddCoin(int amount)
    {
        coin += amount;
        SaveData();
        UpdateCoinUI();
    }

    public bool HasEnoughCoin(int amount) => coin >= amount;

    public bool SpendCoin(int amount)
    {
        if (HasEnoughCoin(amount))
        {
            coin -= amount;
            SaveData();
            UpdateCoinUI();
            return true;
        }
        return false;
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
    }

    private void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = coin.ToString();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = currentScore.ToString();
    }

    public void SetCoinTextReference(Text text)
    {
        coinText = text;
        UpdateCoinUI();
    }

    public void SetScoreTextReference(Text text)
    {
        scoreText = text;
        UpdateScoreUI();
    }
    #endregion

    #region Item System
    public bool IsPurchased(string itemId)
    {
        string purchased = PlayerPrefs.GetString(purchasedItemsKey, "");
        return purchased.Contains(itemId);
    }

    public void AddPurchasedItem(string itemId)
    {
        string purchased = PlayerPrefs.GetString(purchasedItemsKey, "");
        if (!purchased.Contains(itemId))
        {
            purchased += itemId + ",";
            PlayerPrefs.SetString(purchasedItemsKey, purchased);
            PlayerPrefs.Save();
        }
    }

    private void LoadSelectedItem()
    {
        selectedItemID = PlayerPrefs.GetString("SelectedItem", "");
    }

    public void SetSelectedItem(string itemId)
    {
        if (IsPurchased(itemId))
        {
            selectedItemID = itemId;
            PlayerPrefs.SetString("SelectedItem", itemId);
            PlayerPrefs.Save();
            ApplySelectedItemToPlayer();
        }
    }

    public void ApplySelectedItemToPlayer()
    {
        if (characterCustomizer == null || string.IsNullOrEmpty(selectedItemID))
            return;

        foreach (GameObject hat in hatPrefabs)
        {
            if (hat.name == selectedItemID)
            {
                characterCustomizer.SetHat(hat);
                break;
            }
        }
    }

    public void PreviewItem(string itemId)
    {
        foreach (GameObject hat in hatPrefabs)
        {
            if (hat.name == itemId)
            {
                if (characterCustomizer != null)
                    characterCustomizer.SetHat(hat);
                break;
            }
        }
    }
    #endregion

    #region Save & Load
    private void SaveData()
    {
        PlayerPrefs.SetInt("Coin", coin);
        PlayerPrefs.SetString("SelectedItem", selectedItemID);
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        coin = PlayerPrefs.GetInt("Coin", 1000);
        LoadSelectedItem();
    }
    #endregion
}
