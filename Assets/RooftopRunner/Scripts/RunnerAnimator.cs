using UnityEngine;
namespace RooftopRunner
{
    [RequireComponent(typeof(Runner))]
    public sealed class RunnerAnimator : MonoBehaviour
    {
        public SunsetArt art;
        public SpriteRenderer illustration;
        [Min(1)] public float runFramesPerSecond = 11;
        [Min(0.5f)] public float standingHeight = 1.7f;
        Runner runner;
        Rigidbody2D body;
        float cycle, landing;
        bool wasGrounded;
        Vector3 originalScale;
        void Start()
        {
            runner = GetComponent<Runner>(); body = GetComponent<Rigidbody2D>();
            if (!art || art.Frames.Length < 8) { enabled = false; return; }
            float scale = standingHeight / art.Frames[0].bounds.size.y;
            originalScale = Vector3.one * scale;
            illustration.transform.localScale = originalScale;
        }
        void LateUpdate()
        {
            if (Time.timeScale == 0) return;
            if (runner.Grounded && !wasGrounded) landing = 0.1f;
            wasGrounded = runner.Grounded; landing = Mathf.Max(0, landing - Time.deltaTime);
            int frame;
            if (!runner.Active) frame = 0;
            else if (!runner.Grounded) frame = body.linearVelocity.y > 0.2f ? 5 : 6;
            else if (runner.Charging) frame = 4;
            else if (landing > 0) frame = 7;
            else { cycle += Time.deltaTime * runFramesPerSecond; frame = (int)cycle % 4; }
            illustration.sprite = art.Frames[frame];
            illustration.transform.localScale = originalScale;
            illustration.color = runner.Charging ? Color.Lerp(Color.white, new Color(1, 0.86f, 0.6f), runner.Charge * 0.3f) : Color.white;
        }
    }
}
