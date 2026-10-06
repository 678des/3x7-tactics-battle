// Responsibility: Manages base health, damage taking, and base status for battle processing.
// Attachment Note: Attach this script to a Base GameObject in the scene that can be attacked by units.
using UnityEngine;

public class BaseController : MonoBehaviour
{
    [SerializeField] private int maxHp = 100;
    private int currentHp;

    private void Awake()
    {
        currentHp = maxHp;
    }

    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        if (currentHp < 0)
        {
            currentHp = 0;
        }

        if (currentHp <= 0)
        {
            DestroyBase();
        }
    }

    private void DestroyBase()
    {
        Destroy(gameObject);
    }
}