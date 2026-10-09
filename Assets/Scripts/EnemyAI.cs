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
    public void ExecuteEnemySpawnPhase()
    {
        Debug.Log("今から敵スポーン");

        // 1. 敵陣（奥の3列、行Rows-1 から Rows-3 あたり）の空きマスを動的に取得する
        List<Vector2Int> availableSpawnCells = new List<Vector2Int>();
        int rows = 7; // GridManagerの定数やプロパティに合わせて調整してください
        int cols = 3;

        // 奥の陣地（例: 行 4, 5, 6）の空きマスを探索
        for (int r = rows - 1; r >= rows - 3 && r >= 0; r--)
        {
            for (int c = 0; c < cols; c++)
            {
                // GridManagerの既存メソッドを使って、ユニットや障害物がないか確認
                if (gridManager.CanPlaceUnit(c, r))
                {
                    availableSpawnCells.Add(new Vector2Int(c, r));
                }
            }
        }
        Debug.Log($"{availableSpawnCells[0]}スポーン可能範囲");
        // 2. 簡易AIのスポーンループ（コストが許す限り、ランダムにユニットを召喚）
        while (enemyCurrentCost > 0 && availableSpawnCells.Count > 0)
        {
            // A. コスト内で出せる「召喚（Summon）カード」を抽出
            List<CardData> affordableSummonCards = new List<CardData>();

            // 修正内容: GameObject から CardData を直接参照できないため、GetComponent<CardView>() で CardData を取得するよう修正
            // 既存の foreach ループ内の該当箇所を修正

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


            // Debug.Log($"{affordableSummonCards[0]}敵の所持するカード");

            // 出せる召喚カードがない場合はループを抜ける
            if (affordableSummonCards.Count == 0)
            {
                Debug.Log("敵の出せるカードがない");
                break;
            }

            // B. ランダムに1枚選ぶ
            int randomCardIndex = Random.Range(0, affordableSummonCards.Count);
            CardData selectedCard = affordableSummonCards[randomCardIndex];
            Debug.Log($"{selectedCard}ランダムに1枚");

            // C. ランダムに空きマスを1つ選ぶ
            int randomCellIndex = Random.Range(0, availableSpawnCells.Count);
            Vector2Int spawnPos = availableSpawnCells[randomCellIndex];

            // D. ユニットをスポーン（敵チーム用の回転や設定を適用）
            // ※必要に応じてチームやプレハブの向きを調整してください
            Quaternion enemyRotation = Quaternion.Euler(0, -90f, 0); // 必要に応じた向き

            gridManager.SpawnUnit(selectedCard.unitPrefab, spawnPos, enemyRotation);

            // E. 生成したユニットのチームフラグ（IsPlayerOwnedなど）を敵側に設定する（もしSpawnUnit内で設定されていない場合）
            Unit spawnedUnit = gridManager.GetUnitAt(spawnPos);
            if (spawnedUnit != null)
            {
                // ユニット側に IsPlayerOwned などの設定があれば false（敵）にする
                // spawnedUnit.IsPlayerOwned = false; 
            }

            // F. コストを消費し、手札から除外、選んだマスをリストから削除
            enemyCurrentCost -= selectedCard.cost;
            //nemyHand.Remove(selectedCard);
            availableSpawnCells.RemoveAt(randomCellIndex);

            Debug.Log($"敵が [{spawnPos.x}, {spawnPos.y}] にカード [{selectedCard.cardName}] でユニットを召喚しました。残りコスト: {enemyCurrentCost}");
        }

        Debug.Log("敵スポーンフェーズ終了");
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
            return; // ターゲットがいない場合は何もしない

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
                Vector2Int bestMovePos = CalculateBestMove(enemy, enemyPos, playerPos);

                // GridManager側のメソッド名（ReserveMove）に合わせる
                gridManager.ReserveMove(enemy, bestMovePos);
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

    /// <summary>
    /// 敵の移動可能範囲の中から、ターゲットに最も近づけるマスを計算する
    /// </summary>
    private Vector2Int CalculateBestMove(Unit enemy, Vector2Int currentPos, Vector2Int targetPosition)
    {
        // GridManager側にある GetMovableCells を利用
        List<Vector2Int> movableCells = gridManager.GetMovableCells(enemy);

        Vector2Int bestPosition = currentPos; // 動かない場合の初期値
        float minDistance = Vector2Int.Distance(currentPos, targetPosition);

        foreach (var cell in movableCells)
        {
            // すでに埋まっているマスは除外
            if (gridManager.IsCellOccupied(cell)) continue;

            float dist = Vector2Int.Distance(cell, targetPosition);
            if (dist < minDistance)
            {
                minDistance = dist;
                bestPosition = cell;
            }
        }

        return bestPosition;
    }
}