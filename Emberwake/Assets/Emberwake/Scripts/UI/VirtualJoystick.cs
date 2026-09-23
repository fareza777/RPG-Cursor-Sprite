using UnityEngine;
using UnityEngine.EventSystems;

namespace Emberwake
{
    /// <summary>
    /// Fixed left pad — analog move with light 8-way assist (less sticky than hard snap).
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] RectTransform background;
        [SerializeField] RectTransform handle;
        [SerializeField] float handleRange = 100f;
        [SerializeField] float deadZone = 0.1f;

        CanvasGroup group;
        bool active;
        Vector2 current;
        Vector2 smooth;

        public void Setup(RectTransform bg, RectTransform handleRt, float range = 100f)
        {
            background = bg;
            handle = handleRt;
            handleRange = range;
            group = background.GetComponent<CanvasGroup>();
            if (group == null) group = background.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0.95f;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            active = true;
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (background == null) return;
            active = true;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, eventData.pressEventCamera, out Vector2 local);
            Vector2 clamped = Vector2.ClampMagnitude(local, handleRange);
            if (handle != null) handle.anchoredPosition = clamped;

            Vector2 dir = clamped / handleRange;
            if (dir.magnitude < deadZone)
            {
                dir = Vector2.zero;
                if (handle != null) handle.anchoredPosition = Vector2.zero;
            }
            else
            {
                // Light 8-way assist — mostly analog, slight cardinal bias
                Vector2 snapped = Snap8(dir.normalized);
                float bias = Mathf.Lerp(0.2f, 0.4f, dir.magnitude);
                dir = Vector2.Lerp(dir.normalized, snapped, bias) * Mathf.Clamp01(dir.magnitude);
                if (handle != null)
                    handle.anchoredPosition = dir * handleRange;
            }

            // Smooth stick so movement doesn't stutter
            smooth = Vector2.Lerp(smooth, dir, 0.55f);
            current = smooth;
            GameInput.SetMobileMove(current);
            GameInput.SetRunHeld(current.magnitude > 0.78f);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            active = false;
            current = Vector2.zero;
            smooth = Vector2.zero;
            if (handle != null) handle.anchoredPosition = Vector2.zero;
            GameInput.SetMobileMove(Vector2.zero);
            GameInput.SetRunHeld(false);
        }

        void OnDisable()
        {
            if (active) OnPointerUp(null);
        }

        static Vector2 Snap8(Vector2 v)
        {
            float ang = Mathf.Atan2(v.y, v.x);
            float step = Mathf.PI * 0.25f;
            float snapped = Mathf.Round(ang / step) * step;
            return new Vector2(Mathf.Cos(snapped), Mathf.Sin(snapped));
        }
    }
}
