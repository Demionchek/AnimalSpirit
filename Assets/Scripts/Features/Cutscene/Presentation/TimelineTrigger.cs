using Features.Cutscene.Application;
using UnityEngine;
using VContainer;

namespace Features.Cutscene.Presentation
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class TimelineTrigger : MonoBehaviour
    {
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private int _cutsceneIndex;

        [Inject]
        private CutsceneFacade _facade;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if ((_layerMask.value & (1 << collision.gameObject.layer)) == 0)
                return;

            _facade.Play(_cutsceneIndex);
            GetComponent<Collider2D>().enabled = false;
        }
    }
}