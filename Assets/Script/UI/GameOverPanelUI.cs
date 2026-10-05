using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro 사용 시

public class GameOverPanelUI : MonoBehaviour
{
    public static GameOverPanelUI Instance;

    [Header("UI 패널 연결")]
    public GameObject endPanel; // 엔드 패널 오브젝트

    [Header("텍스트 컴포넌트 (TMPro 또는 레거시 Text 선택)")]
    public TextMeshProUGUI playTimeTMP;
    public TextMeshProUGUI weaponsTMP;

    [Header("씬 이름 설정")]
    public string mainMenuSceneName = "MainMenu";

    // 엔드 패널이 열려 있는지 여부 (ESC 일시정지 메뉴 차단용)
    public bool IsEndPanelActive { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (endPanel != null)
        {
            endPanel.SetActive(false);
        }
        IsEndPanelActive = false;
    }

    // ==============================================================
    // 엔드 패널 활성화 (사망 또는 보스 처치 시 호출)
    // ==============================================================
    public void ShowEndPanel(string titleMessage = "GAME OVER")
    {
        IsEndPanelActive = true;

        // 1. 세이브 데이터 삭제
        if (GameManager.Instance != null)
        {
            GameManager.Instance.DeleteSaveFile(); // 저장된 saveData.json 제거
            GameManager.Instance.StopTimer();     // 타이머 정지
        }

        // 2. 정보 갱신 (플레이 타임, 무기)
        UpdateEndInfo();

        // 3. UI 활성화 및 게임 정지
        if (endPanel != null)
        {
            endPanel.SetActive(true);
        }

        Time.timeScale = 0f; // 진행 정지
    }

    private void UpdateEndInfo()
    {
        if (GameManager.Instance == null) return;

        // [1] 플레이 타임 서식 적용
        string formattedTime = GameManager.Instance.FormatTime(GameManager.Instance.currentPlayTime);
        if (playTimeTMP != null)
        {
            playTimeTMP.text = $"Play Time : {formattedTime}";
        }

        // [2] 현재 소지 중인 무기 정보 가져오기
        string mainWeapon = "none";
        string subWeapon = "none";

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            WeaponManager weaponMgr = player.GetComponent<WeaponManager>();
            if (weaponMgr != null && weaponMgr.weapons != null)
            {
                if (weaponMgr.weapons.Length > 0 && weaponMgr.weapons[0] != null)
                {
                    Weapon w0 = weaponMgr.weapons[0].GetComponent<Weapon>();
                    if (w0 != null) mainWeapon = w0.weaponName;
                }

                if (weaponMgr.weapons.Length > 1 && weaponMgr.weapons[1] != null)
                {
                    Weapon w1 = weaponMgr.weapons[1].GetComponent<Weapon>();
                    if (w1 != null) subWeapon = w1.weaponName;
                }
            }
        }

        if (weaponsTMP != null)
        {
            weaponsTMP.text = $"Equipped Weapons\nMain: {mainWeapon}\nSub : {subWeapon}";
        }
    }

    // ==============================================================
    // 버튼 이벤트
    // ==============================================================

    // [버튼 1] 처음부터 재시작
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

    // [버튼 2] 메인화면으로 나가기
    public void OnClickMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}