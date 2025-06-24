using UnityEngine;

namespace DefaultNamespace
{
    public class CheckPoints : MonoBehaviour
    {
        [SerializeField] private Transform[] _checkPoints;

        public Transform CurrentCheckPoint { get; private set; }
        private void Awake()
        {
            CurrentCheckPoint = _checkPoints[0];
        }

        public void SetCurrentCheckpoint(int index) => CurrentCheckPoint = _checkPoints[index];
    }
}