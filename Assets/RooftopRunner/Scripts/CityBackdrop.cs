using UnityEngine;
namespace RooftopRunner
{
    public sealed class CityBackdrop : MonoBehaviour
    {
        public Camera view;
        public SunsetArt art;
        // Retained for compatibility with the original placeholder scene.
        public Sprite square, disc;
        [Range(0, 1)] public float distantParallax = 0.10f;
        [Range(0, 1)] public float nearParallax = 0.24f;
        Transform sky;
        readonly Transform[] strips = new Transform[6];
        readonly float[] widths = new float[2];
        void Start()
        {
            if (!art || !art.sky || !art.skyline) { enabled = false; return; }
            sky = Shape("Painted sunset sky", art.sky, Color.white, -40);
            for (int layer = 0; layer < 2; layer++)
            {
                float height = layer == 0 ? 10f : 8.2f;
                float scale = height / art.skyline.bounds.size.y;
                widths[layer] = art.skyline.bounds.size.x * scale;
                for (int tile = 0; tile < 3; tile++)
                {
                    int i = layer * 3 + tile;
                    strips[i] = Shape(layer == 0 ? "Distant city" : "Near city", art.skyline,
                        layer == 0 ? new Color(1f, 0.81f, 0.77f, 0.7f) : new Color(0.65f, 0.57f, 0.73f), -30 + layer * 10);
                    strips[i].localScale = Vector3.one * scale;
                }
            }
            LateUpdate();
        }
        Transform Shape(string label, Sprite sprite, Color color, int order)
        {
            var go = new GameObject(label); go.transform.SetParent(transform);
            var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sprite;
            if (art.nightSkyMaterial && art.nightCityMaterial) renderer.sharedMaterial = order == -40 ? art.nightSkyMaterial : art.nightCityMaterial;
            renderer.color = color; renderer.sortingOrder = order;
            return go.transform;
        }
        void LateUpdate()
        {
            if (!sky) return;
            float cover = Mathf.Max(view.orthographicSize * 2 / art.sky.bounds.size.y, view.orthographicSize * 2 * view.aspect / art.sky.bounds.size.x);
            sky.localScale = Vector3.one * cover;
            sky.position = new Vector3(view.transform.position.x, view.transform.position.y, 8);
            for (int layer = 0; layer < 2; layer++)
            {
                float width = widths[layer];
                float scroll = view.transform.position.x * (layer == 0 ? distantParallax : nearParallax);
                int segment = Mathf.FloorToInt(scroll / width);
                for (int tile = 0; tile < 3; tile++)
                {
                    int segmentIndex = segment + tile - 1;
                    var strip = strips[layer * 3 + tile];
                    strip.position = new Vector3(view.transform.position.x + segmentIndex * width - scroll, layer == 0 ? 2f : -0.5f, 5);
                    // Alternate mirrored tiles so each scrolling seam joins identical edge pixels.
                    strip.GetComponent<SpriteRenderer>().flipX = (segmentIndex & 1) != 0;
                }
            }
        }
    }
}
