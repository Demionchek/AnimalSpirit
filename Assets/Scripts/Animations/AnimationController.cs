using UnityEngine;

namespace Animations
{
    public class AnimationController : MonoBehaviour
    {
        protected Animator animator;
        protected SpriteRenderer spriteRenderer;

        public bool isAttacking = false;

        public static string ATTACK_S = "Attack";
        public static string SPEED_S = "Speed";
        public static string IS_DEAD_S = "isDead";
    }
}