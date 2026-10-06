using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// キャラクターの属性、パワー、移動・攻撃範囲を保持するデータクラス。
/// 各ユニットのプレハブにアタッチして使用する。
/// </summary>
public class Unit : MonoBehaviour
{
    public enum AttributeType
    {
        Fire,   // 水に弱く、木に強い
        Water,  // 木に弱く、火に強い
        Wood    // 火に弱く、水に強い
    }

    [Header("Unit Stats")]
    [SerializeField] private string unitName;
    [SerializeField] private AttributeType attribute;
    [SerializeField] private int power;
    [SerializeField] private bool isPlayerOwned;

    [Header("Range Definitions")]
    [Tooltip("移動可能な相対座標リスト")]
    [SerializeField] private List<Vector2Int> moveRange;
    [Tooltip("攻撃可能な相対座標リスト")]
    [SerializeField] private List<Vector2Int> attackRange;

    // 現在のグリッド上の位置
    public Vector2Int CurrentGridPosition { get; set; }

    public string UnitName => unitName;
    public AttributeType Attribute => attribute;
    public int Power => power;
    public bool IsPlayerOwned => isPlayerOwned;
    public List<Vector2Int> MoveRange => moveRange;
    public List<Vector2Int> AttackRange => attackRange;

    /// <summary>
    /// ユニットの初期化処理
    /// </summary>
    private void Awake()
    {
        // 必要に応じて初期化ロジックを記述
    }

    /// <summary>
    /// ユニットのパワーを一時的に変更する（バフカード用）
    /// </summary>
    /// <param name="amount">加算する値</param>
    public void ModifyPower(int amount)
    {
        power = Mathf.Max(0, power + amount);
    }

    /// <summary>
    /// 指定された座標が攻撃範囲内にあるか判定
    /// </summary>
    public bool IsInRange(Vector2Int targetGridPos)
    {
        Vector2Int relativePos = targetGridPos - CurrentGridPosition;
        return attackRange.Contains(relativePos);
    }

    /// <summary>
    /// ユニットが撃破された際の処理
    /// </summary>
    public void OnDefeated()
    {
        // 演出やオブジェクトの破棄処理
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
