using UnityEngine;
using System;

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

    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private BattleProcessor battleProcessor;
    [SerializeField] private EnemyAI enemyAI;

    [Header("Initial Setup")]
    [SerializeField] private System.Collections.Generic.List<GameObject> playerStartingCards;
    [SerializeField] private System.Collections.Generic.List<GameObject> enemyStartingCards;

    public static GamePhase CurrentPhase { get; private set; }

    private void Awake()
    {
        // 依存関係のキャッシュ
        if (gridManager == null) gridManager = GetComponent<GridManager>();
        if (battleProcessor == null) battleProcessor = GetComponent<BattleProcessor>();
        if (enemyAI == null) enemyAI = GetComponent<EnemyAI>();
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

    /// <summary>
    /// フェーズを開始する。
    /// </summary>
    public void StartPhase(GamePhase phase)
    {
        CurrentPhase = phase;

        switch (phase)
        {
            case GamePhase.Spawn:
                // コスト全回復処理などをここに記述
                break;
            case GamePhase.MoveReservation:
                // プレイヤーの入力を許可
                break;
            case GamePhase.MoveExecution:
                ExecuteMovement();
                break;
            case GamePhase.Battle:
                ExecuteBattle();
                break;
        }
    }

    /// <summary>
    /// 移動フェーズの処理。味方と敵の予約済み移動を一斉に実行する。
    /// </summary>
    private void ExecuteMovement()
    {
        // 1. 敵AIの移動予約を確定させる
        enemyAI.ExecuteEnemyTurn();

        // 2. GridManagerを通じて全ユニットを移動させる
        gridManager.ProcessAllUnitMovements();

        // 移動完了後、バトルフェーズへ移行
        StartPhase(GamePhase.Battle);
    }

    /// <summary>
    /// バトルフェーズの処理。全ユニットの攻撃判定と拠点ダメージを計算する。
    /// </summary>
    private void ExecuteBattle()
    {
        // BattleProcessorに判定を委譲
        battleProcessor.ResolveAllBattles();

        // バトル終了後、次のターンのスポーンフェーズへ
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
    }
}
