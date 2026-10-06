using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ゲーム内のキャラクター（ユニット）を表すコンポーネント。
/// </summary>
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
}