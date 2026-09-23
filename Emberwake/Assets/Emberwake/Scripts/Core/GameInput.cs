using UnityEngine;

namespace Emberwake
{
    /// <summary>
    /// Shared input for keyboard (editor) + virtual portrait controls (Play Store).
    /// </summary>
    public static class GameInput
    {
        static Vector2 mobileMove;
        static bool attackPressed;
        static bool spinPressed;
        static bool runHeld;

        public static void SetMobileMove(Vector2 v) => mobileMove = Vector2.ClampMagnitude(v, 1f);
        public static void SetRunHeld(bool held) => runHeld = held;
        public static void PressAttack() => attackPressed = true;
        public static void PressSpin() => spinPressed = true;

        public static Vector2 Move
        {
            get
            {
                Vector2 k = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                if (k.sqrMagnitude > 0.01f) return Vector2.ClampMagnitude(k, 1f);
                return mobileMove;
            }
        }

        public static bool RunHeld =>
            runHeld || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        public static bool ConsumeAttack()
        {
            bool k = Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Z);
            // Avoid LMB when touching UI on mobile — keyboard/editor only for mouse
            if (!Application.isMobilePlatform && Input.GetMouseButtonDown(0) && mobileMove.sqrMagnitude < 0.01f)
                k = true;
            bool m = attackPressed;
            attackPressed = false;
            return k || m;
        }

        public static bool ConsumeSpin()
        {
            bool k = Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.X);
            bool m = spinPressed;
            spinPressed = false;
            return k || m;
        }
    }
}
