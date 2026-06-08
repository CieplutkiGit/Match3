using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Match3.Core;
using Match3.Data;
using Match3.Model;
using Match3.View;

namespace Match3.View
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private LevelSettings _levelSettings;
        [SerializeField] private BoardView _boardView;
        [SerializeField] private InputManager _inputManager;

        private BoardSystem _boardSystem;
        private MatchDetector _matchDetector;

        private GameState _state = GameState.Initializing;

        private int _score;
        private int _movesLeft;

        public int Score => _score;
        public int MovesLeft => _movesLeft;

        public event System.Action<int> OnScoreChanged;
        public event System.Action<int> OnMovesChanged;
        public event System.Action OnWin;
        public event System.Action OnLose;

        private void Start()
        {
            _matchDetector = new MatchDetector();
            _boardSystem = new BoardSystem(_levelSettings, _matchDetector);
            _boardSystem.FillBoard();
            _boardView.Initialize(_boardSystem);

            _movesLeft = _levelSettings.MaxMoves;
            _score = 0;

            _inputManager.OnSwapRequested += HandleSwapRequested;

            _state = GameState.AwaitingInput;
        }

        private void OnDestroy()
        {
            if (_inputManager != null)
            {
                _inputManager.OnSwapRequested -= HandleSwapRequested;
            }
        }

        private void HandleSwapRequested(int x1, int y1, int x2, int y2)
        {
            if (_state != GameState.AwaitingInput) return;
            StartCoroutine(ProcessSwap(x1, y1, x2, y2));
        }

        private IEnumerator ProcessSwap(int x1, int y1, int x2, int y2)
        {
            _state = GameState.Animating;

            bool validSwap = _boardSystem.TrySwap(x1, y1, x2, y2, out List<MatchResult> matches);

            yield return StartCoroutine(_boardView.AnimateSwap(x1, y1, x2, y2, !validSwap));

            if (!validSwap)
            {
                _state = GameState.AwaitingInput;
                yield break;
            }

            _movesLeft--;
            OnMovesChanged?.Invoke(_movesLeft);

            yield return StartCoroutine(ProcessChainReaction(matches, x1, y1));

            if (_score >= _levelSettings.TargetScore)
            {
                _state = GameState.GameOver;
                OnWin?.Invoke();
                yield break;
            }

            if (_movesLeft <= 0)
            {
                _state = GameState.GameOver;
                OnLose?.Invoke();
                yield break;
            }

            _state = GameState.AwaitingInput;
        }

        private IEnumerator ProcessChainReaction(List<MatchResult> matches, int focusX = -1, int focusY = -1)
        {
            while (matches != null && matches.Count > 0)
            {
                int gained = 0;
                foreach (var m in matches)
                {
                    gained += m.MatchedPieces.Count * 10;
                }

                _score += gained;
                OnScoreChanged?.Invoke(_score);

                _boardSystem.ClearMatches(matches, focusX, focusY);
                yield return StartCoroutine(_boardView.AnimateDestroy(matches));

                var fallInfos = _boardSystem.ApplyGravityAndRefill(out List<PieceData> spawnedPieces);
                _boardView.RefreshView();

                yield return StartCoroutine(_boardView.AnimateFall(fallInfos, spawnedPieces));

                matches = _matchDetector.FindMatches(_boardSystem.Grid);
                focusX = -1;
                focusY = -1;
            }
        }
    }
}
