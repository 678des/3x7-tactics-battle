using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵AIの思考と移動予約を管理するクラス。
/// 一番近い味方のマスに向かって移動するロジックを実行する。
/// </summary>
/// // AIの「移動の候補」を1つにまとめるデータ入れ
public struct AIMoveCandidate
{
    public Unit unit;         // 動かすキャラクター
    public Vector2Int targetPos; // 移動先の座標
    public float score;       // その評価値（点数）
}

public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Cost Settings")]
    [SerializeField] private int enemyCurrentCost = 3;

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

        // 簡易AIのスポーンループ
        while (enemyCurrentCost > 0 && availableSpawnCells.Count > 0)
        {
            List<CardData> affordableSummonCards = new List<CardData>();

            foreach (GameObject cardView in CardManager.Instance.EnemyStartingCards)
            {
                if (cardView != null)
                {
                    var cardViewComponent = cardView.GetComponent<Card>();
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

            Quaternion enemyRotation = Quaternion.Euler(-90, 0f, 0);

            gridManager.SpawnUnit(selectedCard.unitPrefab, spawnPos, enemyRotation, false);

            // コストを消費し、選んだマスをリストから削除
            enemyCurrentCost -= selectedCard.cost;
            availableSpawnCells.RemoveAt(randomCellIndex);

            //Debug.Log($"敵が [{spawnPos.x}, {spawnPos.y}] にカード [{selectedCard.cardName}] でユニットを召喚しました。残りコスト: {enemyCurrentCost}");
        }

    }


    /// <summary>
    /// フィールドにいるすべての敵の、すべての移動可能先を網羅してリストにして返す関数
    /// </summary>
    public List<AIMoveCandidate> GetAllEnemyMoveCandidates()
    {
        List<AIMoveCandidate> allCandidates = new List<AIMoveCandidate>();
        if (gridManager == null) return allCandidates;

        // 1. フィールド上のすべての敵ユニットのリストを取得
        List<Unit> enemyUnits = gridManager.GetUnitsByTeam(false);

        foreach (var enemy in enemyUnits)
        {
            if (enemy == null) continue;

            // 敵の現在地を取得
            Vector2Int enemyPos = gridManager.GetUnitPosition(enemy);
            if (!gridManager.IsValidCoordinate(enemyPos.x, enemyPos.y)) continue;

            // 2. この敵が移動できるマスのリストを取得する
            List<Vector2Int> movableCells = enemy.GetMovalePositions(enemyPos);

            // 3. 取得した移動先マスを、1つずつ構造体に詰めてリストに追加していく
            foreach (var targetPos in movableCells)
            {
                //すでに他のユニットがいるマス」を移動先から除外したい場合
                //if (targetPos != enemyPos && gridManager.IsCellOccupied(targetPos)) continue;


                AIMoveCandidate candidate = new AIMoveCandidate
                {
                    unit = enemy,
                    targetPos = targetPos,
                    score = 0f
                };

                allCandidates.Add(candidate);
            }
        }

        return allCandidates;
    }


    /// <summary>
    /// 移動フェーズ開始時に呼ばれ、全敵の全移動候補を洗い出し、
    /// スコア（評価値）を計算して最も良い移動先を予約する。
    /// </summary>
    public void ExecuteEnemyReservePhase()
    {
        if (gridManager == null) return;

        // 1. すべての敵の移動候補（全敵の動ける全マス）をリストアップする
        List<AIMoveCandidate> allCandidates = GetAllEnemyMoveCandidates();
        if (allCandidates.Count == 0) return;

        // 2. 「リストに入ったすべての候補に対して、スコア（評価値）を計算して代入する」
        for (int i = 0; i < allCandidates.Count; i++)
        {
            AIMoveCandidate candidate = allCandidates[i];

            // 評価関数を呼んでスコアを計算し、構造体に代入する
            candidate.score = EvaluateCandidate(candidate);

            // 構造体は値渡しなので、リストの要素を上書きして更新する
            allCandidates[i] = candidate;
        }

        // 3. スコアが最も高い候補を1つだけ選ぶ（同値の場合は最初のもの）
        AIMoveCandidate bestCandidate = allCandidates[0];
        float highestScore = -9999f;

        foreach (var candidate in allCandidates)
        {
            if (candidate.score > highestScore)
            {
                highestScore = candidate.score;
                bestCandidate = candidate;
            }
        }

        // 4. 選ばれた最善の移動先を GridManager に予約する
        if (bestCandidate.unit != null)
        {
            gridManager.ReserveMove(bestCandidate.unit, bestCandidate.targetPos);
            Debug.Log($"[敵AI] {bestCandidate.unit.name} が {bestCandidate.targetPos} への移動を予約しました。（評価スコア: {highestScore}）");
        }
    }


    //private float EvaluateCandidate(AIMoveCandidate candidate)
    //{
    //    float score = 0f;
    //    Vector2Int pos = candidate.targetPos;
    //    Unit enemyUnit = candidate.unit; // 動こうとしている敵ユニット自身

    //    // フィールド上のすべてのプレイヤー（味方）ユニットを取得
    //    List<Unit> playerUnits = gridManager.GetUnitsByTeam(true);

    //    if (playerUnits.Count == 0)
    //    {
    //        // プレイヤーがいない場合は、とりあえず前進（yが小さくなる方向など）を評価
    //        return -pos.y;
    //    }

    //    // ① 一番近いプレイヤーユニットとの距離を測る（近いほど高得点）
    //    float minDistanceToPlayer = 999f;
    //    Unit nearestPlayer = null;

    //    foreach (var player in playerUnits)
    //    {
    //        if (player == null) continue;
    //        Vector2Int playerPos = gridManager.GetUnitPosition(player);

    //        // マンハッタン距離 (|x1 - x2| + |y1 - y2|)
    //        float dist = Mathf.Abs(pos.x - playerPos.x) + Mathf.Abs(pos.y - playerPos.y);
    //        if (dist < minDistanceToPlayer)
    //        {
    //            minDistanceToPlayer = dist;
    //            nearestPlayer = player; // 一番近い味方を記録しておく
    //        }
    //    }

    //    // 基本スコア：プレイヤーに近づくほど高得点（例: 20から距離を引く）
    //    score += (20f - minDistanceToPlayer);

    //    // ②一番近いプレイヤーとの相性を判定して評価を調整する
    //    if (nearestPlayer != null && enemyUnit != null)
    //    {
    //        // Unit側、あるいは BattleProcessor 側にある属性相性判定を呼び出す
    //        int advantage = BattleProcessor.Instance.GetElementAdvantage(enemyUnit.Element, nearestPlayer.Element);

    //        if (advantage < 0 && minDistanceToPlayer <= 1f) score -= 500f;
    //        else if (advantage < 0) score -= 300f;
    //        else if (advantage > 0) score += 15f;

    //    }

    //    Debug.Log($"{candidate.unit.name} が {pos} に移動した場合のスコア: {score}点");
    //    return score;
    //}
    /// <summary>
    /// 1つの移動候補（AIMoveCandidate）に対する「評価値（点数）」を計算する関数
    /// </summary>
    private float EvaluateCandidate(AIMoveCandidate candidate)
    {
        float score = 0f;
        Vector2Int pos = candidate.targetPos;
        Unit enemyUnit = candidate.unit; // 動こうとしている敵ユニット自身

        // フィールド上のすべてのプレイヤー（味方）ユニットを取得
        List<Unit> playerUnits = gridManager.GetUnitsByTeam(true);

        if (playerUnits.Count == 0)
        {
            return -pos.y; // プレイヤーがいない場合は前進
        }

        // ① 一番近いプレイヤーとの距離、およびその相性を調べる
        float minDistanceToPlayer = 999f;
        Unit nearestPlayer = null;
        int nearestAdvantage = 0;

        foreach (var player in playerUnits)
        {
            if (player == null) continue;
            Vector2Int playerPos = gridManager.GetUnitPosition(player);

            float dist = Mathf.Abs(pos.x - playerPos.x) + Mathf.Abs(pos.y - playerPos.y);
            if (dist < minDistanceToPlayer)
            {
                minDistanceToPlayer = dist;
                nearestPlayer = player;
            }
        }

        if (nearestPlayer != null && enemyUnit != null)
        {
            // この「一番近いプレイヤー」との相性を取得 (-1: 不利, 0: 互角, 1: 有利)
            nearestAdvantage = BattleProcessor.Instance.GetElementAdvantage(enemyUnit.Element, nearestPlayer.Element);
        }

        // ② 相性に応じた距離の評価（ここがポイント！）
        if (nearestAdvantage > 0)
        {
            // 【有利な相手】：近づくほどスコアが高くなる（積極的に詰める）
            score += (20f - minDistanceToPlayer) * 1.5f;
            score += 50f; // 有利な相手がいること自体のボーナス
        }
        else if (nearestAdvantage < 0)
        {
            // 【不利な相手】：**離れるほどスコアが高くなる（逃げる・距離を取る）**
            // 逆に「minDistanceToPlayer」が大きいほど高得点にする
            score += minDistanceToPlayer * 2.0f;
            score -= 100f; // 不利な相手が近くにいることへの強烈なペナルティ
        }
        else
        {
            // 【互角の相手】：程よい距離感を保つ（例: 近すぎず遠すぎずの中間を評価するなど、お好みで）
            score += (10f - Mathf.Abs(minDistanceToPlayer - 2f));
        }

        // ③ もしこの移動先から攻撃できる相手がいれば大ボーナス（オプション）
        // if (CanAttackAnyPlayerFrom(pos, enemyUnit)) { score += 100f; }

        Debug.Log($"{candidate.unit.name} が {pos} に移動した場合のスコア: {score}点 (相性:{nearestAdvantage})");
        return score;
    }
}