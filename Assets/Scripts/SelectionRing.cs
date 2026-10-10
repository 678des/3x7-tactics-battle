using UnityEngine;

public class SelectionRing : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float _rotationSpeed = 50f;
    [SerializeField] private float _pulseSpeed = 5f;

    private Material _ringMaterial;

    private void Awake()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            _ringMaterial = meshRenderer.material; // マテリアルをインスタンス化して個別に光らせる
        }
    }

    private void Update()
    {
        // 1. ゆっくり回転させる（タクティクス感が出る演出）
        transform.Rotate(Vector3.up * _rotationSpeed * Time.deltaTime);

        // 2. 発光の強さを脈動させる
        if (_ringMaterial != null)
        {
            float t = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) / 2f;
            float intensity = Mathf.Lerp(0, 1.0f, t);
            _ringMaterial.SetColor("_EmissionColor", Color.cyan * intensity);
        }
    }
}