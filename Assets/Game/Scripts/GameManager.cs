using UnityEngine;
using System;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public event Action<int> OnScoreChanged;
        public int CurrentScore { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void AddScore(int amount)
        {
            CurrentScore += amount;
            OnScoreChanged?.Invoke(CurrentScore);
        }

        public void GameOver()
        {
            Debug.Log("Game Over!");
            // Future: Show Game Over UI, Stop Time, etc.
        }
    }
}
