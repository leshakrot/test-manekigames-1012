using UnityEngine;

namespace Game.Core.Combat
{
    public class AttackEffect : MonoBehaviour
    {
        [SerializeField] private Transform _visualTransform;
        [SerializeField] private float _scaleUpDuration = 0.1f;
        [SerializeField] private float _scaleDownDuration = 0.2f;
        [SerializeField] private float _scaleMultiplier = 1.3f;
        [SerializeField] private AnimationCurve _scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        private Vector3 _originalScale;
        private float _attackTimer;
        private bool _isAttacking;
        
        
        private void Start()
        {
            if (_visualTransform != null)
            {
                _originalScale = _visualTransform.localScale;
            }
        }
        
        public void PlayAttackAnimation()
        {
            if (_visualTransform == null) return;
            
            _isAttacking = true;
            _attackTimer = 0f;
        }
        
        private void Update()
        {
            if (!_isAttacking || _visualTransform == null) return;
            
            _attackTimer += Time.deltaTime;
            
            float totalDuration = _scaleUpDuration + _scaleDownDuration;
            
            if (_attackTimer <= _scaleUpDuration)
            {
                float t = _scaleCurve.Evaluate(_attackTimer / _scaleUpDuration);
                _visualTransform.localScale = Vector3.Lerp(_originalScale, _originalScale * _scaleMultiplier, t);
            }
            else if (_attackTimer <= totalDuration)
            {
                float t = _scaleCurve.Evaluate((_attackTimer - _scaleUpDuration) / _scaleDownDuration);
                _visualTransform.localScale = Vector3.Lerp(_originalScale * _scaleMultiplier, _originalScale, t);
            }
            else
            {
                _isAttacking = false;
                _visualTransform.localScale = _originalScale;
            }
        }
    }
}
