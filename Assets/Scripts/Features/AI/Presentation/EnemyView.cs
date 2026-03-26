using UnityEngine;

namespace Features.AI.Presentation
{
    public sealed class EnemyView : MonoBehaviour
    {
        [SerializeField] private Transform shootPoint;
        [SerializeField] private GameObject fireGO;

        public GameObject FireGO => fireGO;
        public Transform ShootPoint => shootPoint;
    }
}
