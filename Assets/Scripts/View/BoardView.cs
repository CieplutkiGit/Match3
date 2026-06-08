using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Match3.Model;
using Match3.Core;
using Match3.Data;

namespace Match3.View
{
    public class BoardView : MonoBehaviour
    {
        [SerializeField] private PieceView _piecePrefab;
        [SerializeField] private PieceSpriteConfig _spriteConfig;
        [SerializeField] private float _spacing       = 1.0f;
        [SerializeField] private float _swapDuration  = 0.25f;
        [SerializeField] private float _fallDuration  = 0.3f;
        [SerializeField] private float _destroyDuration = 0.2f;

        private BoardSystem _boardSystem;
        private PieceView[,] _pieceViews;

        public bool IsAnimating { get; private set; }

        public void Initialize(BoardSystem boardSystem)
        {
            _boardSystem = boardSystem;
            _pieceViews  = new PieceView[_boardSystem.Grid.Width, _boardSystem.Grid.Height];
            SpawnInitialVisuals();
        }

        private void SpawnInitialVisuals()
        {
            var grid = _boardSystem.Grid;
            for (int x = 0; x < grid.Width; x++)
                for (int y = 0; y < grid.Height; y++)
                {
                    var data = grid.Get(x, y);
                    if (data != null && data.Color != PieceColor.None)
                        SpawnAt(x, y, data.Color, data.Type, GetWorldPosition(x, y), false);
                }
        }

        private PieceView SpawnAt(int x, int y, PieceColor color, PieceType type, Vector3 worldPos, bool animate)
        {
            var pieceInstance = Instantiate(_piecePrefab, worldPos, Quaternion.identity, transform);
            var sprite        = _spriteConfig.GetSprite(color, type);
            pieceInstance.Setup(x, y, color, type, sprite);
            _pieceViews[x, y] = pieceInstance;
            if (animate) pieceInstance.PlaySpawnAnimation();
            return pieceInstance;
        }

        public IEnumerator AnimateSwap(int x1, int y1, int x2, int y2, bool revert)
        {
            IsAnimating = true;

            var v1 = GetPieceView(x1, y1);
            var v2 = GetPieceView(x2, y2);

            if (v1 == null || v2 == null) { IsAnimating = false; yield break; }

            int tweensComplete = 0;
            bool done = false;

            v1.MoveTo(GetWorldPosition(x2, y2), _swapDuration, Ease.OutCubic, () => { tweensComplete++; if (tweensComplete >= 2) done = true; });
            v2.MoveTo(GetWorldPosition(x1, y1), _swapDuration, Ease.OutCubic, () => { tweensComplete++; if (tweensComplete >= 2) done = true; });

            yield return new WaitUntil(() => done);

            if (!revert)
            {
                _pieceViews[x1, y1] = v2;
                _pieceViews[x2, y2] = v1;
                v1.UpdatePosition(x2, y2);
                v2.UpdatePosition(x1, y1);
            }
            else
            {
                v1.MoveTo(GetWorldPosition(x1, y1), _swapDuration * 0.5f, Ease.OutCubic);
                v2.MoveTo(GetWorldPosition(x2, y2), _swapDuration * 0.5f, Ease.OutCubic);
                yield return new WaitForSeconds(_swapDuration * 0.5f);
            }

            IsAnimating = false;
        }

        public IEnumerator AnimateDestroy(List<MatchResult> matches)
        {
            IsAnimating = true;

            var piecesToDestroy = new HashSet<PieceView>();
            var specialViews    = new HashSet<PieceView>();

            foreach (var match in matches)
                foreach (var pieceData in match.MatchedPieces)
                {
                    var view = GetPieceView(pieceData.X, pieceData.Y);
                    if (view == null) continue;
                    piecesToDestroy.Add(view);
                    if (pieceData.Type != PieceType.Normal)
                        specialViews.Add(view);
                }

            if (piecesToDestroy.Count == 0) { IsAnimating = false; yield break; }

            if (specialViews.Count > 0)
            {
                if (Camera.main != null)
                    Camera.main.transform.DOShakePosition(0.5f, 0.45f, 20, 90f, false, true);
                transform.DOShakePosition(0.4f, 0.28f, 16, 90f, false, true);
            }

            int animatingCount = piecesToDestroy.Count;
            bool done = false;

            foreach (var view in piecesToDestroy)
            {
                _pieceViews[view.X, view.Y] = null;
                if (specialViews.Contains(view))
                    view.PlaySpecialDestroyAnimation(_destroyDuration * 2.5f, () => { animatingCount--; if (animatingCount <= 0) done = true; });
                else
                    view.PlayDestroyAnimation(_destroyDuration, () => { animatingCount--; if (animatingCount <= 0) done = true; });
            }

            yield return new WaitUntil(() => done);
            IsAnimating = false;
        }

        public IEnumerator AnimateFall(List<PieceFallInfo> fallInfos, List<PieceData> spawnedPieces)
        {
            IsAnimating = true;

            var spawnedPieceSet = new HashSet<PieceData>(spawnedPieces);
            int tweensComplete  = 0;
            int totalTweens     = fallInfos.Count;

            if (totalTweens == 0) { IsAnimating = false; yield break; }

            var remapped = new Dictionary<(int, int), PieceView>();
            foreach (var info in fallInfos)
            {
                var data = _boardSystem.Grid.Get(info.ToX, info.ToY);
                if (data == null || spawnedPieceSet.Contains(data)) continue;
                var view = _pieceViews[info.FromX, info.FromY];
                if (view != null)
                {
                    _pieceViews[info.FromX, info.FromY] = null;
                    remapped[(info.ToX, info.ToY)] = view;
                    view.UpdatePosition(info.ToX, info.ToY);
                }
            }

            foreach (var kv in remapped)
                _pieceViews[kv.Key.Item1, kv.Key.Item2] = kv.Value;

            foreach (var info in fallInfos)
            {
                var data = _boardSystem.Grid.Get(info.ToX, info.ToY);
                PieceView view;

                if (data != null && spawnedPieceSet.Contains(data))
                    view = SpawnAt(info.ToX, info.ToY, data.Color, data.Type, GetWorldPosition(info.FromX, info.FromY), false);
                else
                    view = _pieceViews[info.ToX, info.ToY];

                if (view == null) { tweensComplete++; continue; }

                view.MoveTo(GetWorldPosition(info.ToX, info.ToY), _fallDuration, Ease.OutBounce, () => tweensComplete++);
            }

            yield return new WaitUntil(() => tweensComplete >= totalTweens);
            IsAnimating = false;
        }

        public Vector3 GetWorldPosition(int x, int y)
        {
            float xPos = (x - (_boardSystem.Grid.Width  - 1) * 0.5f) * _spacing;
            float yPos = (y - (_boardSystem.Grid.Height - 1) * 0.5f) * _spacing;
            return new Vector3(xPos, yPos, 0f) + transform.position;
        }

        public PieceView GetPieceView(int x, int y)
        {
            if (x >= 0 && x < _pieceViews.GetLength(0) && y >= 0 && y < _pieceViews.GetLength(1))
                return _pieceViews[x, y];
            return null;
        }
    }
}
