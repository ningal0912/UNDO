using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public Transform weaponHolder;
    public GameObject[] weapons = new GameObject[2];
    public int currentWeaponIndex = 0;

    private GameObject nearbyDropItem;

    void Start()
    {
        UpdateWeaponVisibility();
    }

    // 1. Attack 함수 
    public void Attack()
    {
        if (weapons[currentWeaponIndex] != null)
        {
            weapons[currentWeaponIndex].GetComponent<Weapon>()?.Shoot();
        }
    }

    // 2. SwitchWeapon 함수 
    public void SwitchWeapon()
    {
        currentWeaponIndex = (currentWeaponIndex == 0) ? 1 : 0;
        UpdateWeaponVisibility();
    }

    // 3. PickupWeapon 함수 
    public void PickupWeapon()
    {
        if (nearbyDropItem == null) return;

        DroppedItem dropped = nearbyDropItem.GetComponent<DroppedItem>();
        if (dropped == null || dropped.weaponPrefab == null) return;

        if (weapons[currentWeaponIndex] != null)
        {
            Weapon currentWeaponScript = weapons[currentWeaponIndex].GetComponent<Weapon>();
            if (currentWeaponScript != null && currentWeaponScript.droppedPrefab != null)
            {
                Instantiate(currentWeaponScript.droppedPrefab, transform.position, Quaternion.identity);
            }
            Destroy(weapons[currentWeaponIndex]);
        }

        GameObject newWeapon = Instantiate(dropped.weaponPrefab, weaponHolder);
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;
        weapons[currentWeaponIndex] = newWeapon;

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