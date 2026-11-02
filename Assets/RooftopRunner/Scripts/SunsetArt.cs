using UnityEngine;

namespace RooftopRunner
{
    [CreateAssetMenu(menuName = "Rooftop Runner/Sunset Art")]
    public sealed class SunsetArt : ScriptableObject
    {
        public Sprite sky, skyline;
        public Material nightSkyMaterial, nightCityMaterial, nightPropMaterial;
        public Texture2D facadeSheet, propSheet, runnerSheet;
        public Rect[] facadeRects, propRects, runnerRects;
        Sprite[] facades, props, frames;
        public Sprite[] Facades => facades ?? (facades = Slice(facadeSheet, facadeRects, new Vector2(0.5f, 1)));
        public Sprite[] Props => props ?? (props = Slice(propSheet, propRects, new Vector2(0.5f, 0)));
        public Sprite[] Frames => frames ?? (frames = Slice(runnerSheet, runnerRects, new Vector2(0.6f, 0)));
        static Sprite[] Slice(Texture2D texture, Rect[] rects, Vector2 pivot)
        {
            if (!texture || rects == null) return new Sprite[0];
            var result = new Sprite[rects.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = Sprite.Create(texture, rects[i], pivot, 100, 0, SpriteMeshType.FullRect);
            return result;
        }
        void OnDisable()
        {
            Release(facades); Release(props); Release(frames);
            facades = props = frames = null;
        }
        static void Release(Sprite[] sprites)
        {
            if (sprites == null) return;
            foreach (var sprite in sprites) if (sprite) Destroy(sprite);
        }
    }
}
