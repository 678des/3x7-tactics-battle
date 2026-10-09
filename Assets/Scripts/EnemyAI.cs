using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵AIの思考と移動予約を管理するクラス。
/// 一番近い味方のマスに向かって移動するロジックを実行する。
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Cost Settings")]
    [SerializeField] private int enemyCurrentCost = 3;
    [SerializeField] private int enemyMaxCost = 3;

    [Header("References")]
    [SerializeField] private GridManager gridManager;
    private void Awake()
    {
        if (gridManager == null)
            gridManager = GetComponent<GridManager>();
    }

    /// <summary>
    /// 敵のスポーンフェーズを実行する処理（コストがある限り、奥の陣地にランダムでユニットを召喚する）
    /// </summary>
    /// <summary>
    /// 敵のスポーンフェーズを実行する処理（コストがある限り、奥の陣地にランダムでユニットを召喚する）
    /// </summary>
    public void ExecuteEnemySpawnPhase()
    {

        List<Vector2Int> availableSpawnCells = new List<Vector2Int>();
        int rows = 7;
        int cols = 3;

        // 奥の陣地（例: 行 4, 5, 6）の空きマスを探索
        for (int r = rows - 1; r >= rows - 3 && r >= 0; r--)
        {
            for (int c = 0; c < cols; c++)
            {
                if (gridManager.CanPlaceUnit(c, r))
                {
                    availableSpawnCells.Add(new Vector2Int(c, r));
                }
            }
        }

        if (availableSpawnCells.Count > 0)
        {
            Debug.Log($"{availableSpawnCells[0]}スポーン可能範囲");
        }

        // 簡易AIのスポーンループ
        while (enemyCurrentCost > 0 && availableSpawnCells.Count > 0)
        {
            List<CardData> affordableSummonCards = new List<CardData>();

            foreach (GameObject cardView in GameManager.Instance.enemyStartingCards)
            {
                if (cardView != null)
                {
                    var cardViewComponent = cardView.GetComponent<CardView>();
                    if (cardViewComponent != null && cardViewComponent.CardData != null &&
                        cardViewComponent.CardData.type == CardType.Summon &&
                        cardViewComponent.CardData.cost <= enemyCurrentCost)
                    {
                        affordableSummonCards.Add(cardViewComponent.CardData);
                    }
                }
            }

            if (affordableSummonCards.Count == 0)
            {
                break;
            }

            // ランダムにカードと空きマスを選ぶ
            int randomCardIndex = Random.Range(0, affordableSummonCards.Count);
            CardData selectedCard = affordableSummonCards[randomCardIndex];

            int randomCellIndex = Random.Range(0, availableSpawnCells.Count);
            Vector2Int spawnPos = availableSpawnCells[randomCellIndex];

            Quaternion enemyRotation = Quaternion.Euler(0, -90f, 0);

            // 💡 修正ポイント：SpawnUnitの引数に `false`（敵チーム）を渡すことで、
            //    自動的に「グリッド配置」「リスト登録」「敵所属の設定」が一度に行われます。
            gridManager.SpawnUnit(selectedCard.unitPrefab, spawnPos, enemyRotation, false);

            // コストを消費し、選んだマスをリストから削除
            enemyCurrentCost -= selectedCard.cost;
            availableSpawnCells.RemoveAt(randomCellIndex);

            //Debug.Log($"敵が [{spawnPos.x}, {spawnPos.y}] にカード [{selectedCard.cardName}] でユニットを召喚しました。残りコスト: {enemyCurrentCost}");
        }

    }
    /// <summary>
    /// 敵のターン（移動フェーズ開始時）に呼ばれ、全敵ユニットの移動先を決定・予約する。
    /// </summary>
    public void ExecuteEnemyTurn()
    {
        if (gridManager == null) return;

        // 1. フィールド上の敵ユニットと味方ユニットのリストを取得
        List<Unit> enemyUnits = gridManager.GetUnitsByTeam(false);
        List<Unit> playerUnits = gridManager.GetUnitsByTeam(true);

        if (playerUnits.Count == 0 || enemyUnits.Count == 0)
            return;


        // 2. 各敵ユニットごとに移動先を決定
        foreach (var enemy in enemyUnits)
        {
            Vector2Int enemyPos = gridManager.GetUnitPosition(enemy);
            if (!gridManager.IsValidCoordinate(enemyPos.x, enemyPos.y)) continue;

            // 一番近い味方ユニットを探す
            Unit nearestPlayer = FindNearestUnit(enemyPos, playerUnits);
            if (nearestPlayer != null)
            {
                Vector2Int playerPos = gridManager.GetUnitPosition(nearestPlayer);

                // 移動可能な範囲から、一番「ターゲットに近づけるマス」を選んで予約
                //Vector2Int bestMovePos = CalculateBestMove(enemy, enemyPos, playerPos);

                //// GridManager側のメソッド名（ReserveMove）に合わせる
                //gridManager.ReserveMove(enemy, bestMovePos);
            }
        }
    }

    /// <summary>
    /// 指定された位置から最も近い味方ユニットを見つける
    /// </summary>
    private Unit FindNearestUnit(Vector2Int fromPosition, List<Unit> targets)
    {
        Unit nearest = null;
        float minDistance = float.MaxValue;

        foreach (var target in targets)
        {
            Vector2Int targetPos = gridManager.GetUnitPosition(target);
            float dist = Vector2Int.Distance(fromPosition, targetPos);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = target;
            }
        }
        return nearest;
    }


}