using UnityEngine;

namespace Game.Core
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new Vector3(0, 10, -10);
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private bool _autoFindPlayer = true;
        
        private void Start()
        {
            if (_autoFindPlayer && _target == null)
            {
                FindPlayer();
            }
        }
        
        private void LateUpdate()
        {
            if (_target == null)
            {
                if (_autoFindPlayer)
                {
                    FindPlayer();
                }
                return;
            }
            
            Vector3 desiredPosition = _target.position + _offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
            
            transform.LookAt(_target);
        }
        
        private void FindPlayer()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _target = player.transform;
            }
        }
    }
}