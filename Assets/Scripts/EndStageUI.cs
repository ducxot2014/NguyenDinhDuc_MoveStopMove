using UnityEngine;
using UnityEngine.UI;

public class EndStageUI : MonoBehaviour
{
    [SerializeField] private Button nextLevelBtn;
    [SerializeField] private Button restartBtn;
    [SerializeField] private Button mainMenuBtn;
    [SerializeField] private Text endStageText;
    [SerializeField] private Text topText;

    private void Awake()
    {
        // Gắn sự kiện cho các nút
        if (nextLevelBtn != null)
        {
            nextLevelBtn.onClick.AddListener(OnNextLevelClicked);
        }
        else
        {
            Debug.LogWarning("NextLevelBtn chưa được gán trong EndStageUI!");
        }

        if (restartBtn != null)
        {
            restartBtn.onClick.AddListener(OnRestartClicked);
        }
        else
        {
            Debug.LogWarning("RestartBtn chưa được gán trong EndStageUI!");
        }

        if (mainMenuBtn != null)
        {
            mainMenuBtn.onClick.AddListener(OnMainMenuClicked);
        }
        else
        {
            Debug.LogWarning("MainMenuBtn chưa được gán trong EndStageUI!");
        }

        // Ẩn panel khi khởi tạo
        gameObject.SetActive(false);
    }

    // Cập nhật nội dung panel dựa trên trạng thái win/lose
   public void UpdatePanelContent(bool isPlayerDead, bool isWin, int rank = 0)
{
    if (endStageText != null)
    {
        if (isPlayerDead)
        {
            endStageText.text = "You Lose!";
            ShowLose();

            // Hiển thị thứ hạng hiện tại
            
                topText.text = "Non quá " ;
        }
        else if (isWin)
        {
            endStageText.text = "You Win!";
            ShowWin();

            if (topText != null)
                topText.text = "Top 1";
        }
        else
        {
            endStageText.text = "Game Over";
            ShowLose();

            if (topText != null)
                topText.text = "Top " + rank;
        }
    }
    else
    {
        Debug.LogWarning("endStageText chưa được gán trong EndStageUI!");
    }
}


    private void ShowWin()
    {
        if (nextLevelBtn != null) nextLevelBtn.gameObject.SetActive(true);
        if (restartBtn != null) restartBtn.gameObject.SetActive(false);
        if (mainMenuBtn != null) mainMenuBtn.gameObject.SetActive(true);
    }

    private void ShowLose()
    {
        if (nextLevelBtn != null) nextLevelBtn.gameObject.SetActive(false);
        if (restartBtn != null) restartBtn.gameObject.SetActive(true);
        if (mainMenuBtn != null) mainMenuBtn.gameObject.SetActive(true);
    }

    private void OnNextLevelClicked()
    {
        Debug.Log("Next Level button clicked!");
        GameManager.Instance.LoadNextLevel();
    }

    private void OnRestartClicked()
    {
        Debug.Log("Restart button clicked!");
        GameManager.Instance.RestartGame(); // SỬA: Gọi RestartGame thay vì RestartLevel
    }

    private void OnMainMenuClicked()
    {
        Debug.Log("Main Menu button clicked!");
        GameManager.Instance.LoadMainMenu();
    }
}