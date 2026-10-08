// Responsibility: Mobile touch and mouse input detection for triggering card usage.
// Attachment Note: Attach to a persistent GameObject or GameManager GameObject in the scene.

using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// モバイルタッチおよびマウス入力を検知し、ゲーム内のアクション（カード使用等）をトリガーするクラス。
/// </summary>
[RequireComponent(typeof(UnityEngine.Transform))]
public class InputHandler : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask cardLayerMask;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit cardHit, 100f, cardLayerMask))
        {
            CardView cardView = cardHit.collider.GetComponent<CardView>();
            if (cardView != null)
            {
                CardManager cardManager = FindFirstObjectByType<CardManager>();
                if (cardManager != null)
                {
                    cardManager.SelectCard(cardView.CardData, cardView);
                }
            }
        }
    }
}
