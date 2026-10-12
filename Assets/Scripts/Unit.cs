// Responsibility: Represents an in-game unit that handles player clicks during the Planning phase to schedule movement via GridManager.
// Attachment Note: Attach this script to Unit GameObjects that have a Collider for mouse clicking.

using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum ElementType { Fire, Water, Grass }
    public enum MoveCellType { Cross, UpDown, naname, Surrounding, None };
    public enum AttackCellType { Cross, UpDown, naname, Surrounding, None };


    [Header("ユニット設定")]
    [SerializeField] private ElementType element = ElementType.Fire;
    [SerializeField] private int power = 10;
    [SerializeField] private MoveCellType moveType = MoveCellType.UpDown;
    [SerializeField] private AttackCellType attackCellType = AttackCellType.UpDown;



    [HideInInspector] public bool IsPlayerOwned = true;
    public ElementType Element => element;
    public Vector2Int CurrentPosition { get; set; }


    public int CurrentPower
    {
        get => power;
        set => power = Mathf.Max(0, value);
    }


    [HideInInspector]
    public List<Vector2Int> MoveCells = new List<Vector2Int>
    {
        new Vector2Int( 0,  1), // 上
        new Vector2Int( 0, -1), // 下
        new Vector2Int( 1,  0), // 右
        new Vector2Int(-1,  0), // 左
        new Vector2Int( 1,  1), // 右上
        new Vector2Int(-1,  1), // 左上
        new Vector2Int( 1, -1), // 右下
        new Vector2Int(-1, -1)  // 左下
    };

    [HideInInspector]
    public List<Vector2Int> AttackCells = new List<Vector2Int>
    {
        new Vector2Int(0,0),
        new Vector2Int( 0,  1), // 上
        new Vector2Int( 0, -1), // 下
        new Vector2Int( 1,  0), // 右
        new Vector2Int(-1,  0), // 左
        new Vector2Int( 1,  1), // 右上
        new Vector2Int(-1,  1), // 左上
        new Vector2Int( 1, -1), // 右下
        new Vector2Int(-1, -1)  // 左下
    };


    private void Awake()
    {

        SetUnitAttackMoveCells();
    }

    //
    /// <summary>
    /// ユニットの移動可能範囲と攻撃可能範囲を設定する
    /// </summary>
    private void SetUnitAttackMoveCells()
    {
        // リストを初期化（必要に応じて）
        MoveCells.Clear();
        AttackCells.Clear();

        // --- 移動範囲の設定 ---
        switch (moveType)
        {
            case MoveCellType.Cross:
                // 上下左右
                MoveCells.Add(new Vector2Int(0, 1));   // 上
                MoveCells.Add(new Vector2Int(0, -1));  // 下
                MoveCells.Add(new Vector2Int(-1, 0));  // 左
                MoveCells.Add(new Vector2Int(1, 0));   // 右
                break;

            case MoveCellType.UpDown:
                // 上下のみ
                MoveCells.Add(new Vector2Int(0, 1));   // 上
                MoveCells.Add(new Vector2Int(0, -1));  // 下
                break;

            case MoveCellType.naname:
                // 斜め4方向
                MoveCells.Add(new Vector2Int(-1, 1));  // 左上
                MoveCells.Add(new Vector2Int(1, 1));   // 右上
                MoveCells.Add(new Vector2Int(-1, -1)); // 左下
                MoveCells.Add(new Vector2Int(1, -1));  // 右下
                break;
            case MoveCellType.Surrounding:
                MoveCells.Add(new Vector2Int(0, 1));   // 上
                MoveCells.Add(new Vector2Int(0, -1));  // 下
                MoveCells.Add(new Vector2Int(-1, 0));  // 左
                MoveCells.Add(new Vector2Int(1, 0));   // 右
                MoveCells.Add(new Vector2Int(-1, 1));  // 左上
                MoveCells.Add(new Vector2Int(1, 1));   // 右上
                MoveCells.Add(new Vector2Int(-1, -1)); // 左下
                MoveCells.Add(new Vector2Int(1, -1));  // 右下
                break;

            case MoveCellType.None:
            default:
                // 移動なし
                break;
        }

        // --- 攻撃範囲の設定 ---
        // （攻撃側の設定も同様にswitchで分岐）
        switch (attackCellType)
        {
            case AttackCellType.Cross:
                // 上下左右
                AttackCells.Add(new Vector2Int(0, 1));
                AttackCells.Add(new Vector2Int(0, -1));
                AttackCells.Add(new Vector2Int(-1, 0));
                AttackCells.Add(new Vector2Int(1, 0));
                break;

            case AttackCellType.UpDown:
                // 上下のみ
                AttackCells.Add(new Vector2Int(0, 1));
                AttackCells.Add(new Vector2Int(0, -1));
                break;
            case AttackCellType.naname:
                MoveCells.Add(new Vector2Int(-1, 1));  // 左上
                MoveCells.Add(new Vector2Int(1, 1));   // 右上
                MoveCells.Add(new Vector2Int(-1, -1)); // 左下
                MoveCells.Add(new Vector2Int(1, -1));  // 右下
                break;
            case AttackCellType.Surrounding:
                MoveCells.Add(new Vector2Int(0, 1));   // 上
                MoveCells.Add(new Vector2Int(0, -1));  // 下
                MoveCells.Add(new Vector2Int(-1, 0));  // 左
                MoveCells.Add(new Vector2Int(1, 0));   // 右
                MoveCells.Add(new Vector2Int(-1, 1));  // 左上
                MoveCells.Add(new Vector2Int(1, 1));   // 右上
                MoveCells.Add(new Vector2Int(-1, -1)); // 左下
                MoveCells.Add(new Vector2Int(1, -1));  // 右下
                break;

            case AttackCellType.None:
            default:
                // 攻撃範囲なし
                break;
        }
    }



    public List<Vector2Int> GetAttackablePositions(Vector2Int currentPos)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        foreach (var offset in AttackCells)
        {
            Vector2Int targetPos = currentPos + offset;
            result.Add(targetPos);
        }

        return result;
    }
    public List<Vector2Int> GetMovalePositions(Vector2Int currentPos)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        foreach (var offset in MoveCells)
        {
            Vector2Int targetPos = currentPos + offset;
            if (targetPos.y >= GridManager.Rows || targetPos.x >= GridManager.Cols || targetPos.y < 0 || targetPos.x < 0) continue;
            result.Add(targetPos);
        }
        return result;
    }

    /// <summary>
    /// 指定した位置（myPos）にいるとき、相手の位置（targetPos）を攻撃できるか？
    /// </summary>
    public bool CanAttackTarget(Vector2Int myPos, Vector2Int targetPos)
    {
        List<Vector2Int> attackableCells = GetAttackablePositions(myPos);
        return attackableCells.Contains(targetPos);
    }
    private void OnMouseDown()
    {
        if (!IsPlayerOwned || GameManager.CurrentPhase != GameManager.GamePhase.MoveReservation) return;
        //もう一度クリックしたら削除
        if (GridManager.Instance.SelectedUnit == this)
        {
            GridManager.Instance.DeselectUnit();
            return;
        }
        else
        {

            GridManager.Instance.SelectUnit(this);
        }

    }
}