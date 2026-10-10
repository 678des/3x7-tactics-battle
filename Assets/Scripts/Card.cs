// Responsibility: Visual representation of a card in the player's hand.
// Attachment Note: Attach to a Card Prefab GameObject.

using UnityEngine;

/// <summary>
/// 手札のカードのビジュアル表示と選択状態を管理するクラス。
/// </summary>
public class Card : MonoBehaviour
{
    [SerializeField] private CardData cardData;
    private CardManager cardManager;

    private bool isSelected = false;

    private Vector3 originalPosition = new Vector3(0, 0.3f, -0.4f);
    public CardData CardData => cardData;

    private void Awake()
    {
        //originalPosition = transform.localPosition;
        //transform.rotation = originalQuaternion;
    }

    private void Start()
    {
        cardManager = FindAnyObjectByType<CardManager>();
    }


    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (isSelected)
        {
            CardManager.currentPlayerSelectedCard = this;
            transform.localPosition += new Vector3(0f, 1, 0f);
        }
        else transform.localPosition = originalPosition;

    }


    private void OnMouseDown()
    {
        Debug.Log(gameObject.name + " がクリックされました！");
        SetSelected(true);


    }
}
