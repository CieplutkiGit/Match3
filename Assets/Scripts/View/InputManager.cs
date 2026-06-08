using System;
using UnityEngine;

namespace Match3.View
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private float _minSwipeDistance = 50f;

        private PieceView _selectedPiece;
        private Vector3 _startMousePos;

        public event Action<int, int, int, int> OnSwapRequested;

        private void Awake()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                ProcessSelection();
            }
            else if (Input.GetMouseButton(0) && _selectedPiece != null)
            {
                ProcessSwipe();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _selectedPiece = null;
            }
        }

        private void ProcessSelection()
        {
            Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

            if (hit.collider != null)
            {
                PieceView piece = hit.collider.GetComponent<PieceView>();
                if (piece != null)
                {
                    _selectedPiece = piece;
                    _startMousePos = Input.mousePosition;
                }
            }
        }

        private void ProcessSwipe()
        {
            Vector3 swipeVector = Input.mousePosition - _startMousePos;
            if (swipeVector.magnitude > _minSwipeDistance)
            {
                int x1 = _selectedPiece.X;
                int y1 = _selectedPiece.Y;
                int x2 = x1;
                int y2 = y1;

                if (Mathf.Abs(swipeVector.x) > Mathf.Abs(swipeVector.y))
                {
                    if (swipeVector.x > 0)
                    {
                        x2 = x1 + 1;
                    }
                    else
                    {
                        x2 = x1 - 1;
                    }
                }
                else
                {
                    if (swipeVector.y > 0)
                    {
                        y2 = y1 + 1;
                    }
                    else
                    {
                        y2 = y1 - 1;
                    }
                }

                _selectedPiece = null;
                OnSwapRequested?.Invoke(x1, y1, x2, y2);
            }
        }
    }
}
