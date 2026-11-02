using UnityEngine;

namespace RooftopRunner
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class Runner : MonoBehaviour
    {
        [Min(1)] public float speed = 6.5f;
        [Min(1)] public float minimumJump = 5.5f;
        [Min(1)] public float maximumJump = 12f;
        [Min(0.1f)] public float chargeSeconds = 0.65f;
        [Min(1)] public float gravity = 24f;
        public LayerMask roofMask = 1 << 6;
        public bool Grounded { get; private set; }
        public bool MissedRoof { get; private set; }
        public bool Charging { get; private set; }
        public float Charge => Charging ? Mathf.Clamp01(held / chargeSeconds) : 0;
        public bool Active { get; set; }
        Rigidbody2D body;
        BoxCollider2D shape;
        PhysicsMaterial2D frictionless;
        public event System.Action Jumped;
        float held;
        bool jumpQueued;
        float queuedStrength;
        readonly RaycastHit2D[] hits = new RaycastHit2D[4];

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            shape = GetComponent<BoxCollider2D>();
            // Constant forward velocity must not generate enough wall friction to suspend a fall.
            frictionless = new PhysicsMaterial2D("Runner frictionless") { friction = 0, bounciness = 0 };
            shape.sharedMaterial = frictionless;
            body.gravityScale = gravity / Mathf.Abs(Physics2D.gravity.y);
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
        void Update()
        {
            if (Active && Charging)
            {
                if (!Grounded) CancelCharge();
                else held = Mathf.Min(held + Time.deltaTime, chargeSeconds);
            }
        }
        void FixedUpdate()
        {
            if (!Active) return;
            var filter = new ContactFilter2D { useLayerMask = true, layerMask = roofMask, useTriggers = false };
            int count = shape.Cast(Vector2.down, filter, hits, 0.07f);
            Grounded = false;
            for (int i = 0; i < count; i++)
                if (hits[i].normal.y > 0.65f && body.linearVelocity.y <= 0.1f) Grounded = true;
            var velocity = body.linearVelocity;
            velocity.x = speed;
            if (jumpQueued)
            {
                if (Grounded) { velocity.y = queuedStrength; Grounded = false; Jumped?.Invoke(); }
                jumpQueued = false;
            }
            body.linearVelocity = velocity;
            if (!Grounded && Charging) CancelCharge();
        }
        public void BeginCharge()
        {
            if (!Active || !Grounded || Charging || jumpQueued) return;
            held = 0; Charging = true;
        }
        public void ReleaseCharge()
        {
            if (!Active || !Charging) return;
            queuedStrength = Mathf.Lerp(minimumJump, maximumJump, Charge);
            jumpQueued = Grounded;
            Charging = false; held = 0;
        }
        public void CancelCharge() { Charging = false; held = 0; jumpQueued = false; }
        void OnCollisionEnter2D(Collision2D collision) { CheckMissedRoof(collision); }
        void OnCollisionStay2D(Collision2D collision) { CheckMissedRoof(collision); }
        void CheckMissedRoof(Collision2D collision)
        {
            if (!Active || (roofMask.value & (1 << collision.gameObject.layer)) == 0) return;
            // A frontal wall hit with the feet below its roof is a missed landing.
            // Upward-facing roof contacts remain valid landings.
            if (shape.bounds.min.y >= collision.collider.bounds.max.y - 0.08f) return;
            for (int i = 0; i < collision.contactCount; i++)
            {
                if (collision.GetContact(i).normal.x >= -0.65f) continue;
                MissedRoof = true; CancelCharge(); return;
            }
        }
        public void ResetRunner(Vector2 position)
        {
            Active = false; CancelCharge(); Grounded = false; MissedRoof = false;
            body.simulated = true; body.position = position; body.linearVelocity = Vector2.zero;
            transform.position = position;
        }
        public void Freeze() { Active = false; CancelCharge(); body.linearVelocity = Vector2.zero; body.simulated = false; }
        void OnDestroy() { if (frictionless) Destroy(frictionless); }
        void OnValidate()
        {
            maximumJump = Mathf.Max(minimumJump, maximumJump);
            gravity = Mathf.Max(1, gravity); chargeSeconds = Mathf.Max(0.1f, chargeSeconds);
        }
    }
}
