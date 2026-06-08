using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Match3.View
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private float _minSwipeDistance = 50f;

        private PieceView _selectedPiece;
        private Vector2 _startMousePos;

        public event Action<int, int, int, int> OnSwapRequested;

        private void Awake()
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;
        }

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                ProcessSelection(mouse.position.ReadValue());
            }
            else if (mouse.leftButton.isPressed && _selectedPiece != null)
            {
                ProcessSwipe(mouse.position.ReadValue());
            }
            else if (mouse.leftButton.wasReleasedThisFrame)
            {
                _selectedPiece = null;
            }
        }

        private void ProcessSelection(Vector2 screenPos)
        {
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider != null)
            {
                PieceView piece = hit.collider.GetComponent<PieceView>();
                if (piece != null)
                {
                    _selectedPiece = piece;
                    _startMousePos = screenPos;
                }
            }
        }

        private void ProcessSwipe(Vector2 currentPos)
        {
            Vector2 swipeVector = currentPos - _startMousePos;
            if (swipeVector.magnitude > _minSwipeDistance)
            {
                int x1 = _selectedPiece.X;
                int y1 = _selectedPiece.Y;
                int x2 = x1;
                int y2 = y1;

                if (Mathf.Abs(swipeVector.x) > Mathf.Abs(swipeVector.y))
                    x2 = swipeVector.x > 0 ? x1 + 1 : x1 - 1;
                else
                    y2 = swipeVector.y > 0 ? y1 + 1 : y1 - 1;

                _selectedPiece = null;
                OnSwapRequested?.Invoke(x1, y1, x2, y2);
            }
        }
    }
}
