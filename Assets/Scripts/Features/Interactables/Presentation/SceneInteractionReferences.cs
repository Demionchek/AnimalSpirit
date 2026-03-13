using UnityEngine;

namespace Features.Interactables.Presentation
{
    public sealed class SceneInteractionReferences : MonoBehaviour
    {
        public DoorView doorToOpen;
        public bool manualDoorOpen;
        public GameObject objectToActivate;
        public AudioSource audioSource;
        public AudioClip audioClip;
    }
}