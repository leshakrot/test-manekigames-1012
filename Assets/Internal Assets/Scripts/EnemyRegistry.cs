using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.Combat
{
    public class EnemyRegistry : Singleton<EnemyRegistry>
    {
        private readonly HashSet<EnemyController> _activeEnemies = new HashSet<EnemyController>();
        
        public IReadOnlyCollection<EnemyController> ActiveEnemies => _activeEnemies;
        public int EnemyCount => _activeEnemies.Count;
        
        public void RegisterEnemy(EnemyController enemy)
        {
            if (enemy != null)
            {
                _activeEnemies.Add(enemy);
            }
        }
        
        public void UnregisterEnemy(EnemyController enemy)
        {
            if (enemy != null)
            {
                _activeEnemies.Remove(enemy);
            }
        }
        
        public EnemyController FindClosestEnemy(Vector3 position, float maxDistance = float.MaxValue)
        {
            EnemyController closestEnemy = null;
            float closestDistanceSqr = maxDistance * maxDistance;
            
            foreach (var enemy in _activeEnemies)
            {
                if (enemy != null && enemy.IsAlive)
                {
                    float distanceSqr = (enemy.transform.position - position).sqrMagnitude;
                    if (distanceSqr < closestDistanceSqr)
                    {
                        closestDistanceSqr = distanceSqr;
                        closestEnemy = enemy;
                    }
                }
            }
            
            return closestEnemy;
        }
        
        public List<EnemyController> FindEnemiesInRange(Vector3 position, float range)
        {
            var enemiesInRange = new List<EnemyController>();
            float rangeSqr = range * range;
            
            foreach (var enemy in _activeEnemies)
            {
                if (enemy != null && enemy.IsAlive)
                {
                    float distanceSqr = (enemy.transform.position - position).sqrMagnitude;
                    if (distanceSqr <= rangeSqr)
                    {
                        enemiesInRange.Add(enemy);
                    }
                }
            }
            
            return enemiesInRange;
        }
        
        public void ClearAllEnemies()
        {
            _activeEnemies.Clear();
        }
    }
}