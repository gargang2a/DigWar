using UnityEngine;

namespace Game
{
    /// <summary>
    /// 타겟을 부드럽게 추적하고, 타겟의 크기에 따라 줌을 조절하는 카메라 컨트롤러
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        [Header("Follow")]
        [SerializeField] private Transform _target;
        [SerializeField] private float _followSmoothTime = 0.15f;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 0f, -10f);

        [Header("Zoom")]
        [SerializeField] private float _baseSize = 5f;
        [SerializeField] private float _zoomFactor = 2f;
        [SerializeField] private float _zoomSmoothTime = 0.3f;

        private Camera _cam;
        private Vector3 _velocity = Vector3.zero;
        private float _zoomVelocity;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            FollowTarget();
            UpdateZoom();
        }

        private void FollowTarget()
        {
            Vector3 desiredPosition = _target.position + _offset;
            // SmoothDamp 사용: Lerp보다 자연스러운 감속 효과
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref _velocity,
                _followSmoothTime
            );
        }

        private void UpdateZoom()
        {
            float targetZoom = _baseSize + (_target.localScale.x - 1f) * _zoomFactor;
            // SmoothDamp로 부드러운 줌 전환
            _cam.orthographicSize = Mathf.SmoothDamp(
                _cam.orthographicSize,
                targetZoom,
                ref _zoomVelocity,
                _zoomSmoothTime
            );
        }

        /// <summary>
        /// 런타임에 추적 대상을 변경합니다.
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            _target = newTarget;
        }
    }
}
