using UnityEngine;

namespace Emberwake
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] float smooth = 7f;
        [SerializeField] float lookAhead = 0.85f;
        [SerializeField] Vector3 offset = new Vector3(0f, 0.35f, -10f);
        [SerializeField] bool lockToRoom;
        [SerializeField] Vector2 roomMin;
        [SerializeField] Vector2 roomMax;

        PlayerController controller;
        Vector2 look;

        public void SetTarget(Transform t)
        {
            target = t;
            controller = t != null ? t.GetComponent<PlayerController>() : null;
        }

        public void SetRoomLock(bool enabled, Vector2 min, Vector2 max)
        {
            lockToRoom = enabled;
            roomMin = min;
            roomMax = max;
        }

        void LateUpdate()
        {
            if (target == null) return;

            if (controller != null && controller.IsMoving)
                look = Vector2.Lerp(look, controller.MoveVector * lookAhead, 1f - Mathf.Exp(-6f * Time.deltaTime));
            else
                look = Vector2.Lerp(look, Vector2.zero, 1f - Mathf.Exp(-4f * Time.deltaTime));

            Vector3 desired = target.position + offset + (Vector3)look;
            if (lockToRoom)
            {
                if (roomMin.x <= roomMax.x)
                    desired.x = Mathf.Clamp(desired.x, roomMin.x, roomMax.x);
                if (roomMin.y <= roomMax.y)
                    desired.y = Mathf.Clamp(desired.y, roomMin.y, roomMax.y);
            }
            transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-smooth * Time.deltaTime));
        }
    }
}
