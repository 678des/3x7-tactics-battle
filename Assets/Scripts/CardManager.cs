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

    [Header("Card Database / Hand")]
    [SerializeField] private List<CardData> hand = new List<CardData>();

    public bool IsCardSelected { get; private set; }
    private CardData selectedCard;
    private CardView selectedView;

    public int CurrentCost => currentCost;
    public int MaxCost => maxCost;

    private void Awake()
    {
        if (gridManager == null)
            gridManager = GetComponent<GridManager>();
    }

    public void ResetCost()
    {
        currentCost = maxCost;
    }

    public void SelectCard(CardData card, CardView view)
    {
        if (selectedView != null)
        {
            selectedView.SetSelected(false);
        }

        selectedCard = card;
        selectedView = view;
        IsCardSelected = (card != null);

        if (selectedView != null)
        {
            selectedView.SetSelected(true);
        }
    }

    public void UseSelectedCard(Vector2Int targetPosition)
    {
        if (!IsCardSelected || selectedCard == null)
            return;

        if (TryUseCard(selectedCard, targetPosition))
        {
            if (selectedView != null)
            {
                selectedView.SetSelected(false);
                Destroy(selectedView.gameObject);
            }
            selectedCard = null;
            selectedView = null;
            IsCardSelected = false;
        }
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
