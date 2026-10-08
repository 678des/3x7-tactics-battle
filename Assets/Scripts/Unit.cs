// Responsibility: Represents an in-game unit that handles player clicks during the ReservePhase to schedule movement via GridManager.
// Attachment Note: Attach this script to Unit GameObjects that have a Collider for mouse clicking.

using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(UnityEngine.Transform))]
public class Unit : MonoBehaviour
{
    [SerializeField] private bool isPlayerOwned = true;
    [SerializeField] private List<Vector2Int> movePattern = new List<Vector2Int> { new Vector2Int(0, 1) };
    [SerializeField] private ElementType element = ElementType.Fire;
    [SerializeField] private int power = 10;

    public bool IsPlayerOwned => isPlayerOwned;
    public bool isPlayerSide => isPlayerOwned;
    public List<Vector2Int> MovePattern => movePattern;
    public ElementType Element => element;
    public int Power => power;

    public void ApplyBuff(int amount)
    {
        Debug.Log($"{gameObject.name} にバフ適用: +{amount}");
    }

    private void OnMouseDown()
    {
        if (!isPlayerOwned)
        { 
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.CurrentPhase == GamePhase.Planning)
        {
            if (GridManager.Instance != null)
            {
                GridManager.Instance.ReserveMove(this);
            }
        }
    }
}