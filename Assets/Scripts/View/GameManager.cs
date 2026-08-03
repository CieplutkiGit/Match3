using System;
using System.Collections;
using System.Collections.Generic;
using Match3.Core;
using Match3.Data;
using Match3.Model;
using UnityEngine;

namespace Match3.View
{
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private LevelSettings _levelSettings;
        [SerializeField] private BoardView _boardView;
        [SerializeField] private InputManager _inputManager;
        [SerializeField] private SoundManager _soundManager;
        [SerializeField] private JuiceManager _juiceManager;

        private BoardSystem _boardSystem;
        private IMatchDetector _matchDetector;
        private GameState _state = GameState.Initializing;
        private int _score;
        private int _movesLeft;

        public int Score => _score;
        public int MovesLeft => _movesLeft;
        public int TargetScore => _levelSettings != null ? _levelSettings.TargetScore : 0;
        public bool IsInitialized { get; private set; }

        public event Action Initialized;
        public event Action<int> OnScoreChanged;
        public event Action<int> OnMovesChanged;
        public event Action OnWin;
        public event Action OnLose;

        private void Awake()
        {
            ValidateReferences();
            _levelSettings.ValidateGameplaySettings();
        }

        private void OnEnable()
        {
            if (_inputManager != null)
                _inputManager.OnSwapRequested += HandleSwapRequested;
            if (_juiceManager != null)
                _juiceManager.DisplayScoreChanged += HandleDisplayedScoreChanged;
        }

        private void Start()
        {
            _matchDetector = new MatchDetector(_levelSettings.CreateMatchRules());
            _boardSystem = new BoardSystem(_levelSettings.CreateBoardConfiguration(), _matchDetector);
            _boardSystem.FillBoard();
            _boardView.Initialize(_boardSystem, _juiceManager);

            _movesLeft = _levelSettings.MaxMoves;
            _score = 0;

            ConfigureJuiceBar();
            _juiceManager.InitializeDisplay(_score, GetScoreProgress());

            _state = GameState.AwaitingInput;
            IsInitialized = true;
            Initialized?.Invoke();
            OnScoreChanged?.Invoke(_score);
            OnMovesChanged?.Invoke(_movesLeft);
        }

        private void OnDisable()
        {
            if (_inputManager != null)
                _inputManager.OnSwapRequested -= HandleSwapRequested;
            if (_juiceManager != null)
                _juiceManager.DisplayScoreChanged -= HandleDisplayedScoreChanged;
        }

        private void ValidateReferences()
        {
            if (_levelSettings == null)
                throw new InvalidOperationException("Level settings are required.");
            if (_boardView == null)
                throw new InvalidOperationException("Board view is required.");
            if (_inputManager == null)
                throw new InvalidOperationException("Input manager is required.");
            if (_soundManager == null)
                throw new InvalidOperationException("Sound manager is required.");
            if (_juiceManager == null)
                throw new InvalidOperationException("Juice manager is required.");
        }

        private void ConfigureJuiceBar()
        {
            int gridHeight = _boardSystem.Grid.Height;
            Vector3 bottom = _boardView.GetWorldPosition(0, 0);
            Vector3 top = _boardView.GetWorldPosition(0, gridHeight - 1);
            float cellSize = _boardView.Spacing;
            float boardHeight = top.y - bottom.y + cellSize;
            float centerY = (top.y + bottom.y) * 0.5f;
            float barWidth = cellSize * _boardView.JuiceBarWidthRatio;
            float boardLeftEdge = bottom.x - cellSize * 0.5f;
            float barX = boardLeftEdge - cellSize * _boardView.JuiceBarOffset - barWidth * 0.5f;

            _juiceManager.Configure(new Vector3(barX, centerY, bottom.z), boardHeight, barWidth);
        }

        private void HandleDisplayedScoreChanged(int score)
        {
            OnScoreChanged?.Invoke(score);
        }

        private void HandleSwapRequested(int x1, int y1, int x2, int y2)
        {
            if (_state != GameState.AwaitingInput)
                return;

            StartCoroutine(ProcessSwap(x1, y1, x2, y2));
        }

        private IEnumerator ProcessSwap(int x1, int y1, int x2, int y2)
        {
            _state = GameState.Animating;

            bool validSwap = _boardSystem.TrySwap(x1, y1, x2, y2, out List<MatchResult> matches);
            _soundManager.PlaySwap();
            yield return StartCoroutine(_boardView.AnimateSwap(x1, y1, x2, y2, !validSwap));

            if (!validSwap)
            {
                _soundManager.PlayInvalid();
                _state = GameState.AwaitingInput;
                yield break;
            }

            _movesLeft--;
            OnMovesChanged?.Invoke(_movesLeft);

            yield return StartCoroutine(ProcessChainReaction(matches, new GridPosition(x2, y2)));

            if (_score >= _levelSettings.TargetScore)
            {
                _state = GameState.GameOver;
                _soundManager.PlayWin();
                OnWin?.Invoke();
                yield break;
            }

            if (_movesLeft <= 0)
            {
                _state = GameState.GameOver;
                _soundManager.PlayLose();
                OnLose?.Invoke();
                yield break;
            }

            _state = GameState.AwaitingInput;
        }

        private IEnumerator ProcessChainReaction(
            List<MatchResult> matches,
            GridPosition? focus)
        {
            int combo = 0;
            while (matches.Count > 0)
            {
                var resolution = _boardSystem.ResolveMatches(matches, focus);
                _score += resolution.ClearedPieces.Count * _levelSettings.PointsPerPiece;
                _juiceManager.SetTargets(_score, GetScoreProgress());

                if (resolution.ActivatedSpecials.Count > 0)
                    _soundManager.PlayBlast();
                _soundManager.PlayPop(combo);
                combo++;

                yield return StartCoroutine(_boardView.AnimateResolution(resolution));

                var fallInfos = _boardSystem.ApplyGravityAndRefill(out List<PieceData> spawnedPieces);
                yield return StartCoroutine(_boardView.AnimateFall(fallInfos, spawnedPieces));

                matches = _matchDetector.FindMatches(_boardSystem.Grid);
                focus = null;
            }
        }

        private float GetScoreProgress()
        {
            return (float)_score / _levelSettings.TargetScore;
        }
    }
}
