using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Match3.View
{
    public sealed class UIManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _movesText;
        [SerializeField] private TextMeshProUGUI _targetScoreText;
        [SerializeField] private GameObject _winPanel;
        [SerializeField] private GameObject _losePanel;
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private JuiceManager _juiceManager;
        [SerializeField] private float _scorePunchScale = 0.3f;
        [SerializeField] private float _scorePunchDuration = 0.25f;
        [SerializeField] private int _scorePunchVibrato = 6;
        [SerializeField] private float _scorePunchElasticity = 0.6f;

        private void OnEnable()
        {
            _gameManager.Initialized += HandleInitialized;
            _gameManager.OnScoreChanged += UpdateScore;
            _gameManager.OnMovesChanged += UpdateMoves;
            _gameManager.OnWin += ShowWin;
            _gameManager.OnLose += ShowLose;
            _juiceManager.ScoreWorldTarget = GetScoreWorldPosition;

            if (_gameManager.IsInitialized)
                HandleInitialized();
        }

        private void Start()
        {
            _winPanel.SetActive(false);
            _losePanel.SetActive(false);
        }

        private void OnDisable()
        {
            if (_gameManager != null)
            {
                _gameManager.Initialized -= HandleInitialized;
                _gameManager.OnScoreChanged -= UpdateScore;
                _gameManager.OnMovesChanged -= UpdateMoves;
                _gameManager.OnWin -= ShowWin;
                _gameManager.OnLose -= ShowLose;
            }

            if (_juiceManager != null && _juiceManager.ScoreWorldTarget == GetScoreWorldPosition)
                _juiceManager.ScoreWorldTarget = null;
        }

        private void HandleInitialized()
        {
            UpdateScore(_gameManager.Score);
            UpdateMoves(_gameManager.MovesLeft);
            UpdateTargetScore(_gameManager.TargetScore);
        }

        private Vector3 GetScoreWorldPosition()
        {
            if (_scoreText == null || Camera.main == null)
                return Vector3.zero;

            var canvas = _scoreText.canvas;
            Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;

            Vector3 screenPosition = RectTransformUtility.WorldToScreenPoint(uiCamera, _scoreText.transform.position);
            float depth = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(
                new Vector3(screenPosition.x, screenPosition.y, depth));
            worldPosition.z = 0f;
            return worldPosition;
        }

        private void UpdateScore(int score)
        {
            if (_scoreText == null)
                return;

            _scoreText.text = score.ToString();
            _scoreText.transform.DOKill();
            _scoreText.transform.localScale = Vector3.one;
            _scoreText.transform.DOPunchScale(
                Vector3.one * _scorePunchScale,
                _scorePunchDuration,
                _scorePunchVibrato,
                _scorePunchElasticity);
        }

        private void UpdateMoves(int moves)
        {
            if (_movesText != null)
                _movesText.text = moves.ToString();
        }

        private void UpdateTargetScore(int target)
        {
            if (_targetScoreText != null)
                _targetScoreText.text = target.ToString();
        }

        private void ShowWin()
        {
            if (_winPanel != null)
                _winPanel.SetActive(true);
        }

        private void ShowLose()
        {
            if (_losePanel != null)
                _losePanel.SetActive(true);
        }

        public void OnRestartPressed()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
