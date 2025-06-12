using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{

    [RequireComponent(typeof(PlayerInput))]
    public class InputHandler : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool BarkPressed { get; private set; }
        public bool ChangeShapePressed { get; private set; }

        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction barkAction;
        private InputAction changeShapeAction;

        private void Awake()
        {
            var playerInput = GetComponent<PlayerInput>();

            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
            barkAction = playerInput.actions["Bark"];
            changeShapeAction = playerInput.actions["ChangeShape"];
        }

        private void Update()
        {
            MoveInput = moveAction.ReadValue<Vector2>();
        }

        private void LateUpdate()
        {
            // Автоматический сброс триггеров в конце кадра
            JumpPressed = false;
            BarkPressed = false;
            ChangeShapePressed = false;
        }

        private void OnJump(InputValue context)
        {
            if (context.isPressed) JumpPressed = true;
        }

        private void OnBark(InputValue context)
        {
            if (context.isPressed) BarkPressed = true;
        }

        private void OnChangeShape(InputValue context)
        {
            if (context.isPressed) ChangeShapePressed = true;
        }

        // private void OnEnable()
        // {
        //     jumpAction.performed += OnJump;
        //     barkAction.performed += OnBark;
        //     changeShapeAction.performed += OnChangeShape;
        // }
        //
        // private void OnDisable()
        // {
        //     jumpAction.performed -= OnJump;
        //     barkAction.performed -= OnBark;
        //     changeShapeAction.performed -= OnChangeShape;
        // }
    }
}