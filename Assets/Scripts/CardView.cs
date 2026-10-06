// Responsibility: Visual representation of a card in the player's hand.
// Attachment Note: Attach to a Card Prefab GameObject.

using UnityEngine;

/// <summary>
/// 手札のカードのビジュアル表示と選択状態を管理するクラス。
/// </summary>
[RequireComponent(typeof(UnityEngine.RectTransform))]
public class CardView : MonoBehaviour
{
    [SerializeField] private CardData cardData;
    [SerializeField] private CardManager cardManager;

    private bool isSelected = false;

    public CardData CardData => cardData;

    public void Initialize(CardData data, CardManager manager)
    {
        cardData = data;
        cardManager = manager;
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        // 選択状態に応じた視覚表現の切り替えをここに記述できます
    }

    public void OnCardClicked()
    {
        if (cardManager != null)
        {
            cardManager.SelectCard(cardData, this);
        }
    }
}
