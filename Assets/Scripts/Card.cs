// Responsibility: Visual representation of a card in the player's hand.
// Attachment Note: Attach to a Card Prefab GameObject.

using UnityEngine;

/// <summary>
/// 手札のカードのビジュアル表示と選択状態を管理するクラス。
/// </summary>
public class Card : MonoBehaviour
{
    [SerializeField] private CardData cardData;

    private Vector3 originalPosition = new();
    public CardData CardData => cardData;

    private void Awake()
    {
        originalPosition = transform.localPosition;
    }
    private void OnEnable()
    {
        originalPosition = transform.localPosition;
    }



    public void SetSelected(bool selected)
    {

        if (selected)
        {

            CardManager.currentPlayerSelectedCard = this;
            transform.localPosition = new Vector3(transform.localPosition.x, 0.6f, transform.localPosition.z);
        }
        else
        {
            CardManager.currentPlayerSelectedCard = null;
            transform.localPosition = originalPosition;
        }
    }


    private void OnMouseDown()
    {
        CardManager.Instance.AllCardViewController(this);
    }
}
