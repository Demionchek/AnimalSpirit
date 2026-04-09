using Features.Openers.Domain;
using UnityEngine;
using VContainer;

namespace Features.Openers.Presentation
{
    public abstract class OpenerView : MonoBehaviour
    {
        public OpenerModel Model { get; private set; }

        [Inject]
        public void Construct(OpenerModel model)
        {
            Model = model;
        }
    }
}