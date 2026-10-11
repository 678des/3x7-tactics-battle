using System.Collections;
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

    private GridManager gridManager;
    private BattleProcessor battleProcessor;
    private EnemyAI enemyAI;

    [Header("Base HP Settings")]
    [SerializeField] private int playerBaseHP = 10;
    [SerializeField] private int enemyBaseHP = 10;

    [Header("Cost Settings")]
    [SerializeField] private int maxCost = 20;      // 最大コスト（上限）
    private int currentPlayerCost;                 // プレイヤーの現在のコスト
    private int currentEnemyCost;

    public static GamePhase CurrentPhase { get; private set; }
    public static GameManager Instance;
    public int PlayerBaseHP => playerBaseHP;
    public int EnemyBaseHP => enemyBaseHP;
    public int CurrentPlayerCost => currentPlayerCost;
    public int CurrentEnemyCost => currentEnemyCost;


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

        gridManager = FindAnyObjectByType<GridManager>();
        enemyAI = FindAnyObjectByType<EnemyAI>();
        battleProcessor = FindAnyObjectByType<BattleProcessor>();

        currentEnemyCost = maxCost;
        currentPlayerCost = maxCost;

        CardManager.Instance.DistributeStartingCards();
        StartPhase(GamePhase.Spawn);
    }



    // コストを消費する処理
    public bool TryConsumeCost(bool isPlayer, int amount)
    {
        if (isPlayer)
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

        SoundManager.Instance.PlayAttackBase();
        if (targetTeam == TeamType.Player)
        {
            playerBaseHP -= damage;
            if (playerBaseHP <= 0)
            {
                StartCoroutine(HandleGameEndRoutine(false));
            }
        }
        else
        {
            enemyBaseHP -= damage;
            if (enemyBaseHP <= 0)
            {
                StartCoroutine(HandleGameEndRoutine(true));
            }
        }
    }



    /// <summary>
    /// 1秒待機した後にUIManagerへリザルト表示を依頼するコルーチン
    /// </summary>
    /// <param name="isPlayerWin">プレイヤーが勝ったか</param>
    private IEnumerator HandleGameEndRoutine(bool isPlayerWin)
    {
        // 1秒間しっかり余韻（演出やヒットストップ、エフェクトの残り時間）を待つ
        yield return new WaitForSeconds(2.0f);

        // UIManager経由でリザルト画面を表示する
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowResultScreen(isPlayerWin);
        }
        else
        {
            Debug.LogWarning("[GameManager] UIManager.Instance が見つかりませんでした。");
        }
    }


    public void StartPhase(GamePhase phase)
    {
        CurrentPhase = phase;

        switch (phase)
        {
            case GamePhase.Spawn:
                UIManager.Instance.PhaseEndButtonDisplay(true);
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
            UIManager.Instance.PhaseEndButtonDisplay(false);
            StartPhase(GamePhase.MoveExecution);

        }
        else if (CurrentPhase == GamePhase.Spawn)
        {
            StartPhase(GamePhase.MoveReservation);
        }
    }

}
