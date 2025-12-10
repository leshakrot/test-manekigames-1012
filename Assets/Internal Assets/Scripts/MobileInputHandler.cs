using UnityEngine;
using Game.Bootstrap;

namespace Game.Core.MobileInput
{
    public class MobileInputHandler : Singleton<MobileInputHandler>
    {
        [SerializeField] private float _minSwapDistance = 10f;       
        private Vector2 _swipeStartPosition;
        private Vector2 _currentSwipeDirection;
        
        public bool IsMobileControlsActive { get; private set; }
        
        public void Initialize(GameStateController gameState)
        {
            IsMobileControlsActive = Application.isMobilePlatform;
        }
        
        private void Update()
        {
            if (IsMobileControlsActive)
            {
                HandleTouchInput();
            }
            else
            {
                HandleMouseInput();
            }
        }
        
        private void HandleTouchInput()
        {
            if (UnityEngine.Input.touchCount == 0)
            {
                _currentSwipeDirection = Vector2.zero;
                return;
            }
            
            Touch touch = UnityEngine.Input.GetTouch(0);
            ProcessInputPhase(touch.phase, touch.position);
        }
        
        private void HandleMouseInput()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                BeginSwipe(UnityEngine.Input.mousePosition);
            }
            else if (UnityEngine.Input.GetMouseButton(0))
            {
                UpdateSwipe(UnityEngine.Input.mousePosition);
            }
            else if (UnityEngine.Input.GetMouseButtonUp(0) || !UnityEngine.Input.GetMouseButton(0))
            {
                EndSwipe();
            }
        }
        
        private void ProcessInputPhase(TouchPhase phase, Vector2 position)
        {
            switch (phase)
            {
                case TouchPhase.Began:
                    BeginSwipe(position);
                    break;
                    
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    UpdateSwipe(position);
                    break;
                    
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    EndSwipe();
                    break;
            }
        }
        
        private void BeginSwipe(Vector2 position)
        {
            _swipeStartPosition = position;
            _currentSwipeDirection = Vector2.zero;
        }
        
        private void UpdateSwipe(Vector2 currentPosition)
        {
            Vector2 swipeDelta = currentPosition - _swipeStartPosition;
            
            if (swipeDelta.magnitude >= _minSwapDistance)
            {
                _currentSwipeDirection = swipeDelta.normalized;
            }
            else
            {
                _currentSwipeDirection = Vector2.zero;
            }
        }
        
        private void EndSwipe()
        {
            _currentSwipeDirection = Vector2.zero;
        }
        
        public Vector2 GetMovementInput()
        {
            return _currentSwipeDirection;
        }
    }
}