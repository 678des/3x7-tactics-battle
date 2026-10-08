// Responsibility: Manages the display of current and maximum cost, player base HP, enemy base HP, and current game phase on screen.
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
    [SerializeField] private Text phaseText;

    private void Update()
    {
        UpdateCostDisplay();
        UpdateBaseHpDisplay();
        UpdatePhaseDisplay();
    }

    private void UpdateCostDisplay()
    {
        if (cardManager != null && costText != null)
        {
            costText.text = cardManager.CurrentCost.ToString() + " / " + cardManager.MaxCost.ToString();
        }
    }

    private void UpdateBaseHpDisplay()
    {
        if (playerBase != null && playerBaseHpText != null)
        {
            playerBaseHpText.text = "Player HP: " + playerBase.CurrentHp.ToString();
        }

        if (enemyBase != null && enemyBaseHpText != null)
        {
            enemyBaseHpText.text = "Enemy HP: " + enemyBase.CurrentHp.ToString();
        }
    }

    private void UpdatePhaseDisplay()
    {
        if (GameManager.Instance != null && phaseText != null)
        {
            phaseText.text = "Phase: " + GameManager.Instance.CurrentPhase.ToString();
        }
    }
}
