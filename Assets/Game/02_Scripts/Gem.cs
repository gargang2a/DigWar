using UnityEngine;
using System;

namespace Game
{
    /// <summary>
    /// 수집 가능한 보석 아이템
    /// 회전 애니메이션은 선택적이며, 성능이 중요할 경우 비활성화 가능
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Gem : MonoBehaviour
    {
        [SerializeField] private int _scoreValue = 10;
        [SerializeField] private float _rotateSpeed = 90f;
        [SerializeField] private bool _enableRotation = true;

        /// <summary>
        /// 보석이 수집되었을 때 발생하는 이벤트 (풀링 매니저에서 구독)
        /// </summary>
        public event Action<Gem> OnCollected;

        public int ScoreValue => _scoreValue;

        private void Update()
        {
            if (_enableRotation)
            {
                // Z축 회전만 수행 (2D 게임)
                transform.Rotate(0f, 0f, _rotateSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// 플레이어가 보석을 획득했을 때 호출
        /// </summary>
        public void Collect()
        {
            // TODO: 파티클 이펙트, 사운드 재생
            OnCollected?.Invoke(this);
        }

        private void OnEnable()
        {
            // 풀에서 재활성화될 때 회전 초기화 (선택 사항)
            transform.rotation = Quaternion.identity;
        }
    }
}
