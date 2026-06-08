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
        [SerializeField] private float _spacing = 1.0f;
        [SerializeField] private float _swapDuration = 0.25f;
        [SerializeField] private float _fallDuration = 0.3f;
        [SerializeField] private float _destroyDuration = 0.2f;

        private BoardSystem _boardSystem;
        private PieceView[,] _pieceViews;

        public bool IsAnimating { get; private set; }

        public void Initialize(BoardSystem boardSystem)
        {
            _boardSystem = boardSystem;
            _pieceViews = new PieceView[_boardSystem.Grid.Width, _boardSystem.Grid.Height];
            SpawnInitialVisuals();
        }

        private void SpawnInitialVisuals()
        {
            var grid = _boardSystem.Grid;
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var data = grid.Get(x, y);
                    if (data != null && data.Color != PieceColor.None)
                    {
                        SpawnPieceView(x, y, data.Color, data.Type, false);
                    }
                }
            }
        }

        private PieceView SpawnPieceView(int x, int y, PieceColor color, PieceType type, bool animate = true)
        {
            var worldPos = GetWorldPosition(x, y);
            var pieceInstance = Instantiate(_piecePrefab, worldPos, Quaternion.identity, transform);
            var sprite = _spriteConfig.GetSprite(color, type);
            pieceInstance.Setup(x, y, color, type, sprite);
            _pieceViews[x, y] = pieceInstance;

            if (animate)
            {
                pieceInstance.PlaySpawnAnimation();
            }

            return pieceInstance;
        }

        public IEnumerator AnimateSwap(int x1, int y1, int x2, int y2, bool revert)
        {
            IsAnimating = true;

            var v1 = GetPieceView(x1, y1);
            var v2 = GetPieceView(x2, y2);

            if (v1 == null || v2 == null)
            {
                IsAnimating = false;
                yield break;
            }

            var pos1 = GetWorldPosition(x1, y1);
            var pos2 = GetWorldPosition(x2, y2);

            bool done = false;
            int tweensComplete = 0;

            v1.MoveTo(pos2, _swapDuration, Ease.OutCubic, () =>
            {
                tweensComplete++;
                if (tweensComplete >= 2) done = true;
            });
            v2.MoveTo(pos1, _swapDuration, Ease.OutCubic, () =>
            {
                tweensComplete++;
                if (tweensComplete >= 2) done = true;
            });

            yield return new WaitUntil(() => done);

            if (!revert)
            {
                _pieceViews[x1, y1] = v2;
                _pieceViews[x2, y2] = v1;
                v1.UpdatePosition(x2, y2);
                v2.UpdatePosition(x1, y1);
            }

            IsAnimating = false;
        }

        public IEnumerator AnimateDestroy(List<MatchResult> matches)
        {
            IsAnimating = true;

            var piecesToDestroy = new HashSet<PieceView>();
            foreach (var match in matches)
            {
                foreach (var pieceData in match.MatchedPieces)
                {
                    var view = GetPieceView(pieceData.X, pieceData.Y);
                    if (view != null)
                    {
                        piecesToDestroy.Add(view);
                    }
                }
            }

            bool done = false;
            int animatingCount = piecesToDestroy.Count;

            if (animatingCount == 0)
            {
                IsAnimating = false;
                yield break;
            }

            foreach (var view in piecesToDestroy)
            {
                _pieceViews[view.X, view.Y] = null;
                view.PlayDestroyAnimation(_destroyDuration, () =>
                {
                    animatingCount--;
                    if (animatingCount <= 0) done = true;
                });
            }

            yield return new WaitUntil(() => done);
            IsAnimating = false;
        }

        public IEnumerator AnimateFall(List<PieceFallInfo> fallInfos, List<PieceData> spawnedPieces)
        {
            IsAnimating = true;

            var spawnedPieceSet = new HashSet<PieceData>(spawnedPieces);
            int tweensComplete = 0;
            int totalTweens = fallInfos.Count;

            if (totalTweens == 0)
            {
                IsAnimating = false;
                yield break;
            }

            foreach (var info in fallInfos)
            {
                PieceView view;
                var data = _boardSystem.Grid.Get(info.ToX, info.ToY);

                if (data != null && spawnedPieceSet.Contains(data))
                {
                    Vector3 spawnWorldPos = GetWorldPosition(info.FromX, info.FromY);
                    view = SpawnPieceView(info.ToX, info.ToY, data.Color, data.Type, false);
                    view.transform.position = spawnWorldPos;
                }
                else
                {
                    view = _pieceViews[info.ToX, info.ToY];
                    _pieceViews[info.ToX, info.ToY] = view;
                }

                if (view == null)
                {
                    tweensComplete++;
                    continue;
                }

                Vector3 targetWorldPos = GetWorldPosition(info.ToX, info.ToY);
                view.MoveTo(targetWorldPos, _fallDuration, Ease.OutBounce, () =>
                {
                    tweensComplete++;
                });
            }

            yield return new WaitUntil(() => tweensComplete >= totalTweens);
            IsAnimating = false;
        }

        public void RefreshView()
        {
            var grid = _boardSystem.Grid;
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var data = grid.Get(x, y);
                    if (data != null && _pieceViews[x, y] != null)
                    {
                        _pieceViews[x, y].UpdatePosition(x, y);
                    }
                }
            }
        }

        public Vector3 GetWorldPosition(int x, int y)
        {
            float xPos = (x - (_boardSystem.Grid.Width - 1) * 0.5f) * _spacing;
            float yPos = (y - (_boardSystem.Grid.Height - 1) * 0.5f) * _spacing;
            return new Vector3(xPos, yPos, 0f) + transform.position;
        }

        public PieceView GetPieceView(int x, int y)
        {
            if (x >= 0 && x < _pieceViews.GetLength(0) && y >= 0 && y < _pieceViews.GetLength(1))
            {
                return _pieceViews[x, y];
            }
            return null;
        }
    }
}
