// Responsibility: ScriptableObject defining card properties such as cost, type, unit prefab, and modifiers.
// Attachment Note: Create asset instances via Project window (Create -> Cards -> CardData).

using UnityEngine;

/// <summary>
/// カードのデータを保持するScriptableObjectクラス。
/// </summary>
[CreateAssetMenu(fileName = "NewCardData", menuName = "Cards/CardData")]
public class CardData : ScriptableObject
{
    [Header("Card Info")]
    public string cardName;
    public CardType type;
    public int cost;

    [Header("Prefab / Effects")]
    public GameObject unitPrefab;
    public int powerModifier;
}

public enum CardType
{
    Summon,
    Buff,
    Debuff
}
