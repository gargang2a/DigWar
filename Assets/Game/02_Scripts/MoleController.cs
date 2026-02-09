using UnityEngine;

namespace Game
{
    /// <summary>
    /// 두더지 플레이어의 이동, 굴착, 성장을 담당하는 컨트롤러
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class MoleController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _turnSpeed = 180f;

        [Header("Digging")]
        [SerializeField] private float _baseDigRadius = 0.5f;

        [Header("Growth")]
        [SerializeField] private float _growthFactor = 0.005f; // 성장 계수 (점수당)
        [SerializeField] private float _maxScale = 3f;
        [SerializeField] private float _growthLerpSpeed = 2f; // 부드러운 성장을 위한 보간 속도

        private Vector3 _initialScale;
        private float _targetScale = 1f;
        private float _currentScale = 1f;

        private void Start()
        {
            _initialScale = transform.localScale;
        }

        private void Update()
        {
            HandleMovement();
            //HandleDigging();
            //HandleGrowthLerp();
        }

        private void HandleMovement()
        {
            // 지속적인 전진 이동
            transform.Translate(Vector3.up * (_moveSpeed * Time.deltaTime));

            // 좌우 조향
            float turn = -Input.GetAxis("Horizontal");
            transform.Rotate(0f, 0f, turn * _turnSpeed * Time.deltaTime);
        }

        private void HandleDigging()
        {
            if (SoilManager.Instance == null) return;

            // 현재 스케일에 비례한 굴착 반경
            float currentRadius = _baseDigRadius * _currentScale;
            SoilManager.Instance.Dig(transform.position, currentRadius);
        }

        private void HandleGrowthLerp()
        {
            // 부드러운 성장 보간
            if (!Mathf.Approximately(_currentScale, _targetScale))
            {
                _currentScale = Mathf.Lerp(_currentScale, _targetScale, _growthLerpSpeed * Time.deltaTime);
                transform.localScale = _initialScale * _currentScale;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // 보석 수집 처리
            if (other.TryGetComponent(out Gem gem))
            {
                CollectGem(gem);
            }
        }

        private void CollectGem(Gem gem)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(gem.ScoreValue);
                UpdateGrowthTarget(GameManager.Instance.CurrentScore);
            }
            gem.Collect();
        }

        private void UpdateGrowthTarget(int score)
        {
            // 목표 스케일 계산 (부드러운 성장을 위해 즉시 적용하지 않음)
            _targetScale = Mathf.Min(1f + score * _growthFactor, _maxScale);
        }
    }
}
