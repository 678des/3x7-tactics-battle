// Responsibility: Visual representation of a card in the player's hand.
// Attachment Note: Attach to a Card Prefab GameObject.

using UnityEngine;

/// <summary>
/// 手札のカードのビジュアル表示と選択状態を管理するクラス。
/// </summary>
public class CardView : MonoBehaviour
{
    [SerializeField] private CardData cardData;
    [SerializeField] private CardManager cardManager;

    private bool isSelected = false;
    private Vector3 originalPosition;

    public CardData CardData => cardData;

    private void Awake()
    {
        originalPosition = transform.localPosition;
    }

    private void Start()
    {
        if (cardManager == null)
        {
            cardManager = Object.FindAnyObjectByType<CardManager>();
        }
    }

    public void Initialize(CardData data, CardManager manager)
    {
        cardData = data;
        cardManager = manager;
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (isSelected)
        {
            transform.localPosition = originalPosition + new Vector3(0f, 1, 0f);
        }
        else
        {
            transform.localPosition = originalPosition;
        }
    }


    private void OnMouseDown()
    {
        Debug.Log(gameObject.name + " がクリックされました！");

        if (cardManager != null)
        {
            cardManager.SelectCard(cardData, this);
        }
    }
}
