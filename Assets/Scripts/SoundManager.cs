using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private AudioSource _audioSource;

    [Header("Audio Clips (SE)")]
    [SerializeField] private AudioClip _clickClip;       // 1. 決定・クリック音
    [SerializeField] private AudioClip _moveClip;        // 2. 移動・配置音
    [SerializeField] private AudioClip _cancelClip;      // 3. キャンセル・エラー音
    [SerializeField] private AudioClip _turnChangeClip;  // 4. ターン切替の合図
    [SerializeField] private AudioClip _attackClip;      // 5. 攻撃・カード発動音
    [SerializeField] private AudioClip _putClip;      // 6. 配置音
    [SerializeField] private AudioClip _reserve;
    [SerializeField] private AudioClip _attackBase;


    private void Awake()
    {
        // シングルトンの設定
        if (Instance == null)
        {
            Instance = this;
            // 場面転換しても消したくない場合は下のコメントアウトを外してください
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _audioSource = GetComponent<AudioSource>();
    }

    /// <summary> 1. 決定・クリック音 </summary>
    public void PlayClick()
    {
        PlaySE(_clickClip);
    }
    public void PlayPut()
    {
        PlaySE(_putClip);
    }
    public void PlayReserve()
    {
        PlaySE(_reserve);
    }
    public void PlayAttackBase()
    {
        PlaySE(_attackBase);
    }
    /// <summary> 2. 移動・配置音 </summary>
    public void PlayMove()
    {
        PlaySE(_moveClip);
    }

    /// <summary> 3. キャンセル・エラー音 </summary>
    public void PlayCancel()
    {
        PlaySE(_cancelClip);
    }

    /// <summary> 4. ターン開始・終了の合図 </summary>
    public void PlayTurnChange()
    {
        PlaySE(_turnChangeClip);
    }

    /// <summary> 5. 攻撃・カード発動音 </summary>
    public void PlayAttack()
    {
        PlaySE(_attackClip);
    }

    private void PlaySE(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
        {
            _audioSource.PlayOneShot(clip);
        }
    }
}