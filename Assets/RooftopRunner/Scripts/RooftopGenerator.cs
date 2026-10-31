using System.Collections.Generic;
using UnityEngine;

namespace RooftopRunner
{
    public sealed class RooftopGenerator : MonoBehaviour
    {
        public GameObject rooftopPrefab;
        public Runner runner;
        [Min(8)] public float minimumWidth = 10;
        [Min(8)] public float maximumWidth = 16;
        [Min(1)] public float minimumGap = 1.8f;
        [Min(1)] public float maximumGap = 3.8f;
        [Range(0, 1.5f)] public float heightChange = 1;
        public float lowestRoof = -1.5f;
        public float highestRoof = 2;
        [Min(30)] public float generateAhead = 65;
        public int seed = 137;
        readonly Queue<GameObject> roofs = new Queue<GameObject>();
        System.Random random;
        float edge, top;
        int roofIndex;
        public int RoofCount => roofs.Count;
        float Range(float a, float b) => Mathf.Lerp(a, b, (float)random.NextDouble());
        public void ResetRoute()
        {
            foreach (var roof in roofs) { roof.SetActive(false); Destroy(roof); }
            roofs.Clear(); roofIndex = 0; random = new System.Random(seed); edge = -15; top = 0;
            AddRoof(30, 0, 0); Extend(0);
        }
        public static float Reach(float speed, float jump, float gravity, float rise)
        {
            float discriminant = jump * jump - 2 * gravity * rise;
            return discriminant <= 0 ? 0 : speed * (jump + Mathf.Sqrt(discriminant)) / gravity;
        }
        void Update() { if (random != null) Extend(runner.transform.position.x); }
        void Extend(float x)
        {
            while (edge < x + generateAhead)
            {
                // Cap rises at 40% of the full jump apex. Reserve travel for an early takeoff
                // and a landing inside the next roof, not merely touching its corner.
                float riseLimit = Mathf.Min(heightChange, runner.maximumJump * runner.maximumJump / (2 * runner.gravity) * 0.4f);
                float nextTop = Mathf.Clamp(top + Range(-heightChange, riseLimit), lowestRoof, highestRoof);
                float reach = Reach(runner.speed, runner.maximumJump, runner.gravity, nextTop - top);
                float safeGap = Mathf.Max(0.25f, reach - 2.1f);
                float gap = Range(Mathf.Min(minimumGap, safeGap), Mathf.Min(maximumGap, safeGap));
                float width = Range(minimumWidth, Mathf.Max(minimumWidth, maximumWidth));
                width = Mathf.Max(width, runner.speed * (runner.chargeSeconds + 0.8f), reach + 2);
                AddRoof(width, gap, nextTop);
            }
            while (roofs.Count > 0 && roofs.Peek().transform.position.x + roofs.Peek().transform.localScale.x / 2 < x - 35)
                Destroy(roofs.Dequeue());
        }
        void AddRoof(float width, float gap, float height)
        {
            const float depth = 18;
            var roof = Instantiate(rooftopPrefab, transform);
            roof.name = "Rooftop";
            roof.transform.position = new Vector3(edge + gap + width / 2, height - depth / 2, 0);
            roof.transform.localScale = new Vector3(width, depth, 1);
            var dress = roof.GetComponent<RooftopDress>();
            if (dress) dress.Build(width, roofIndex % 3, seed + roofIndex * 17);
            roofIndex++;
            roofs.Enqueue(roof); edge += gap + width; top = height;
        }
    }
}
