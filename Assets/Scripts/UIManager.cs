// Responsibility: Manages the display of current and maximum cost, player base HP, and enemy base HP on screen.
// Attachment Note: Attach to a UI Canvas GameObject in the scene.

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CardManager cardManager;
    [SerializeField] private BaseController playerBase;
    [SerializeField] private BaseController enemyBase;

    [Header("UI Elements")]
    [SerializeField] private Text costText;
    [SerializeField] private Text playerBaseHpText;
    [SerializeField] private Text enemyBaseHpText;

    private void Update()
    {
        UpdateCostDisplay();
        UpdateBaseHpDisplay();
    }

    private void UpdateCostDisplay()
    {
        if (cardManager != null && costText != null)
        {
            costText.text = $"Cost: {cardManager.CurrentCost} / {cardManager.MaxCost}";
        }
    }

    private void UpdateBaseHpDisplay()
    {
        if (playerBase != null && playerBaseHpText != null)
        {
            playerBaseHpText.text = $"Player Base HP: {playerBase.CurrentHp} / {playerBase.MaxHp}";
        }
        else if (playerBaseHpText != null)
        {
            playerBaseHpText.text = "Player Base: Destroyed";
        }

        if (enemyBase != null && enemyBaseHpText != null)
        {
            enemyBaseHpText.text = $"Enemy Base HP: {enemyBase.CurrentHp} / {enemyBase.MaxHp}";
        }
        else if (enemyBaseHpText != null)
        {
            enemyBaseHpText.text = "Enemy Base: Destroyed";
        }
    }
}
