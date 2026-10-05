using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // 1. New Input System 네임스페이스 추가

public class PauseMenuUI : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject pauseMenuPanel;

    [Header("씬 이름 설정")]
    public string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;

    private void Start()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // 💡 엔드 패널이 열려 있는 상태라면 ESC 메뉴를 열지 않음
        if (GameOverPanelUI.Instance != null && GameOverPanelUI.Instance.IsEndPanelActive)
        {
            return;
        }

        // ESC 키 입력 시 메뉴창 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);

        Time.timeScale = 0f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StopTimer();
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);

        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartTimer();
        }
    }

    public void OnClickRestart()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetTimer();
            GameManager.Instance.StartTimer();
        }

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void OnClickSaveAndExit()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveGame();
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}