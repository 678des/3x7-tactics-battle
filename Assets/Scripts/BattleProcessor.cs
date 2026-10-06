// Responsibility: Processes combat between units using element advantages and power checks, and handles base attacks.
// Attachment Note: Attach this script to a BattleManager or GameManager GameObject in the scene.
using UnityEngine;
using System.Collections.Generic;

public enum ElementType { Fire, Water, Grass }

public class BattleProcessor : MonoBehaviour
{
    /// <summary>
    /// 3すくみ判定の結果
    /// </summary>
    public enum BattleResult { Win, Lose, Draw }

    /// <summary>
    /// 属性相性による勝敗判定
    /// </summary>
    /// <param name="attacker">攻撃側の属性</param>
    /// <param name="defender">防御側の属性</param>
    /// <returns>攻撃側から見た結果</returns>
    public BattleResult GetElementResult(ElementType attacker, ElementType defender)
    {
        if (attacker == defender) return BattleResult.Draw;

        return attacker switch
        {
            ElementType.Fire => (defender == ElementType.Grass) ? BattleResult.Win : BattleResult.Lose,
            ElementType.Water => (defender == ElementType.Fire) ? BattleResult.Win : BattleResult.Lose,
            ElementType.Grass => (defender == ElementType.Water) ? BattleResult.Win : BattleResult.Lose,
            _ => BattleResult.Draw
        };
    }

    /// <summary>
    /// バトル処理を実行し、勝敗を判定して対象を撃破する
    /// </summary>
    /// <param name="attacker">攻撃ユニット</param>
    /// <param name="defender">防御ユニット</param>
    /// <returns>撃破されたユニットのリスト</returns>
    public List<Unit> ResolveBattle(Unit attacker, Unit defender)
    {
        List<Unit> defeatedUnits = new List<Unit>();

        BattleResult elementResult = GetElementResult(attacker.Element, defender.Element);

        // 属性判定が引き分けの場合、パワーで比較
        if (elementResult == BattleResult.Draw)
        {
            if (attacker.Power > defender.Power)
            {
                defeatedUnits.Add(defender);
            }
            else if (attacker.Power < defender.Power)
            {
                defeatedUnits.Add(attacker);
            }
            else
            {
                // パワーも同じなら相打ち
                defeatedUnits.Add(attacker);
                defeatedUnits.Add(defender);
            }
        }
        else if (elementResult == BattleResult.Win)
        {
            defeatedUnits.Add(defender);
        }
        else
        {
            defeatedUnits.Add(attacker);
        }

        return defeatedUnits;
    }

    /// <summary>
    /// 拠点攻撃処理
    /// </summary>
    /// <param name="attacker">攻撃ユニット</param>
    /// <param name="baseObject">拠点オブジェクト（HPを持つ想定）</param>
    public void ResolveBaseAttack(Unit attacker, BaseController baseObject)
    {
        if (baseObject != null)
        {
            baseObject.TakeDamage(attacker.Power);
        }
    }
}