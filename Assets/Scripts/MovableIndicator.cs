using UnityEngine;

public class MovableIndicator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float _pulseSpeed = 10f;

    private Material _indicatorMaterial;

    private void Awake()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        _indicatorMaterial = meshRenderer.material; // マテリアルをインスタンス化

    }

    private void Update()
    {
        if (_indicatorMaterial != null)
        {
            float t = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) / 2f;
            float intensity = Mathf.Lerp(0f, 1.0f, t);
            _indicatorMaterial.SetColor("_EmissionColor", Color.cyan * intensity);
        }
    }
}