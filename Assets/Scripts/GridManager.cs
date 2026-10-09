using System.Collections.Generic;
using UnityEngine;

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
    public static GridManager Instance;
    private const int Rows = 7;
    private const int Cols = 3;

    public Quaternion cardRotation = Quaternion.Euler(0, 90f, 0);
    private List<Unit> _allActiveUnits = new List<Unit>();

    [Header("Grid Settings")]
    [SerializeField, Tooltip("生成するマスのPrefab")]
    private GameObject cellPrefab;

    private Unit[,] _grid = new Unit[Cols, Rows];
    private bool[,] _obstacles = new bool[Cols, Rows];

    public Unit SelectedUnit { get; private set; }
    private Vector2Int _selectedUnitPos;

    //全てのユニットを格納する
    private Dictionary<Unit, Vector2Int> _reservedMoves = new Dictionary<Unit, Vector2Int>();

    private void Awake()
    {
        Instance = this;
    }
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

            SpawnUnit(cardPrefabs[i], availablePositions[i], cardRotation, false);
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



    public void SpawnUnit(GameObject unitPrefab, Vector2Int pos, Quaternion rotation, bool isPlayerOwned)
    {
        if (!IsValidCoordinate(pos.x, pos.y)) return;
        if (IsCellOccupied(pos)) return;

        if (unitPrefab != null)
        {
            GameObject obj = Instantiate(unitPrefab, new Vector3(pos.x, 0, pos.y), rotation);
            Unit unit = obj.GetComponent<Unit>();

            if (unit != null)
            {
                // ユニット側の所属フラグを設定
                unit.IsPlayerOwned = isPlayerOwned; // ※小文字/大文字はUnit側の変数名に合わせて調整してください
            }

            _grid[pos.x, pos.y] = unit;

            // 全ユニット管理リストに追加
            if (!_allActiveUnits.Contains(unit))
            {
                _allActiveUnits.Add(unit);
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



    public void SelectUnit(Unit unit)
    {
        SelectedUnit = unit;
        Debug.Log($"{unit}選択した");
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
        Debug.Log($"{targetPos}移動予約した");
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
        _allActiveUnits.RemoveAll(unit => unit == null);
        return new List<Unit>(_allActiveUnits);
    }
    public List<Unit> GetUnitsByTeam(bool isPlayerSide)
    {
        List<Unit> list = new List<Unit>();
        foreach (var unit in GetAllUnits())
        {
            if (unit != null && unit.IsPlayerOwned == isPlayerSide)
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
        Debug.Log($"すべてのユニット移動{_reservedMoves}");
        foreach (var kvp in _reservedMoves)
        {
            Unit unit = kvp.Key;
            Vector2Int targetPos = kvp.Value;
            Vector2Int currentPos = GetUnitPosition(unit);

            if (IsValidCoordinate(currentPos.x, currentPos.y) && IsValidCoordinate(targetPos.x, targetPos.y))
            {
                // 1. グリッドのデータを更新
                _grid[currentPos.x, currentPos.y] = null;
                _grid[targetPos.x, targetPos.y] = unit;

                // 2. 実際の3Dオブジェクトの位置を移動させる
                // ※ GridManager側にある「グリッド座標をワールド座標に変換する関数」を使ってください
                Vector3 worldPos = GridToWorldPosition(targetPos);
                unit.transform.position = worldPos;

                // 3. ユニット側が持っている現在地データがあればそれも更新
                unit.CurrentPosition = targetPos;
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
        Debug.Log($"すべてのユニット移動完了");
    }
    /// <summary>
    /// グリッド座標 (Vector2Int) をワールド座標 (Vector3) に変換する
    /// </summary>
    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x, 0f, gridPos.y);
    }

    /// <summary>
    /// ワールド座標 (Vector3) を一番近いグリッド座標 (Vector2Int) に変換する
    /// </summary>
    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x);
        int y = Mathf.RoundToInt(worldPos.z);
        return new Vector2Int(x, y);
    }// ==========================================
    // 追加・補強が必要なメソッド群
    // ==========================================

    /// <summary>
    /// 指定されたユニットの移動予約先を取得する
    /// </summary>
    public Vector2Int? GetReservedMove(Unit unit)
    {
        if (_reservedMoves.ContainsKey(unit))
        {
            return _reservedMoves[unit];
        }
        return null;
    }

    /// <summary>
    /// 現在予約されているすべての移動リストを外部（GameManagerやBattleProcessorなど）から参照できるようにする
    /// </summary>
    public Dictionary<Unit, Vector2Int> GetAllReservedMoves()
    {
        return _reservedMoves;
    }

    /// <summary>
    /// 移動予約をクリアする
    /// </summary>
    public void ClearReservedMoves()
    {
        _reservedMoves.Clear();
    }



    public Unit GetUnitAtPosition(Vector2Int pos)
    {
        // 場にいる全ユニットから、指定座標と一致するものを探す
        List<Unit> allUnits = GetAllUnits();
        foreach (var unit in allUnits)
        {
            if (unit != null && unit.CurrentPosition == pos)
            {
                Debug.Log("見つかりました");
                return unit; // 見つかったらそのユニットを返す
            }
        }
        return null; // 誰もいなければnull
    }
}


