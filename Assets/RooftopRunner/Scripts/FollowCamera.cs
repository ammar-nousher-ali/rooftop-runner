using UnityEngine;
namespace RooftopRunner
{
    public sealed class FollowCamera : MonoBehaviour
    {
        public Transform target;
        public Vector2 offset = new Vector2(5, 2.3f);
        [Min(0.01f)] public float smoothTime = 0.18f;
        Vector3 velocity;
        Vector3 Goal => new Vector3(target.position.x + offset.x, Mathf.Clamp(target.position.y * 0.3f, -0.5f, 2.5f) + offset.y, -10);
        public void Snap() { velocity = Vector3.zero; transform.position = Goal; }
        void LateUpdate() { if (target) transform.position = Vector3.SmoothDamp(transform.position, Goal, ref velocity, smoothTime); }
    }
}
