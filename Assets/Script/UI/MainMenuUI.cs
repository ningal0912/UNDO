using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("버튼 참조")]
    public Button continueButton; // 이어하기 버튼 (세이브 파일 유무에 따라 비활성화)

    [Header("씬 이름 설정")]
    public string gameSceneName = "GameScene"; // 실제 게임이 진행되는 씬 이름

    private void Start()
    {
        // 1. 세이브 파일 존재 여부 확인 후 '이어하기' 버튼 활성화/비활성화 처리
        CheckSaveFile();
    }

    private void CheckSaveFile()
    {
        if (continueButton != null && GameManager.Instance != null)
        {
            // 세이브 파일이 존재할 때만 이어하기 버튼 활성화
            bool hasSave = GameManager.Instance.HasSaveFile();
            continueButton.interactable = hasSave;
        }
    }

    // [버튼 1] 새 게임 (New Game)
    public void OnClickNewGame()
    {
        // 1. 기존 세이브 파일 및 시간 데이터 초기화
        if (GameManager.Instance != null)
        {
            GameManager.Instance.DeleteSaveFile();
            GameManager.Instance.ResetTimer();
        }

        // 2. 게임 진행 속도 정시 복구
        Time.timeScale = 1f;

        // 3. 인게임 씬으로 이동
        SceneManager.LoadScene(gameSceneName);
    }

    // [버튼 2] 이어하기 (Continue)
    public void OnClickContinue()
    {
        if (GameManager.Instance == null) return;

        // 세이브 파일이 있을 경우에만 진행
        if (GameManager.Instance.HasSaveFile())
        {
            Time.timeScale = 1f;

            // 게임 씬 로드 후 GameManager에서 저장된 데이터 복원
            SceneManager.LoadScene(gameSceneName);

            // 씬 이동 직후 로드 실행을 위해 GameManager 로드 호출
            // (GameManager가 DontDestroyOnLoad로 유지되므로 씬 전환 후 로드가 진행됨)
            GameManager.Instance.LoadGame();
        }
    }

    // [버튼 3] 게임 종료 (Quit Game)
    public void OnClickQuit()
    {
#if UNITY_EDITOR
        // 유니티 에디터 플레이 모드 종료
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드된 게임 앱 종료
        Application.Quit();
#endif
    }
}