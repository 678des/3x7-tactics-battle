using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 手札の管理、コスト消費、カード効果の適用を行うマネージャークラス。
/// </summary>
public class CardManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxHandSize = 5;
    [SerializeField] private int currentCost = 3;
    [SerializeField] private int maxCost = 3;

    [Header("References")]
    [SerializeField] private GridManager gridManager;

    private List<CardData> hand = new List<CardData>();

    private void Awake()
    {
        if (gridManager == null)
            gridManager = GetComponent<GridManager>();
    }

    /// <summary>
    /// コストを全回復する（フェーズ開始時に呼び出し）
    /// </summary>
    public void ResetCost()
    {
        currentCost = maxCost;
    }

    /// <summary>
    /// カードの使用を試みる
    /// </summary>
    /// <param name="card">使用するカードデータ</param>
    /// <param name="targetPosition">対象のグリッド座標</param>
    /// <returns>使用成功可否</returns>
    public bool TryUseCard(CardData card, Vector2Int targetPosition)
    {
        if (currentCost < card.cost)
        {
            Debug.LogWarning("コスト不足です。");
            return false;
        }

        switch (card.type)
        {
            case CardType.Summon:
                if (!gridManager.IsCellOccupied(targetPosition))
                {
                    gridManager.SpawnUnit(card.unitPrefab, targetPosition);
                }
                else
                {
                    return false;
                }
                break;

            case CardType.Buff:
                ApplyBuff(card);
                break;

            case CardType.Debuff:
                ApplyDebuff(card, targetPosition);
                break;
        }

        currentCost -= card.cost;
        return true;
    }

    private void ApplyBuff(CardData card)
    {
        // 全味方ユニットに対してバフを適用するロジック
        var units = gridManager.GetAllUnits();
        foreach (var unit in units)
        {
            if (unit.isPlayerSide)
            {
                unit.ApplyBuff(card.powerModifier);
            }
        }
    }

    private void ApplyDebuff(CardData card, Vector2Int targetPosition)
    {
        // 特定マスへの妨害処理
        gridManager.SetCellObstacle(targetPosition, true);
    }
}

public enum CardType
{
    Summon,
    Buff,
    Debuff
}

[System.Serializable]
public class CardData
{
    public string cardName;
    public CardType type;
    public int cost;
    public GameObject unitPrefab; // 召喚用
    public int powerModifier;     // バフ用
}
