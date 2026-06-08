using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Match3.Core;
using Match3.Data;
using Match3.Model;

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
        public int TargetScore => _levelSettings != null ? _levelSettings.TargetScore : 0;

        public event Action<int> OnScoreChanged;
        public event Action<int> OnMovesChanged;
        public event Action OnWin;
        public event Action OnLose;

        private void Start()
        {
            _matchDetector = new MatchDetector();
            _boardSystem = new BoardSystem(_levelSettings, _matchDetector);
            _boardSystem.FillBoard();
            _boardView.Initialize(_boardSystem);

            ConfigureBackdrop();

            _movesLeft = _levelSettings.MaxMoves;
            _score = 0;

            if (SoundManager.Instance == null)
                new GameObject("SoundManager").AddComponent<SoundManager>();
            new GameObject("JuiceManager").AddComponent<JuiceManager>();
            ConfigureJuiceBar();
            if (JuiceManager.Instance != null)
            {
                JuiceManager.Instance.DisplayScoreChanged += s => OnScoreChanged?.Invoke(s);
                JuiceManager.Instance.InitializeDisplay(0, 0f);
            }

            _inputManager.OnSwapRequested += HandleSwapRequested;
            _state = GameState.AwaitingInput;
        }

        private void ConfigureBackdrop()
        {
            int w = _boardSystem.Grid.Width;
            int h = _boardSystem.Grid.Height;

            Vector3 origin = _boardView.GetWorldPosition(0, 0);
            float cell = 1f;
            if (w > 1) cell = Mathf.Abs(_boardView.GetWorldPosition(1, 0).x - origin.x);
            else if (h > 1) cell = Mathf.Abs(_boardView.GetWorldPosition(0, 1).y - origin.y);

            var parent = new GameObject("Backdrop").transform;
            new BoardBackdrop().Build(parent, w, h, _boardView.GetWorldPosition, cell);
        }

        private void ConfigureJuiceBar()
        {
            if (JuiceManager.Instance == null) return;

            int gh = _boardSystem.Grid.Height;
            Vector3 bottom = _boardView.GetWorldPosition(0, 0);
            Vector3 top = _boardView.GetWorldPosition(0, gh - 1);

            float cell = gh > 1 ? (top.y - bottom.y) / (gh - 1) : 1f;
            float boardHeight = (top.y - bottom.y) + cell;
            float centerY = (top.y + bottom.y) * 0.5f;
            float barWidth = cell * 0.9f;
            float boardLeftEdge = bottom.x - cell * 0.5f;
            float barX = boardLeftEdge - cell * 0.7f - barWidth * 0.5f;

            JuiceManager.Instance.Configure(new Vector3(barX, centerY, 0f), boardHeight, barWidth);
        }

        private void OnDestroy()
        {
            if (_inputManager != null)
                _inputManager.OnSwapRequested -= HandleSwapRequested;
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
            SoundManager.Instance?.PlaySwap();
            yield return StartCoroutine(_boardView.AnimateSwap(x1, y1, x2, y2, !validSwap));

            if (!validSwap)
            {
                SoundManager.Instance?.PlayInvalid();
                _state = GameState.AwaitingInput;
                yield break;
            }

            _movesLeft--;
            OnMovesChanged?.Invoke(_movesLeft);

            yield return StartCoroutine(ProcessChainReaction(matches, x2, y2));

            if (_score >= _levelSettings.TargetScore)
            {
                _state = GameState.GameOver;
                SoundManager.Instance?.PlayWin();
                OnWin?.Invoke();
                yield break;
            }

            if (_movesLeft <= 0)
            {
                _state = GameState.GameOver;
                SoundManager.Instance?.PlayLose();
                OnLose?.Invoke();
                yield break;
            }

            _state = GameState.AwaitingInput;
        }

        private IEnumerator ProcessChainReaction(List<MatchResult> matches, int focusX = -1, int focusY = -1)
        {
            int combo = 0;
            while (matches != null && matches.Count > 0)
            {
                _boardSystem.ClearMatches(matches, focusX, focusY);

                bool hasBlast = _boardSystem.LastBlastPieces.Count > 0;
                if (hasBlast)
                {
                    var blastResult = new MatchResult();
                    foreach (var p in _boardSystem.LastBlastPieces)
                        blastResult.AddPiece(p);
                    matches.Add(blastResult);
                }

                int gained = 0;
                foreach (var m in matches)
                    gained += m.MatchedPieces.Count * 10;
                _score += gained;

                float progress = _levelSettings != null
                    ? (float)_score / Mathf.Max(1, _levelSettings.TargetScore)
                    : 0f;
                JuiceManager.Instance?.SetTargets(_score, progress);

                if (hasBlast)
                    SoundManager.Instance?.PlayBlast();
                SoundManager.Instance?.PlayPop(combo);
                combo++;

                yield return StartCoroutine(_boardView.AnimateDestroy(matches));

                var fallInfos = _boardSystem.ApplyGravityAndRefill(out List<PieceData> spawnedPieces);
                yield return StartCoroutine(_boardView.AnimateFall(fallInfos, spawnedPieces));

                matches = _matchDetector.FindMatches(_boardSystem.Grid);
                focusX = -1;
                focusY = -1;
            }
        }
    }
}
