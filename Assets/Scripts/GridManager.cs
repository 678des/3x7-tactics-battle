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
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    public const int Rows = 7;
    public const int Cols = 3;

    public Quaternion cardRotation = Quaternion.Euler(0, 90f, 0);
    private List<Unit> _allActiveUnits = new List<Unit>();

    [Header("Grid Settings")]
    [SerializeField, Tooltip("生成するマスのPrefab")]
    private GameObject cellPrefab;

    [Header("Selection Ring Settings")]
    [SerializeField] private Transform _sharedSelectionRingTransform; // シーン上にある唯一のリングのTransform
    [SerializeField] private Vector3 _hiddenPosition = new Vector3(0, -999f, 0); // 非選択時の退避場所

    private Unit[,] _grid = new Unit[Cols, Rows];
    private bool[,] _obstacles = new bool[Cols, Rows];

    public Unit SelectedUnit { get; private set; }

    [Header("Indicator Settings")]
    [SerializeField] private GameObject _movableIndicatorPrefab; // インジケーターのプレハブ
    [SerializeField] private GameObject _movableIndicatorDecidededPrefab; // インジケーターの移動が予約できた時のプレハブ
    [SerializeField] private int _poolSize = 10;                // 同時に表示する最大数（移動力に合わせる）

    private List<Transform> _indicatorPool = new List<Transform>();

    public (Vector2Int reservePos, Unit unit) playerUnitMoveOder;
    public (Vector2Int reservePos, Unit unit) enemyUnitMoveOder;


    private void Awake()
    {
        Instance = this;

    }
    private void Start()
    {
        GenerateGridCubes();
        HideSelectionRing();
        InitializeIndicatorPool();
    }

    /// <summary>
    /// あらかじめ指定数のインジケーターを生成して、隠し場所へ飛ばしておく
    /// </summary>
    private void InitializeIndicatorPool()
    {
        if (_movableIndicatorPrefab == null) return;

        for (int i = 0; i < _poolSize; i++)
        {
            GameObject obj = Instantiate(_movableIndicatorPrefab, _hiddenPosition, Quaternion.identity);
            obj.name = $"MovableIndicator_{i}";
            // GridManagerの子にしておく
            obj.transform.SetParent(this.transform);
            _indicatorPool.Add(obj.transform);
        }
    }

    public void ShowMovableRangeIndicator(List<Vector2Int> movableCoords)
    {
        HideAllMovableIndicators();

        if (movableCoords == null || movableCoords.Count == 0) return;

        // 2. リストの数（またはプール数）だけ、インジケーターを移動させる
        for (int i = 0; i < movableCoords.Count && i < _indicatorPool.Count; i++)
        {
            Transform indicator = _indicatorPool[i];
            Vector2Int coord = movableCoords[i];
            Vector3 worldPos = GridToWorldPosition(coord);
            worldPos.y += 0.1f;

            indicator.position = worldPos;
        }
    }

    public void HideAllMovableIndicators()
    {
        foreach (Transform indicator in _indicatorPool)
        {
            if (indicator != null)
            {
                indicator.position = _hiddenPosition;
            }
        }
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

        GameObject obj = Instantiate(unitPrefab, new Vector3(pos.x, 0, pos.y), rotation);
        Unit unit = obj.GetComponent<Unit>();
        if (!unit) return;
        unit.IsPlayerOwned = isPlayerOwned;
        unit.CurrentPosition = pos;
        _grid[pos.x, pos.y] = unit;
        if (!_allActiveUnits.Contains(unit)) _allActiveUnits.Add(unit);


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
        List<Vector2Int> range = unit.GetMovalePositions(unit.CurrentPosition);
        ShowMovableRangeIndicator(range);
        MoveRingToPosition(GetUnitPosition(unit));

    }

    public void DeselectUnit()
    {
        SelectedUnit = null;
        playerUnitMoveOder = (default, null);
        _movableIndicatorDecidededPrefab.transform.position = _hiddenPosition;
        HideSelectionRing();
        HideAllMovableIndicators();
    }
    private void MoveRingToPosition(Vector2Int gridPos)
    {
        if (_sharedSelectionRingTransform != null)
        {
            Vector3 worldPos = GridToWorldPosition(gridPos);
            worldPos.y += 0.2f;
            _sharedSelectionRingTransform.position = worldPos;
        }
    }
    private void HideSelectionRing()
    {
        if (_sharedSelectionRingTransform != null)
        {
            _sharedSelectionRingTransform.position = _hiddenPosition;
        }
    }

    public void ReserveMove(Unit unit, Vector2Int targetPos, bool isPlayerOwned)
    {
        if (GameManager.CurrentPhase != GameManager.GamePhase.MoveReservation || unit == null || SelectedUnit == null) return;
        if (isPlayerOwned)
        {
            playerUnitMoveOder = (targetPos, unit);
            _movableIndicatorDecidededPrefab.transform.position = new Vector3(targetPos.x, 0.11f, targetPos.y);
        }
        else enemyUnitMoveOder = (targetPos, unit);

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
    /// <summary>
    /// プレイヤーと敵の移動予約を実際に実行し、位置を更新する
    /// </summary>
    public void ProcessAllUnitMovements()
    {
        Debug.Log("すべてのユニットの移動処理を実行します。");
        ExecuteMovementOrder(playerUnitMoveOder);
        ExecuteMovementOrder(enemyUnitMoveOder);
        playerUnitMoveOder = (default, null);
        enemyUnitMoveOder = (default, null);
        DeselectUnit();
    }

    /// <summary>
    /// 個別の移動予約を処理するヘルパーメソッド
    /// </summary>
    private void ExecuteMovementOrder((Vector2Int reservePos, Unit unit) order)
    {
        if (order.unit == null) return;

        Vector2Int targetPos = order.reservePos;
        Vector2Int currentPos = GetUnitPosition(order.unit);

        // 現在地と移動先の座標が両方とも有効かチェック
        if (IsValidCoordinate(currentPos.x, currentPos.y) && IsValidCoordinate(targetPos.x, targetPos.y))
        {
            _grid[currentPos.x, currentPos.y] = null;
            _grid[targetPos.x, targetPos.y] = order.unit;
            Vector3 worldPos = GridToWorldPosition(targetPos);
            order.unit.transform.position = worldPos;
            order.unit.CurrentPosition = targetPos;
            Debug.Log($"{order.unit.name} を {currentPos} から {targetPos} へ移動しました。");
        }
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


