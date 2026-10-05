using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public Transform weaponHolder;

    public GameObject[] weapons =
        new GameObject[2];

    public int currentWeaponIndex = 0;

    [Header("세이브/로드용 무기 프리팹 목록")]
    public GameObject[] weaponPrefabs;

    private GameObject nearbyDropItem;

    // ============================================================
    // Start
    // ============================================================

    private void Start()
    {
        UpdateWeaponVisibility();
    }

    // ============================================================
    // Attack
    // ============================================================

    public void Attack()
    {
        if (weapons == null ||
            weapons.Length <= currentWeaponIndex)
            return;

        if (weapons[currentWeaponIndex] != null)
        {
            Weapon weapon =
                weapons[currentWeaponIndex]
                    .GetComponent<Weapon>();

            weapon?.Shoot();
        }
    }

    // ============================================================
    // Switch
    // ============================================================

    public void SwitchWeapon()
    {
        currentWeaponIndex =
            currentWeaponIndex == 0
                ? 1
                : 0;

        UpdateWeaponVisibility();
    }

    // ============================================================
    // Pickup
    // ============================================================

    public void PickupWeapon()
    {
        if (nearbyDropItem == null)
            return;

        DroppedItem dropped =
            nearbyDropItem
                .GetComponent<DroppedItem>();

        if (dropped == null ||
            dropped.weaponPrefab == null)
            return;

        // 기존 무기 버리기
        if (weapons[currentWeaponIndex] != null)
        {
            Weapon currentWeapon =
                weapons[currentWeaponIndex]
                    .GetComponent<Weapon>();

            if (currentWeapon != null &&
                currentWeapon.droppedPrefab != null)
            {
                Instantiate(
                    currentWeapon.droppedPrefab,
                    transform.position,
                    Quaternion.identity
                );
            }

            Destroy(
                weapons[currentWeaponIndex]
            );
        }

        // 새 무기 장착
        GameObject newWeapon =
            Instantiate(
                dropped.weaponPrefab,
                weaponHolder
            );

        newWeapon.transform.localPosition =
            Vector3.zero;

        newWeapon.transform.localRotation =
            Quaternion.identity;

        weapons[currentWeaponIndex] =
            newWeapon;

        Destroy(
            nearbyDropItem
        );

        nearbyDropItem = null;

        UpdateWeaponVisibility();
    }

    // ============================================================
    // Save / Load
    // ============================================================

    public void RestoreWeapons(
        string mainWeaponName,
        string subWeaponName,
        int savedIndex)
    {
        if (weaponHolder == null)
        {
            Debug.LogError(
                "[WeaponManager] weaponHolder가 없습니다."
            );

            return;
        }

        // 기존 무기 제거
        for (int i = 0;
             i < weapons.Length;
             i++)
        {
            if (weapons[i] != null)
            {
                Destroy(
                    weapons[i]
                );

                weapons[i] = null;
            }
        }

        // --------------------------------------------------------
        // Slot 0
        // --------------------------------------------------------

        if (!string.IsNullOrEmpty(mainWeaponName))
        {
            GameObject prefab =
                FindWeaponPrefab(
                    mainWeaponName
                );

            if (prefab != null)
            {
                weapons[0] =
                    Instantiate(
                        prefab,
                        weaponHolder
                    );

                ResetWeaponTransform(
                    weapons[0]
                );
            }
        }

        // --------------------------------------------------------
        // Slot 1
        // --------------------------------------------------------

        if (!string.IsNullOrEmpty(subWeaponName))
        {
            GameObject prefab =
                FindWeaponPrefab(
                    subWeaponName
                );

            if (prefab != null)
            {
                weapons[1] =
                    Instantiate(
                        prefab,
                        weaponHolder
                    );

                ResetWeaponTransform(
                    weapons[1]
                );
            }
        }

        // --------------------------------------------------------
        // 현재 슬롯
        // --------------------------------------------------------

        currentWeaponIndex =
            Mathf.Clamp(
                savedIndex,
                0,
                weapons.Length - 1
            );

        UpdateWeaponVisibility();
    }

    private GameObject FindWeaponPrefab(
        string weaponName)
    {
        if (weaponPrefabs == null)
            return null;

        string cleanName =
            weaponName.Replace(
                "(Clone)",
                ""
            );

        foreach (GameObject prefab in weaponPrefabs)
        {
            if (prefab == null)
                continue;

            string prefabName =
                prefab.name.Replace(
                    "(Clone)",
                    ""
                );

            if (prefabName ==
                cleanName)
            {
                return prefab;
            }
        }

        Debug.LogWarning(
            $"[WeaponManager] 저장된 무기 프리팹을 찾을 수 없습니다: {weaponName}"
        );

        return null;
    }

    private void ResetWeaponTransform(
        GameObject weapon)
    {
        if (weapon == null)
            return;

        weapon.transform.localPosition =
            Vector3.zero;

        weapon.transform.localRotation =
            Quaternion.identity;

        weapon.transform.localScale =
            Vector3.one;
    }

    // ============================================================
    // Visibility
    // ============================================================

    private void UpdateWeaponVisibility()
    {
        if (weapons == null)
            return;

        for (int i = 0;
             i < weapons.Length;
             i++)
        {
            if (weapons[i] != null)
            {
                weapons[i].SetActive(
                    i == currentWeaponIndex
                );
            }
        }
    }

    // ============================================================
    // Trigger
    // ============================================================

    private void OnTriggerEnter2D(
        Collider2D collision)
    {
        if (collision.CompareTag(
            "DroppedWeapon"))
        {
            nearbyDropItem =
                collision.gameObject;
        }
    }

    private void OnTriggerExit2D(
        Collider2D collision)
    {
        if (collision.gameObject ==
            nearbyDropItem)
        {
            nearbyDropItem = null;
        }
    }
}