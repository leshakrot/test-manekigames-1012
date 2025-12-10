using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Game.Core.UI
{
    public class KeyUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image _keyImage;
        
        private Puzzle.KeyColor _keyColor;
        private Puzzle.PuzzleController _puzzle;
        private UIController _uiController;
        private Canvas _canvas;
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Vector3 _originalPosition;
        private Transform _originalParent;
        
        public void Initialize(Puzzle.KeyColor color, Puzzle.PuzzleController puzzle, UIController uiController)
        {
            _keyColor = color;
            _puzzle = puzzle;
            _uiController = uiController;
            
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            _canvas = GetComponentInParent<Canvas>();
            _keyImage.color = _puzzle.GetColorForKey(_keyColor);
            
            _originalPosition = _rectTransform.position;
            _originalParent = transform.parent;
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            _originalPosition = _rectTransform.position;
            _originalParent = transform.parent;
            
            _canvasGroup.alpha = 0.6f;
            _canvasGroup.blocksRaycasts = false;
            
            transform.SetParent(_canvas.transform);
            transform.SetAsLastSibling();
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            
            bool keyPlaced = false;
            
            if (eventData.pointerEnter != null)
            {
                var dropZone = eventData.pointerEnter.GetComponent<LockDropZone>();
                if (dropZone != null)
                {
                    keyPlaced = _puzzle.TryPlaceKey(_keyColor);
                    _uiController.UpdateKeyCounter();
                    
                    if (keyPlaced)
                    {
                        Destroy(gameObject);
                        return;
                    }
                }
            }
            
            transform.SetParent(_originalParent);
            _rectTransform.position = _originalPosition;
        }
    }
}