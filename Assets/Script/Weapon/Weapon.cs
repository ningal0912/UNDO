using UnityEngine;

// 1. 무기 등급 정의 (Enum)
public enum WeaponRarity
{
    Common,
    Rare,
    Epic,
    Unique,
    Legendary
}

// 2. 모든 무기의 기반이 되는 최상위 클래스 (Weapon)
public abstract class Weapon : MonoBehaviour
{
    [Header("기본 정보")]
    public string weaponName;
    public WeaponRarity rarity = WeaponRarity.Common;
    public float baseDamage = 10f;
    public float attackCooldown = 0.5f;
    public LayerMask enemyLayer;
    public GameObject droppedPrefab;

    [Header("시각화 연출")]
    public LineRenderer rangeLine;
    protected Color normalColor = new Color(0.5f, 0.5f, 0.5f, 0.4f); // 대기 시 회색 (투명도 포함)
    protected Color attackColor = new Color(1f, 1f, 1f, 0.9f);       // 공격 시 하얀색

    protected float lastAttackTime;

    public virtual float Damage => baseDamage * (1f + (int)rarity * 0.25f);
    public virtual float AttackCooldown => attackCooldown;

    public abstract void Shoot();
    public abstract void UpdateRangeIndicator(); // 매 프레임 시각화 업데이트
}