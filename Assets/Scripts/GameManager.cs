using UnityEngine;

/// <summary>
/// ゲームのフェーズ遷移を管理するメインコントローラー。
/// スポーン、移動予約、移動実行、バトル処理のサイクルを制御する。
/// </summary>
public class GameManager : MonoBehaviour
{
    public enum GamePhase
    {
        Spawn,
        MoveReservation,
        MoveExecution,
        Battle
    }


    [Header("Base HP Settings")]
    [SerializeField] private int playerBaseHP = 10;
    [SerializeField] private int enemyBaseHP = 10;
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private BattleProcessor battleProcessor;
    [SerializeField] private EnemyAI enemyAI;
    [Header("Initial Setup")]
    [SerializeField] private System.Collections.Generic.List<GameObject> playerStartingCards;
    public System.Collections.Generic.List<GameObject> enemyStartingCards;
    [Header("Cost Settings")]
    [SerializeField] private int maxCost = 5;      // 最大コスト（上限）
    private int currentPlayerCost;                 // プレイヤーの現在のコスト
    private int currentEnemyCost;

    public static GamePhase CurrentPhase { get; private set; }
    public static GameManager Instance;
    public int PlayerBaseHP => playerBaseHP;
    public int EnemyBaseHP => enemyBaseHP;
    public int CurrentPlayerCost => currentPlayerCost;
    public int CurrentEnemyCost => currentEnemyCost;


    // フェーズが「Spawn」に入ったときにコストを全回復させる
    public void ResetCostForNewPhase()
    {
        currentPlayerCost = maxCost;
        currentEnemyCost = maxCost;
        Debug.Log("コストが全回復しました！");
    }

    // コストを消費する処理
    public bool TryConsumeCost(TeamType team, int amount)
    {
        if (team == TeamType.Player)
        {
            if (currentPlayerCost >= amount)
            {
                currentPlayerCost -= amount;
                return true; // 消費成功
            }
        }
        else
        {
            if (currentEnemyCost >= amount)
            {
                currentEnemyCost -= amount;
                return true; // 消費成功
            }
        }
        return false; // コスト不足
    }

    /// <summary>
    /// 指定されたチームの拠点にダメージを与える
    /// </summary>
    public void DamageBase(TeamType targetTeam, int damage)
    {
        if (targetTeam == TeamType.Player)
        {
            playerBaseHP -= damage;
            Debug.Log($"【プレイヤー拠点】が攻撃を受けた！ 残りHP: {playerBaseHP}");
            if (playerBaseHP <= 0)
            {
                Debug.Log("ゲームオーバー：敵の勝利です");
                // TODO: 敗北演出やリザルト画面への遷移
            }
        }
        else
        {
            enemyBaseHP -= damage;
            Debug.Log($"【敵の拠点】が攻撃を受けた！ 残りHP: {enemyBaseHP}");
            if (enemyBaseHP <= 0)
            {
                Debug.Log("ゲームクリア：プレイヤーの勝利です！");
                // TODO: 勝利演出やリザルト画面への遷移
            }
        }
    }

    private void Awake()
    {
        // 依存関係のキャッシュ
        if (gridManager == null) gridManager = GetComponent<GridManager>();
        if (battleProcessor == null) battleProcessor = GetComponent<BattleProcessor>();
        if (enemyAI == null) enemyAI = GetComponent<EnemyAI>();
        Instance = this;
    }

    private void Start()
    {
        DistributeStartingCards();
        StartPhase(GamePhase.Spawn);
    }

    /// <summary>
    /// ゲームスタート時にプレイヤーと敵の陣地に手持ちのカードを均等に配置する
    /// </summary>
    private void DistributeStartingCards()
    {
        if (gridManager != null)
        {
            if (playerStartingCards != null && playerStartingCards.Count > 0)
            {
                gridManager.SpawnCardsForTeam(playerStartingCards, TeamType.Player);
            }
            if (enemyStartingCards != null && enemyStartingCards.Count > 0)
            {
                gridManager.SpawnCardsForTeam(enemyStartingCards, TeamType.Enemy);
            }
        }
    }

    public void StartPhase(GamePhase phase)
    {
        CurrentPhase = phase;

        switch (phase)
        {
            case GamePhase.Spawn:
                enemyAI.ExecuteEnemySpawnPhase();
                break;
            case GamePhase.MoveReservation:
                enemyAI.ExecuteEnemyReservePhase();
                break;
            case GamePhase.MoveExecution:
                gridManager.ProcessAllUnitMovements();
                StartPhase(GamePhase.Battle);
                break;
            case GamePhase.Battle:
                ExecuteBattle();
                break;
        }
    }

    /// <summary>
    /// 移動フェーズの処理。味方と敵の予約済み移動を一斉に実行する。
    /// </summary>


    /// <summary>
    /// バトルフェーズの処理。全ユニットの攻撃判定と拠点ダメージを計算する。
    /// </summary>
    private void ExecuteBattle()
    {
        Debug.Log("バトル実行");
        battleProcessor.ExecuteBattle();
        StartPhase(GamePhase.Spawn);
    }

    /// <summary>
    /// プレイヤーがターン終了ボタンを押した際に呼ばれる想定のメソッド。
    /// </summary>
    public void OnEndTurnButtonClicked()
    {
        if (CurrentPhase == GamePhase.MoveReservation)
        {
            StartPhase(GamePhase.MoveExecution);
        }
        else if (CurrentPhase == GamePhase.Spawn)
        {
            StartPhase(GamePhase.MoveReservation);
        }
    }

}
