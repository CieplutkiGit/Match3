using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Match3.View
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _movesText;
        [SerializeField] private TextMeshProUGUI _targetScoreText;
        [SerializeField] private GameObject _winPanel;
        [SerializeField] private GameObject _losePanel;
        [SerializeField] private GameManager _gameManager;

        private int _targetScore;

        private void Start()
        {
            _targetScore = FindFirstObjectByType<GameManager>() != null
                ? 0
                : 0;

            _winPanel.SetActive(false);
            _losePanel.SetActive(false);

            _gameManager.OnScoreChanged += UpdateScore;
            _gameManager.OnMovesChanged += UpdateMoves;
            _gameManager.OnWin += ShowWin;
            _gameManager.OnLose += ShowLose;

            UpdateScore(_gameManager.Score);
            UpdateMoves(_gameManager.MovesLeft);
        }

        private void OnDestroy()
        {
            if (_gameManager == null) return;
            _gameManager.OnScoreChanged -= UpdateScore;
            _gameManager.OnMovesChanged -= UpdateMoves;
            _gameManager.OnWin -= ShowWin;
            _gameManager.OnLose -= ShowLose;
        }

        private void UpdateScore(int score)
        {
            if (_scoreText != null)
            {
                _scoreText.text = score.ToString();
            }
        }

        private void UpdateMoves(int moves)
        {
            if (_movesText != null)
            {
                _movesText.text = moves.ToString();
            }
        }

        private void ShowWin()
        {
            if (_winPanel != null)
            {
                _winPanel.SetActive(true);
            }
        }

        private void ShowLose()
        {
            if (_losePanel != null)
            {
                _losePanel.SetActive(true);
            }
        }

        public void SetTargetScore(int target)
        {
            _targetScore = target;
            if (_targetScoreText != null)
            {
                _targetScoreText.text = target.ToString();
            }
        }
    }
}
