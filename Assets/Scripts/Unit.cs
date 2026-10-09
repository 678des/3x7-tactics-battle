// Responsibility: Represents an in-game unit that handles player clicks during the Planning phase to schedule movement via GridManager.
// Attachment Note: Attach this script to Unit GameObjects that have a Collider for mouse clicking.

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnityEngine.Transform))]
public class Unit : MonoBehaviour
{
    [SerializeField] private bool isPlayerOwned = true;
    [SerializeField] private List<Vector2Int> movePattern = new List<Vector2Int> { new Vector2Int(0, 1) };
    [SerializeField] private ElementType element = ElementType.Fire;
    [SerializeField] private int power = 10;

    // 💡 追加: 移動予定のマス（未定のときは null）
    public Vector2Int? PlannedDestination { get; set; } = null;

    public bool IsPlayerOwned => isPlayerOwned;
    public bool isPlayerSide => isPlayerOwned;
    public List<Vector2Int> MovePattern => movePattern;
    public ElementType Element => element;
    public int Power => power;

    public void ApplyBuff(int amount)
    {
        Debug.Log($"{gameObject.name} にバフ適用: +{amount}");
    }

    // 💡 追加: 移動予約をクリアする用
    public void ClearDestination()
    {
        PlannedDestination = null;
    }

    private void OnMouseDown()
    {
        if (!isPlayerOwned)
        {
            return;
        }
        Debug.Log("ユニットクリック");
        GridManager.Instance.SelectUnit(this);

    }
}