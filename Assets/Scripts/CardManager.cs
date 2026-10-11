using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 手札の管理、コスト消費、カード効果の適用、および初期配置を行うマネージャークラス。
/// </summary>
public class CardManager : MonoBehaviour
{
    public static CardManager Instance;
    private GridManager gridManager;

    [Header("デッキの設定")]
    [SerializeField] private List<GameObject> playerHandCards;
    [SerializeField] private List<GameObject> EnemyHandCards;

    [SerializeField] private Transform enemyCardBox;
    [SerializeField] private Transform playerCardBox;


    [Header("Hand / UI Settings")]
    public Vector3 handOffset = new Vector3(0, 0.3f, -0.4f);
    private Quaternion handRotation = Quaternion.Euler(0, 90, 30);

    //現在味方が選択しているカード
    public static Card currentPlayerSelectedCard;
    //現在敵が選択しているカード
    public static Card currentEnemySelectedCard;


    private void Awake()
    {
        Instance = this;
        gridManager = FindFirstObjectByType<GridManager>();

    }

    /// <summary>
    /// ゲームスタート時にプレイヤーと敵のカードをそれぞれ生成・配置する
    /// </summary>
    public void DistributeStartingCards()
    {
        SpawnCardsForTeam(playerHandCards, TeamType.Player, playerCardBox);
        SpawnCardsForTeam(EnemyHandCards, TeamType.Enemy, enemyCardBox);
    }

    /// <summary>
    /// チームごとにカードプレハブを生成する（味方・敵を明確に分離）
    /// </summary>
    private void SpawnCardsForTeam(List<GameObject> cardPrefabs, TeamType team, Transform parent)
    {

        for (int i = 0; i < cardPrefabs.Count; i++)
        {
            if (cardPrefabs[i] == null) continue;
            Vector3 spawnPos = handOffset + new Vector3(i * 0.7f, 0, team == TeamType.Player ? 0 : 7f); // 味方と敵で少し位置を分ける例
            Instantiate(cardPrefabs[i], spawnPos, handRotation, parent);
        }
    }

    /// <summary>
    /// カードを使用する（コストチェック、効果発動、コスト消費、カードの破棄）
    /// </summary>
    public bool UseCard(Vector2Int targetPosition, Card card, bool isPlayerOwnedCard)
    {
        if (card == null) return false;

        // コスト不足のチェック
        if (GameManager.Instance != null && GameManager.Instance.CurrentPlayerCost < card.CardData.cost)
        {
            Debug.LogWarning("コスト不足です。");
            return false;
        }

        // カードの種類に応じた効果処理
        switch (card.CardData.type)
        {
            case CardType.Summon:

                gridManager.SpawnUnit(card.CardData.unitPrefab, targetPosition, Quaternion.Euler(-90, 0, 0), isPlayerOwnedCard);
                break;

            case CardType.Buff:
                // TODO: バフ効果の適用
                break;

            case CardType.Debuff:
                // TODO: デバフ効果の適用
                break;
        }
        SoundManager.Instance.PlayPut();

        // コストの消費
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TryConsumeCost(isPlayerOwnedCard, card.CardData.cost);
        }

        // カードの後処理（選択解除とオブジェクトの破棄）
        card.SetSelected(false);
        card.gameObject.SetActive(false);
        Destroy(card.gameObject);
        if (isPlayerOwnedCard) currentPlayerSelectedCard = null;
        else currentEnemySelectedCard = null;

        return true;
    }


    public List<GameObject> GetEnemyHandCards()
    {
        List<GameObject> cardlist = new List<GameObject>();

        foreach (Transform child in enemyCardBox)
        {
            // 子オブジェクトがnullでなく、かつCardコンポーネントを持っている場合のみ追加する
            if (child != null)
            {
                Card cardComponent = child.GetComponent<Card>();
                if (cardComponent != null)
                {
                    cardlist.Add(child.gameObject);
                }
            }
        }

        return cardlist;
    }


    public void AllCardViewController(Card _card)
    {
        if (_card == null) return;

        // 1.すでに選択されているカードをもう一度押したら沈ませる
        if (_card == currentPlayerSelectedCard)
        {
            _card.SetSelected(false);
            currentPlayerSelectedCard = null;
            return;
        }

        // 2. 「新しい別のカードを押した」場合
        // まず、プレイヤーのカードボックスにあるすべてのカードを一度沈める
        foreach (Transform obj in playerCardBox)
        {
            Card card = obj.GetComponent<Card>();
            if (card == null) continue;
            card.SetSelected(false);
        }
        _card.SetSelected(true);
        currentPlayerSelectedCard = _card;
        SoundManager.Instance.PlayClick();
    }
}