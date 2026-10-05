using UnityEngine;
using UnityEngine.UI; // 레거시 UI Text 사용 시
using TMPro; // TextMeshPro 사용 시 필요

public class WeaponUI : MonoBehaviour
{
    [Header("참조 설정")]
    public WeaponManager weaponManager; // 플레이어의 WeaponManager

    [Header("UI 텍스트 컴포넌트 (2개로 분리)")]
    public Text mainWeaponText; // 메인 무기 이름 표시용 Text
    public Text subWeaponText;  // 서브 무기 이름 표시용 Text

    // TextMeshPro를 사용하실 경우 위의 Text 대신 아래 주석을 해제하여 사용하세요.
    public TextMeshProUGUI mainWeaponTMP;
    public TextMeshProUGUI subWeaponTMP;
    

    [Header("색상 설정")]
    // 메인 무기: 흰색, 서브 무기: 회색
    public Color mainColor = Color.white;
    public Color subColor = new Color(0.5f, 0.5f, 0.5f, 1f); // 회색 (RGB: 128, 128, 128)

    void Start()
    {
        // weaponManager가 연결되지 않았다면 Player 태그 탐색
        if (weaponManager == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                weaponManager = player.GetComponent<WeaponManager>();
            }
        }

        // 초기 색상 세팅
        SetTextColor();
    }

    void Update()
    {
        if (weaponManager == null) return;

        UpdateWeaponUI();
    }

    private void SetTextColor()
    {
        // UI Text 색상 세팅
        if (mainWeaponText != null) mainWeaponText.color = mainColor;
        if (subWeaponText != null) subWeaponText.color = subColor;

         //TextMeshPro 사용 시
        if (mainWeaponTMP != null) mainWeaponTMP.color = mainColor;
        if (subWeaponTMP != null) subWeaponTMP.color = subColor;
        
    }

    private void UpdateWeaponUI()
    {
        // 1. 현재 선택된 메인무기 및 서브무기 인덱스 확인
        int mainIndex = weaponManager.currentWeaponIndex;
        int subIndex = (mainIndex == 0) ? 1 : 0;

        // 2. 무기 오브젝트 가져오기
        GameObject mainObj = weaponManager.weapons[mainIndex];
        GameObject subObj = weaponManager.weapons[subIndex];

        // 3. 무기 이름 추출 (Weapon 스크립트 속 weaponName)
        string mainName = GetWeaponName(mainObj);
        string subName = GetWeaponName(subObj);

        // 4. 각각의 텍스트 메시지에 무기 이름 업데이트
        // (요청하신 대로 서브 무기를 위/먼저, 메인 무기를 아래/나중에 배치하거나 각각 원하는 위치로 설정 가능)
        if (mainWeaponText != null)
        {
            mainWeaponText.text = $"Main: {mainName}";
        }

        if (subWeaponText != null)
        {
            subWeaponText.text = $"Sub : {subName}";
        }

        // TextMeshPro 사용 시
        if (mainWeaponTMP != null) mainWeaponTMP.text = $"Main: {mainName}";
        if (subWeaponTMP != null) subWeaponTMP.text = $"Sub : {subName}";
        
    }

    // 오브젝트에서 Weapon 스크립트의 weaponName을 가져오는 함수
    private string GetWeaponName(GameObject weaponObj)
    {
        if (weaponObj == null) return "none";

        Weapon weaponScript = weaponObj.GetComponent<Weapon>();
        if (weaponScript != null && !string.IsNullOrEmpty(weaponScript.weaponName))
        {
            return weaponScript.weaponName;
        }

        return "none";
    }
}