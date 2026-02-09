using UnityEngine;
using UnityEngine.UI;
using TMPro; // Assuming TextMeshPro is used, if not we fall back to generic approach or standard Text

namespace Game
{
    public class UIManager : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _leaderboardText;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged += UpdateScoreUI;
                UpdateScoreUI(GameManager.Instance.CurrentScore);
            }

            UpdateLeaderboardUI(); // Set dummy data
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= UpdateScoreUI;
            }
        }

        private void UpdateScoreUI(int score)
        {
            if (_scoreText != null)
            {
                _scoreText.text = $"Score: {score}";
            }
        }

        private void UpdateLeaderboardUI()
        {
            if (_leaderboardText != null)
            {
                // Dummy Data
                _leaderboardText.text = "1. MoleKing  9999\n2. DiggerPro 5000\n3. You       0";
            }
        }
    }
}
