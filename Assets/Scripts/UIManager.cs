// Responsibility: Manages the display of current and maximum cost, player base HP, enemy base HP, and current game phase on screen.
// Attachment Note: Attach to a UI Canvas GameObject in the scene.

using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class UIManager : MonoBehaviour
{

    public static UIManager Instance;

    [Header("UI Panels")]
    [SerializeField] private GameObject _resultPanel; // リザルト画面全体のUIパネル
    [SerializeField] private TextMeshProUGUI _resultText;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI playerCostText;
    [SerializeField] private TextMeshProUGUI enemyCostText;
    [SerializeField] private TextMeshProUGUI playerBaseHpText;
    [SerializeField] private TextMeshProUGUI enemyBaseHpText;
    [SerializeField] private TextMeshProUGUI phaseText;
    [SerializeField] private Button phaseEndButton;

    private void Awake()
    {
        Instance = this;
        _resultPanel.SetActive(false);
    }

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

    public void PhaseEndButtonDisplay(bool enabled)
    {
        phaseEndButton.enabled = enabled;
    }

    /// <summary>
    /// ゲーム終了時に呼び出されるリザルト画面表示メソッド
    /// </summary>
    /// <param name="isPlayerWin">trueなら勝利、falseなら敗北</param>
    public void ShowResultScreen(bool isPlayerWin)
    {
        PhaseEndButtonDisplay(false);
        if (_resultPanel != null)
        {
            _resultPanel.SetActive(true);
        }

        // 勝利・敗北に応じたテキストの切り替えなどがあればここに書く
        if (isPlayerWin)
        {
            _resultText.text = "VICTORY";
        }
        else
        {
            _resultText.text = "DEFEAT";
        }
    }
}
