using System;
using Cinemachine;
using UnityEngine;

namespace Camera
{
    public class CameraController : MonoBehaviour
    {

        [SerializeField] private GameObject _curtain;
        private CinemachineVirtualCamera _vcam;
        private UnityEngine.Camera _camera;

        private void Awake()
        {
            _vcam = GetComponentInChildren<CinemachineVirtualCamera>();
            _camera = GetComponent<UnityEngine.Camera>();
        }

        public void SwitchVirtualCamera( bool isActive ) => _vcam.enabled = isActive;

        public void SwitchCurtain( bool isActive ) => _curtain.SetActive(isActive);

        public void SetOrthographicSize( float size ) => _camera.orthographicSize = size;
    }
}