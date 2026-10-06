// Responsibility: Hand management, cost consumption, and card effect application.
// Attachment Note: Attach to a GameManager or CardManager GameObject in the scene.

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 手札の管理、コスト消費、カード効果の適用を行うマネージャークラス。
/// </summary>
[RequireComponent(typeof(UnityEngine.Transform))]
public class CardManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int currentCost = 3;
    [SerializeField] private int maxCost = 3;

    [Header("References")]
    [SerializeField] private GridManager gridManager;

    private List<CardData> hand = new List<CardData>();

    public bool IsCardSelected { get; private set; }
    private CardData selectedCard;

    private void Awake()
    {
        if (gridManager == null)
            gridManager = GetComponent<GridManager>();
    }

    public void ResetCost()
    {
        currentCost = maxCost;
    }

    public void SelectCard(CardData card)
    {
        selectedCard = card;
        IsCardSelected = (card != null);
    }

    public void UseSelectedCard(Vector2Int targetPosition)
    {
        if (!IsCardSelected || selectedCard == null)
            return;

        TryUseCard(selectedCard, targetPosition);
        selectedCard = null;
        IsCardSelected = false;
    }

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
    public GameObject unitPrefab;
    public int powerModifier;
}
