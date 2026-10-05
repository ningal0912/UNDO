using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayTimeUI : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public Text playTimeText;
    public TextMeshProUGUI playTimeTMP;

    private void Update()
    {
        if (GameManager.Instance == null) return;

        // 항상 "00:00:00" 형태로 반환됨
        string formattedTime = GameManager.Instance.FormatTime(GameManager.Instance.currentPlayTime);

        if (playTimeText != null)
        {
            playTimeText.text = formattedTime; // 예: "00:12:45"
        }

        if (playTimeTMP != null)
        {
            playTimeTMP.text = formattedTime;
        }
    }
}