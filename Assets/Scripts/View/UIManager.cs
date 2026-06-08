using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        private void Start()
        {
            _winPanel.SetActive(false);
            _losePanel.SetActive(false);

            _gameManager.OnScoreChanged += UpdateScore;
            _gameManager.OnMovesChanged += UpdateMoves;
            _gameManager.OnWin += ShowWin;
            _gameManager.OnLose += ShowLose;

            UpdateScore(_gameManager.Score);
            UpdateMoves(_gameManager.MovesLeft);
            UpdateTargetScore(_gameManager.TargetScore);
        }

        private bool _targetRegistered;

        private void Update()
        {
            if (_targetRegistered || JuiceManager.Instance == null) return;
            JuiceManager.Instance.ScoreWorldTarget = GetScoreWorldPosition;
            _targetRegistered = true;
        }

        private Vector3 GetScoreWorldPosition()
        {
            if (_scoreText == null || Camera.main == null) return Vector3.zero;

            var canvas = _scoreText.canvas;
            Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;

            Vector3 screen = RectTransformUtility.WorldToScreenPoint(uiCamera, _scoreText.transform.position);
            float depth = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 world = Camera.main.ScreenToWorldPoint(new Vector3(screen.x, screen.y, depth));
            world.z = 0f;
            return world;
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
            if (_scoreText == null) return;
            _scoreText.text = score.ToString();
            _scoreText.transform.DOKill();
            _scoreText.transform.localScale = Vector3.one;
            _scoreText.transform.DOPunchScale(Vector3.one * 0.3f, 0.25f, 6, 0.6f);
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
