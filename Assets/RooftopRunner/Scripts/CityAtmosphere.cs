using UnityEngine;
namespace RooftopRunner
{
    // One shared shader parameter keeps recycled roofs and both city layers in sync.
    public sealed class CityAtmosphere : MonoBehaviour
    {
        public GameController game;
        [Min(0)] public float sunsetUntilDistance = 100;
        [Min(1)] public float fullNightDistance = 800;
        public float NightAmount { get; private set; }
        static readonly int NightProperty = Shader.PropertyToID("_RunnerNight");
        void LateUpdate() { ApplyDistance(game.Distance); }
        public void ApplyDistance(float distance)
        {
            float progress = Mathf.InverseLerp(sunsetUntilDistance, Mathf.Max(sunsetUntilDistance + 1, fullNightDistance), distance);
            NightAmount = Mathf.SmoothStep(0, 1, progress);
            Shader.SetGlobalFloat(NightProperty, NightAmount);
        }
        public void ResetSunset() { ApplyDistance(0); }
        void OnDisable() { Shader.SetGlobalFloat(NightProperty, 0); }
        void OnValidate() { fullNightDistance = Mathf.Max(sunsetUntilDistance + 1, fullNightDistance); }
    }
}
