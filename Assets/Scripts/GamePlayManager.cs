using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayManager : MonoBehaviour
{
    [SerializeField] private Button SettingButton;
    [SerializeField] private GameObject PauseMenuUI;
    [SerializeField] private Button ResumeButton;
    [SerializeField] private Button MainMenuButton;
    [SerializeField] private Text TopTxt;
    [SerializeField] private GameObject JoyStick;    
    
    private void Awake()
    {
        if (SettingButton != null)
            SettingButton.onClick.AddListener(ShowPauseMenu);
        if (ResumeButton != null)
            ResumeButton.onClick.AddListener(ResumeGame);
        if (MainMenuButton != null)
            MainMenuButton.onClick.AddListener(ExitToMainMenu);
        if (PauseMenuUI != null)
            PauseMenuUI.SetActive(false);
    }
    private void Update()
    {
        UpdateTopScore();
         
    }





    public void ShowPauseMenu()
    {
        if (PauseMenuUI != null)
            PauseMenuUI.SetActive(true);
        if (JoyStick != null)
            JoyStick.SetActive(false);
        Time.timeScale = 0f; // Tạm dừng trò chơi
    }
    public void ResumeGame()
    {
        if (PauseMenuUI != null)
            PauseMenuUI.SetActive(false);
        if (JoyStick != null)
            JoyStick.SetActive(true);
        Time.timeScale = 1f; // Tiếp tục trò chơi
    }
    public void ExitToMainMenu()
    {
        Time.timeScale = 1f; // Đảm bảo trò chơi không bị tạm dừng khi trở về menu chính
        GameManager.Instance.RestartGame();
    }

    public void UpdateTopScore()
    {
        EnemySpawner enemySpawner = FindObjectOfType<EnemySpawner>();
        if (TopTxt == null || enemySpawner == null) return;

        int rank = enemySpawner.GetTopScore();
        TopTxt.text = "Top " + rank;

        // ✅ Nếu rank = 1 thì show panel thắng
        if (rank == 1)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowEndStagePanel(isPlayerDead: false, isWin: true);
            }
            else
            {
                Debug.LogWarning("UIManager chưa được gán Instance!");
            }
        }
    }




}
