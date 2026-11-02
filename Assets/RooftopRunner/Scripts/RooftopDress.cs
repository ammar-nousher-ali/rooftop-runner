using UnityEngine;
namespace RooftopRunner
{
    // Artwork never changes collision geometry. All decoration is visual only.
    public sealed class RooftopDress : MonoBehaviour
    {
        public SunsetArt art;
        public void Build(float width, int variant, int decorationSeed)
        {
            if (!art || art.Facades.Length < 3) return;
            var root = new GameObject("Painted facade and props").transform;
            root.SetParent(transform, false);
            // Parent roofs are scaled collider boxes; cancel that scale for undistorted artwork.
            root.localScale = new Vector3(1 / transform.localScale.x, 1 / transform.localScale.y, 1);
            root.localPosition = new Vector3(0, 0.5f, 0);
            int modules = Mathf.Max(1, Mathf.CeilToInt(width / 4.4f));
            float moduleWidth = width / modules;
            for (int i = 0; i < modules; i++)
            {
                var sprite = art.Facades[variant % art.Facades.Length];
                var visual = Add(root, "Facade", sprite, new Vector2(-width / 2 + moduleWidth * (i + 0.5f), -0.035f), 2);
                visual.transform.localScale = new Vector3((moduleWidth + 0.015f) / sprite.bounds.size.x, 7.5f / sprite.bounds.size.y, 1);
            }
            // Keep the takeoff/landing zone visually clear; props sit behind the runner.
            var random = new System.Random(decorationSeed);
            int count = Mathf.Clamp((int)(width / 3), 2, 6);
            for (int i = 0; i < count && art.Props.Length > 0; i++)
            {
                int index = (variant * 3 + i) % art.Props.Length;
                float x = -width / 2 + 1.3f + (width - 2.6f) * (i + 0.5f) / count;
                var sprite = art.Props[index];
                var visual = Add(root, "Prop " + index, sprite, new Vector2(x, 0.015f), 3);
                float[] heights = { 1.65f, 1.8f, 0.65f, 1.0f, 1.45f, 1.7f, 0.65f, 0.8f };
                visual.transform.localScale = Vector3.one * heights[index % heights.Length] / sprite.bounds.size.y;
                visual.color = Color.Lerp(Color.white, new Color(0.75f, 0.69f, 0.81f), (float)random.NextDouble() * 0.25f);
            }
        }
        SpriteRenderer Add(Transform parent, string name, Sprite sprite, Vector2 position, int order)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = position;
            var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sprite;
            var material = order == 2 ? art.nightCityMaterial : art.nightPropMaterial;
            if (material) renderer.sharedMaterial = material;
            renderer.sortingOrder = order; return renderer;
        }
    }
}
