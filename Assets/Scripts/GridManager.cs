using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 3x7のグリッド管理クラス。
/// マスのデータ保持、キャラクターの配置管理、移動判定を行う。
/// </summary>
public class GridManager : MonoBehaviour
{
    private const int Rows = 7;
    private const int Cols = 3;

    // グリッド上のユニット配置データ
    private Unit[,] _grid = new Unit[Cols, Rows];

    /// <summary>
    /// 指定した座標にユニットを配置できるか確認する
    /// </summary>
    public bool CanPlaceUnit(int col, int row)
    {
        if (!IsValidCoordinate(col, row)) return false;
        return _grid[col, row] == null;
    }

    /// <summary>
    /// ユニットをグリッドに配置する
    /// </summary>
    public void PlaceUnit(Unit unit, int col, int row)
    {
        if (IsValidCoordinate(col, row))
        {
            _grid[col, row] = unit;
        }
    }

    /// <summary>
    /// ユニットの配置を解除する
    /// </summary>
    public void RemoveUnit(int col, int row)
    {
        if (IsValidCoordinate(col, row))
        {
            _grid[col, row] = null;
        }
    }

    /// <summary>
    /// 指定座標のユニットを取得する
    /// </summary>
    public Unit GetUnitAt(int col, int row)
    {
        return IsValidCoordinate(col, row) ? _grid[col, row] : null;
    }

    /// <summary>
    /// ユニットの移動処理（配置データの更新）
    /// </summary>
    public bool TryMoveUnit(int fromCol, int fromRow, int toCol, int toRow)
    {
        if (!IsValidCoordinate(toCol, toRow) || _grid[toCol, toRow] != null)
            return false;

        Unit unit = _grid[fromCol, fromRow];
        _grid[toCol, toRow] = unit;
        _grid[fromCol, fromRow] = null;
        return true;
    }

    /// <summary>
    /// 座標がグリッド内か判定
    /// </summary>
    public bool IsValidCoordinate(int col, int row)
    {
        return col >= 0 && col < Cols && row >= 0 && row < Rows;
    }

    /// <summary>
    /// 指定されたユニットの移動可能範囲を計算する
    /// </summary>
    /// <param name="unit">対象ユニット</param>
    /// <param name="currentCol">現在の列</param>
    /// <param name="currentRow">現在の行</param>
    /// <returns>移動可能な座標リスト</returns>
    public List<Vector2Int> GetAvailableMoves(Unit unit, int currentCol, int currentRow)
    {
        List<Vector2Int> availableMoves = new List<Vector2Int>();
        
        // ユニットの移動範囲定義（Unitクラスから取得する想定）
        var movePattern = unit.MovePattern; 

        foreach (var offset in movePattern)
        {
            int targetCol = currentCol + offset.x;
            int targetRow = currentRow + offset.y;

            if (IsValidCoordinate(targetCol, targetRow) && _grid[targetCol, targetRow] == null)
            {
                availableMoves.Add(new Vector2Int(targetCol, targetRow));
            }
        }

        return availableMoves;
    }

    /// <summary>
    /// 全グリッドをクリアする（リセット時用）
    /// </summary>
    public void ClearGrid()
    {
        for (int c = 0; c < Cols; c++)
        {
            for (int r = 0; r < Rows; r++)
            {
                _grid[c, r] = null;
            }
        }
    }
}
