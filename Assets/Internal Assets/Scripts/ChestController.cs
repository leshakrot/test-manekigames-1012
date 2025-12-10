using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System;
using Game.Core;

namespace Game.Core.Puzzle
{
    public class ChestController : MonoBehaviour
    {
        private bool _isOpened;
        private Camera _mainCamera;
        
        public event Action OnOpened;
        
        private void Start()
        {
            _mainCamera = Camera.main;
        }
        
        private void Update()
        {
            if (_isOpened || _mainCamera == null) return;
            if (PauseSystem.Instance != null && PauseSystem.Instance.IsPaused) return;
            
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                
                if (!IsPointerOverUI(mousePos))
                {
                    CheckClickOnChest(mousePos);
                }
            }
            else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                Vector2 touchPos = Touchscreen.current.primaryTouch.position.ReadValue();
                
                if (!IsPointerOverUI(touchPos))
                {
                    CheckClickOnChest(touchPos);
                }
            }
        }
        
        private bool IsPointerOverUI(Vector2 screenPosition)
        {
            if (EventSystem.current == null) return false;
            
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = screenPosition;
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            return results.Count > 0;
        }
        
        private void CheckClickOnChest(Vector2 screenPosition)
        {
            Ray ray = _mainCamera.ScreenPointToRay(screenPosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {               
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    OpenChest();
                }
            }
        }
        
        private void OpenChest()
        {
            _isOpened = true;
            
            if (OnOpened != null)
            {
                OnOpened.Invoke();
            }
        }
    }
}