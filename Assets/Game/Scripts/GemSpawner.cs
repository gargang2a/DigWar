using UnityEngine;
using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// 보석을 맵에 생성하고 관리
    /// 오브젝트 풀링을 사용하여 WebGL 환경에서의 GC 스파이크를 방지
    /// </summary>
    public class GemSpawner : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Gem _gemPrefab;
        [SerializeField] private int _poolSize = 100;
        [SerializeField] private Vector2 _spawnAreaSize = new Vector2(48, 48);
        [SerializeField] private float _spawnInterval = 2f;
        [SerializeField] private int _maxActiveGems = 80;

        private readonly Queue<Gem> _pool = new Queue<Gem>();
        private readonly List<Gem> _activeGems = new List<Gem>();
        private float _timer;

        private void Start()
        {
            InitializePool();
            SpawnInitialGems();
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= _spawnInterval && _activeGems.Count < _maxActiveGems)
            {
                SpawnGem();
                _timer = 0f;
            }
        }

        private void InitializePool()
        {
            if (_gemPrefab == null)
            {
                Debug.LogError("[GemSpawner] Gem Prefab is not assigned!");
                return;
            }

            for (int i = 0; i < _poolSize; i++)
            {
                Gem gem = Instantiate(_gemPrefab, transform);
                gem.gameObject.SetActive(false);
                gem.OnCollected += ReturnToPool; // 이벤트 구독
                _pool.Enqueue(gem);
            }
        }

        private void SpawnInitialGems()
        {
            int initialCount = Mathf.Min(_poolSize / 2, _maxActiveGems);
            for (int i = 0; i < initialCount; i++)
            {
                SpawnGem();
            }
        }

        private void SpawnGem()
        {
            if (_pool.Count == 0) return;

            Gem gem = _pool.Dequeue();
            Vector2 randomPos = new Vector2(
                Random.Range(-_spawnAreaSize.x * 0.5f, _spawnAreaSize.x * 0.5f),
                Random.Range(-_spawnAreaSize.y * 0.5f, _spawnAreaSize.y * 0.5f)
            );

            gem.transform.position = randomPos;
            gem.gameObject.SetActive(true);
            _activeGems.Add(gem);
        }

        private void ReturnToPool(Gem gem)
        {
            if (_activeGems.Contains(gem))
            {
                _activeGems.Remove(gem);
            }
            gem.gameObject.SetActive(false);
            _pool.Enqueue(gem);
        }
    }
}
