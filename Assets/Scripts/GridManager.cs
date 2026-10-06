using UnityEngine;
using System.Collections.Generic;

public enum TeamType
{
    Player,
    Enemy
}

/// <summary>
/// 3x7のグリッド管理クラス。
/// マスのデータ保持、キャラクターの配置管理、移動判定、およびプレハブによる初期マス生成を行う。
/// このスクリプトは、グリッドの原点となる空のGameObjectにアタッチしてください。
/// </summary>
[RequireComponent(typeof(UnityEngine.Transform))]
public class GridManager : MonoBehaviour
{
    private const int Rows = 7;
    private const int Cols = 3;

    public Quaternion cardRotation = Quaternion.Euler(0, 90f, 0);

    [Header("Grid Settings")]
    [SerializeField, Tooltip("生成するマスのPrefab")]
    private GameObject cellPrefab;

    private Unit[,] _grid = new Unit[Cols, Rows];
    private bool[,] _obstacles = new bool[Cols, Rows];

    public Unit SelectedUnit { get; private set; }
    private Vector2Int _selectedUnitPos;
    private Dictionary<Unit, Vector2Int> _reservedMoves = new Dictionary<Unit, Vector2Int>();

    private void Start()
    {
        GenerateGridCubes();
    }

    /// <summary>
    /// 3x7のマスをプレハブから生成する初期関数
    /// </summary>
    private void GenerateGridCubes()
    {
        if (cellPrefab == null)
        {
            Debug.LogWarning("Cell Prefab is not assigned in GridManager.");
            return;
        }

        for (int c = 0; c < Cols; c++)
        {
            for (int r = 0; r < Rows; r++)
            {
                GameObject cellObj = Instantiate(cellPrefab, new Vector3(c, 0, r), Quaternion.identity);
                cellObj.name = "Cell_" + c + "_" + r;
                cellObj.transform.SetParent(this.transform);

                GridCell gridCell = cellObj.GetComponent<GridCell>();
                if (gridCell != null)
                {
                    gridCell.GridPosition = new Vector2Int(c, r);
                }
            }
        }
    }

    /// <summary>
    /// 指定されたチームの陣地に、手持ちのカードプレハブ群を均等に配置する
    /// </summary>
    public void SpawnCardsForTeam(List<GameObject> cardPrefabs, TeamType team)
    {
        if (cardPrefabs == null || cardPrefabs.Count == 0) return;

        List<Vector2Int> availablePositions = new List<Vector2Int>();

        if (team == TeamType.Player)
        {
            // プレイヤーの陣地は行0付近を想定（必要に応じて調整）
            for (int r = -1; r < 2 && r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (CanPlaceUnit(c, r))
                    {
                        availablePositions.Add(new Vector2Int(c, r));
                    }
                }
            }
        }
        else
        {
            // 敵の陣地は行Rows-1付近を想定
            for (int r = Rows; r >= Rows - 2 && r >= 0; r--)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (CanPlaceUnit(c, r))
                    {
                        availablePositions.Add(new Vector2Int(c, r));
                    }
                }
            }
        }

        // 均等に配置するためにリストをシャッフルまたはそのまま利用して順に配置
        for (int i = 0; i < cardPrefabs.Count && i < availablePositions.Count; i++)
        {
           
            SpawnUnit(cardPrefabs[i], availablePositions[i], cardRotation);
        }
    }

    public bool CanPlaceUnit(int col, int row)
    {
        if (!IsValidCoordinate(col, row)) return false;
        return _grid[col, row] == null && !_obstacles[col, row];
    }

    public bool IsCellOccupied(Vector2Int pos)
    {
        if (!IsValidCoordinate(pos.x, pos.y)) return true;
        return _grid[pos.x, pos.y] != null || _obstacles[pos.x, pos.y];
    }

    public void SpawnUnit(GameObject unitPrefab, Vector2Int pos)
    {
        SpawnUnit(unitPrefab, pos, Quaternion.identity);
    }

    public void SpawnUnit(GameObject unitPrefab, Vector2Int pos, Quaternion rotation)
    {
        if (!IsValidCoordinate(pos.x, pos.y)) return;
        if (IsCellOccupied(pos)) return;

        if (unitPrefab != null)
        {
            GameObject obj = Instantiate(unitPrefab, new Vector3(pos.x, 0, pos.y), rotation);
            Unit unit = obj.GetComponent<Unit>();
            if (unit != null)
            {
                _grid[pos.x, pos.y] = unit;
            }
        }
    }

    public void PlaceUnit(Unit unit, int col, int row)
    {
        if (IsValidCoordinate(col, row))
        {
            _grid[col, row] = unit;
        }
    }

    public void RemoveUnit(int col, int row)
    {
        if (IsValidCoordinate(col, row))
        {
            _grid[col, row] = null;
        }
    }

    public Unit GetUnitAt(int col, int row)
    {
        return IsValidCoordinate(col, row) ? _grid[col, row] : null;
    }

    public Unit GetUnitAt(Vector2Int pos)
    {
        return GetUnitAt(pos.x, pos.y);
    }

    public bool TryMoveUnit(int fromCol, int fromRow, int toCol, int toRow)
    {
        if (!IsValidCoordinate(toCol, toRow) || _grid[toCol, toRow] != null || _obstacles[toCol, toRow])
            return false;

        Unit unit = _grid[fromCol, fromRow];
        _grid[toCol, toRow] = unit;
        _grid[fromCol, fromRow] = null;
        return true;
    }

    public bool IsValidCoordinate(int col, int row)
    {
        return col >= 0 && col < Cols && row >= 0 && row < Rows;
    }

    public List<Vector2Int> GetAvailableMoves(Unit unit, int currentCol, int currentRow)
    {
        List<Vector2Int> availableMoves = new List<Vector2Int>();
        var movePattern = unit.MovePattern;

        foreach (var offset in movePattern)
        {
            int targetCol = currentCol + offset.x;
            int targetRow = currentRow + offset.y;

            if (IsValidCoordinate(targetCol, targetRow) && _grid[targetCol, targetRow] == null && !_obstacles[targetCol, targetRow])
            {
                availableMoves.Add(new Vector2Int(targetCol, targetRow));
            }
        }

        return availableMoves;
    }

    public List<Vector2Int> GetReachableTiles(Unit unit, Vector2Int currentPos)
    {
        return GetAvailableMoves(unit, currentPos.x, currentPos.y);
    }

    public bool IsValidMove(Vector2Int targetPos)
    {
        if (SelectedUnit == null) return false;
        var available = GetAvailableMoves(SelectedUnit, _selectedUnitPos.x, _selectedUnitPos.y);
        return available.Contains(targetPos);
    }

    public void SelectUnit(Unit unit)
    {
        SelectedUnit = unit;
        for (int c = 0; c < Cols; c++)
        {
            for (int r = 0; r < Rows; r++)
            {
                if (_grid[c, r] == unit)
                {
                    _selectedUnitPos = new Vector2Int(c, r);
                    return;
                }
            }
        }
    }

    public void DeselectUnit()
    {
        SelectedUnit = null;
    }

    public void ReserveMove(Unit unit, Vector2Int targetPos)
    {
        if (_reservedMoves.ContainsKey(unit))
        {
            _reservedMoves[unit] = targetPos;
        }
        else
        {
            _reservedMoves.Add(unit, targetPos);
        }
    }

    public List<Unit> GetAllUnits()
    {
        List<Unit> list = new List<Unit>();
        for (int c = 0; c < Cols; c++)
        {
            for (int r = 0; r < Rows; r++)
            {
                if (_grid[c, r] != null)
                {
                    list.Add(_grid[c, r]);
                }
            }
        }
        return list;
    }

    public List<Unit> GetAllUnitsByTeam(TeamType team)
    {
        List<Unit> list = new List<Unit>();
        bool targetIsPlayer = (team == TeamType.Player);
        foreach (var unit in GetAllUnits())
        {
            if (unit.IsPlayerOwned == targetIsPlayer)
            {
                list.Add(unit);
            }
        }
        return list;
    }

    public Vector2Int GetUnitPosition(Unit unit)
    {
        for (int c = 0; c < Cols; c++)
        {
            for (int r = 0; r < Rows; r++)
            {
                if (_grid[c, r] == unit)
                {
                    return new Vector2Int(c, r);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    public List<Vector2Int> GetAllUnitPositionsByTeam(TeamType team)
    {
        List<Vector2Int> list = new List<Vector2Int>();
        bool targetIsPlayer = (team == TeamType.Player);
        for (int c = 0; c < Cols; c++)
        {
            for (int r = 0; r < Rows; r++)
            {
                if (_grid[c, r] != null && _grid[c, r].IsPlayerOwned == targetIsPlayer)
                {
                    list.Add(new Vector2Int(c, r));
                }
            }
        }
        return list;
    }

    public void ProcessAllUnitMovements()
    {
        foreach (var kvp in _reservedMoves)
        {
            Unit unit = kvp.Key;
            Vector2Int targetPos = kvp.Value;
            Vector2Int currentPos = GetUnitPosition(unit);

            if (IsValidCoordinate(currentPos.x, currentPos.y) && IsValidCoordinate(targetPos.x, targetPos.y))
            {
                _grid[currentPos.x, currentPos.y] = null;
                _grid[targetPos.x, targetPos.y] = unit;
            }
        }
        _reservedMoves.Clear();
    }

    public void SetCellObstacle(Vector2Int pos, bool isObstacle)
    {
        if (IsValidCoordinate(pos.x, pos.y))
        {
            _obstacles[pos.x, pos.y] = isObstacle;
        }
    }

    public void ClearGrid()
    {
        for (int c = 0; c < Cols; c++)
        {
            for (int r = 0; r < Rows; r++)
            {
                _grid[c, r] = null;
                _obstacles[c, r] = false;
            }
        }
        _reservedMoves.Clear();
    }
}
