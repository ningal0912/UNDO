using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public Transform weaponHolder;
    public GameObject[] weapons = new GameObject[2]; // 최대 2개 슬롯
    public int currentWeaponIndex = 0;

    private GameObject nearbyDropItem; // 범위 내에 있는 dropped 무기

    void Start()
    {
        UpdateWeaponVisibility();
    }

    public void Attack()
    {
        if (weapons[currentWeaponIndex] != null)
        {
            // 현재 장착 중인 무기의 공격 메커니즘 실행
            weapons[currentWeaponIndex].GetComponent<Weapon>()?.Shoot();
        }
    }

    // 마우스 우클릭: 1번/2번 무기 슬롯 전환
    public void SwitchWeapon()
    {
        currentWeaponIndex = (currentWeaponIndex == 0) ? 1 : 0;
        UpdateWeaponVisibility();
    }

    // F키: 범위 내 떨어진 무기와 교체
    public void PickupWeapon()
    {
        if (nearbyDropItem == null) return;

        DroppedItem dropped = nearbyDropItem.GetComponent<DroppedItem>();
        if (dropped == null) return;

        GameObject newWeaponPrefab = dropped.weaponPrefab;

        // 1. 현재 슬롯에 무기가 이미 있다면 바닥에 버림
        if (weapons[currentWeaponIndex] != null)
        {
            // 현재 무기 버리기 로직 (필드에 DroppedItem 생성)
            Instantiate(weapons[currentWeaponIndex].GetComponent<Weapon>().droppedPrefab, transform.position, Quaternion.identity);
            Destroy(weapons[currentWeaponIndex]);
        }

        // 2. 새 무기 생성 및 장착
        GameObject newWeapon = Instantiate(newWeaponPrefab, weaponHolder);
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;
        weapons[currentWeaponIndex] = newWeapon;

        // 3. 필드에 있던 무기 아이템 제거
        Destroy(nearbyDropItem);
        nearbyDropItem = null;

        UpdateWeaponVisibility();
    }

    private void UpdateWeaponVisibility()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
            {
                weapons[i].SetActive(i == currentWeaponIndex);
            }
        }
    }

    // F키 감지용 트리거 범위 체크
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DroppedWeapon"))
        {
            nearbyDropItem = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == nearbyDropItem)
        {
            nearbyDropItem = null;
        }
    }
}