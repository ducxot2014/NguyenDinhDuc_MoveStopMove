using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject MainMenuUI;
    [SerializeField] private GameObject ShopUI;
    [SerializeField] private GameObject endStagePanel;
    [SerializeField] private GameObject SettingBtn;
    [Header("Buttons")]
    [SerializeField] private Button Exitbtn;
    [SerializeField] private Button Shopbtn;
    [SerializeField] private Button PlayBtn;

    [Header("Dependencies")]
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private Button playAgainButton;

    private void Awake()
    {
        // ✅ Luôn reset Instance mỗi khi scene load
        Instance = this;

        // Setup nút Play Again
        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveAllListeners();
            playAgainButton.onClick.AddListener(() =>
            {
                GameManager.Instance.RestartGame();
            });
        }

        if (endStagePanel != null)
            endStagePanel.SetActive(false);
    }

    private void Start()
    {
        ShowMainMenu();

        if (Exitbtn != null)
            Exitbtn.onClick.AddListener(ExitToMainMenu);

        if (Shopbtn != null)
            Shopbtn.onClick.AddListener(ShowShop);

        if (PlayBtn != null)
            PlayBtn.onClick.AddListener(OnPlayed);

        if (cameraFollow != null)
            cameraFollow.ZoomOut(1f);
    }

    // ====================== END STAGE PANEL ======================
    public void ShowEndStagePanel(bool isPlayerDead, bool isWin)
    {
        if (endStagePanel == null)
        {
            Debug.LogError("EndStagePanel chưa được gán trong UIManager!");
            return;
        }

        MainMenuUI.SetActive(false);
        ShopUI.SetActive(false);
        endStagePanel.SetActive(true);

        EndStageUI panelScript = endStagePanel.GetComponent<EndStageUI>();
        if (panelScript != null)
        {
            panelScript.UpdatePanelContent(isPlayerDead, isWin);
        }
        else
        {
            Debug.LogWarning("EndStageUI script không gắn vào EndStagePanel!");
        }
    }

    // ====================== MAIN MENU ======================
    public void ShowMainMenu()
    {
        MainMenuUI.SetActive(true);
        ShopUI.SetActive(false);
        SettingBtn.SetActive(false);
        if (endStagePanel != null)
            endStagePanel.SetActive(false);

        if (cameraFollow != null)
            cameraFollow.ZoomOut(1f);

        if (shopManager != null)
            shopManager.ResetToLastSelected();
    }

    public void ShowShop()
    {
        MainMenuUI.SetActive(false);
        ShopUI.SetActive(true);
        if (endStagePanel != null)
            endStagePanel.SetActive(false);

        if (cameraFollow != null)
            cameraFollow.ZoomIn(1f);
    }

    public void ExitToMainMenu()
    {
        ShowMainMenu();
        Debug.Log("Exiting to Main Menu...");
    }

    public void ViewShop()
    {
        ShowShop();
        Debug.Log("Viewing Shop...");
    }

    // ====================== GAMEPLAY ======================
    public void OnPlayed()
    {
        if (cameraFollow != null)
            cameraFollow.OnPlayed();

        MainMenuUI.SetActive(false);
        ShopUI.SetActive(false);
        SettingBtn.gameObject.SetActive(true);
        if (endStagePanel != null)
            endStagePanel.SetActive(false);

        foreach (var bot in enemySpawner.GetActiveBots())
            bot.SetCanMove(true);

        BotMovement.isGameStarted = true;
        Debug.Log("Game started, bots di chuyển.");
    }

    public void OnNextLevel()
    {
        GameManager.Instance.LoadNextLevel();
        if (endStagePanel != null)
            endStagePanel.SetActive(false);
    }

    public void OnPlayAgain()
    {
        if (cameraFollow != null)
            cameraFollow.ResetCamera();

        MainMenuUI.SetActive(false);
        ShopUI.SetActive(false);
        if (endStagePanel != null)
            endStagePanel.SetActive(false);

        GameManager.Instance.RestartGame();
        Debug.Log("Restart button pressed → Reloading scene...");
    }

    public void ReturnToMainMenu()
    {
        ShowMainMenu();
        if (cameraFollow != null)
            cameraFollow.ResetCamera();

        GameManager.Instance.LoadMainMenu();
        Debug.Log("Returning to Main Menu.");
    }
}
