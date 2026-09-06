using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Transform weaponHolder; // 무기가 위치할 자식 오브젝트 Transform

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 mousePos;
    private WeaponManager weaponManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        weaponManager = GetComponent<WeaponManager>();

        // 1. 마우스 커서 항상 보이게 설정
        Cursor.visible = true;

        // 2. 마우스 커서를 게임 창 화면 안으로 제한 (선택 사항)
        Cursor.lockState = CursorLockMode.None;
    }

    void Update()
    {
        // 1. WASD 이동 입력
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // 2. 마우스 위치 추적
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 3. 공격 (마우스 좌클릭)
        if (Input.GetMouseButtonDown(0))
        {
            weaponManager.Attack();
        }

        // 4. 슬롯 무기 전환 (마우스 우클릭)
        if (Input.GetMouseButtonDown(1))
        {
            weaponManager.SwitchWeapon();
        }

        // 5. 바닥 무기 교체 (F키)
        if (Input.GetKeyDown(KeyCode.F))
        {
            weaponManager.PickupWeapon();
        }
    }

    void FixedUpdate()
    {
        // 이동 처리
        rb.MovePosition(rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime);

        // 마우스 방향 바라보기 & 무기 회전
        Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        weaponHolder.rotation = Quaternion.Euler(0, 0, angle);
    }
}