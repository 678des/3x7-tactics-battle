// Responsibility: Represents an in-game unit that handles player clicks during the Planning phase to schedule movement via GridManager.
// Attachment Note: Attach this script to Unit GameObjects that have a Collider for mouse clicking.

using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum ElementType { Fire, Water, Grass }


    [Header("Unit Status")]
    [SerializeField] private ElementType element = ElementType.Fire;
    [SerializeField] private int power = 10;

    public bool IsPlayerOwned = true;
    public ElementType Element => element;
    public Vector2Int CurrentPosition { get; set; }

    public int CurrentPower
    {
        get => power;
        set => power = Mathf.Max(0, value);
    }

    public Vector2Int? PlannedDestination { get; set; } = null;

    [Header("Ranges (Relative Offsets)")]
    [Tooltip("移動できるマスの相対座標リスト")]
    public List<Vector2Int> MovePattern = new List<Vector2Int>
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

    [Tooltip("攻撃できるマスの相対座標リスト")]
    public List<Vector2Int> AttackPattern = new List<Vector2Int>
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

    public void ClearDestination()
    {
        PlannedDestination = null;
    }

    public List<Vector2Int> GetAttackablePositions(Vector2Int currentPos)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        foreach (var offset in AttackPattern)
        {
            Vector2Int targetPos = currentPos + offset;
            result.Add(targetPos);
        }

        return result;
    }
    public List<Vector2Int> GetMovalePositions(Vector2Int currentPos)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        foreach (var offset in MovePattern)
        {
            Vector2Int targetPos = currentPos + offset;
            if (targetPos.y >= GridManager.Rows || targetPos.x >= GridManager.Cols) continue;
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
        if (!IsPlayerOwned)
        {
            return;
        }

        Debug.Log($"{gameObject.name} がクリックされました。");

        if (GridManager.Instance != null)
        {
            GridManager.Instance.SelectUnit(this);
        }
    }
}