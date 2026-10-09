// Responsibility: Manages the display of current and maximum cost, player base HP, enemy base HP, and current game phase on screen.
// Attachment Note: Attach to a UI Canvas GameObject in the scene.

using TMPro;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CardManager cardManager;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI playerCostText;
    [SerializeField] private TextMeshProUGUI enemyCostText;
    [SerializeField] private TextMeshProUGUI playerBaseHpText;
    [SerializeField] private TextMeshProUGUI enemyBaseHpText;
    [SerializeField] private TextMeshProUGUI phaseText;

    private void Update()
    {
        UpdateCostDisplay();
        UpdateBaseHpDisplay();
        UpdatePhaseDisplay();
    }

    private void UpdateCostDisplay()
    {

        playerCostText.text = GameManager.Instance.CurrentPlayerCost.ToString() + " / 20";
        enemyCostText.text = GameManager.Instance.CurrentEnemyCost.ToString() + " / 20";

    }

    private void UpdateBaseHpDisplay()
    {

        playerBaseHpText.text = "Player HP: " + GameManager.Instance.PlayerBaseHP.ToString();
        enemyBaseHpText.text = "Enemy HP: " + GameManager.Instance.EnemyBaseHP.ToString();

    }

    private void UpdatePhaseDisplay()
    {
        if (phaseText != null)
        {
            phaseText.text = "Phase: " + GameManager.CurrentPhase.ToString();
        }
    }
}
