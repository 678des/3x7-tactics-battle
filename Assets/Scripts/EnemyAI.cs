using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 敵AIの行動ロジックを制御するクラス。
/// 最短距離移動アルゴリズムを用いて、味方ユニットまたは拠点への接近を決定する。
/// </summary>
public class EnemyAI : MonoBehaviour
{
    private GridManager _gridManager;

    private void Awake()
    {
        _gridManager = GetComponent<GridManager>();
    }

    /// <summary>
    /// 敵AIの移動フェーズにおける思考ルーチンを実行する。
    /// </summary>
    public void ExecuteEnemyTurn()
    {
        List<Unit> enemyUnits = _gridManager.GetAllUnitsByTeam(TeamType.Enemy);

        foreach (var unit in enemyUnits)
        {
            Vector2Int currentPos = _gridManager.GetUnitPosition(unit);
            Vector2Int targetPos = CalculateBestMove(unit, currentPos);

            if (targetPos != currentPos)
            {
                _gridManager.ReserveMove(unit, targetPos);
            }
        }
    }

    /// <summary>
    /// 最短距離移動アルゴリズムに基づき、移動先を決定する。
    /// </summary>
    private Vector2Int CalculateBestMove(Unit unit, Vector2Int currentPos)
    {
        List<Vector2Int> reachableTiles = _gridManager.GetReachableTiles(unit, currentPos);
        if (reachableTiles.Count == 0) return currentPos;

        Vector2Int bestTile = currentPos;
        float minDistance = float.MaxValue;

        // ターゲット候補：味方ユニットの全座標
        List<Vector2Int> playerPositions = _gridManager.GetAllUnitPositionsByTeam(TeamType.Player);

        // ターゲットがいない場合は拠点（行0）を目指す
        if (playerPositions.Count == 0)
        {
            playerPositions.Add(new Vector2Int(1, 0)); // 拠点中央を仮定
        }

        foreach (var tile in reachableTiles)
        {
            foreach (var target in playerPositions)
            {
                float dist = ManhattanDistance(tile, target);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    bestTile = tile;
                }
            }
        }

        return bestTile;
    }

    /// <summary>
    /// マンハッタン距離を計算する。
    /// </summary>
    private float ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}