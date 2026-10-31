using UnityEngine;
using UnityEngine.EventSystems;
namespace RooftopRunner
{
    public sealed class JumpButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public GameController game;
        int pointer = int.MinValue;
        public void OnPointerDown(PointerEventData e)
        {
            if (pointer != int.MinValue) return;
            pointer = e.pointerId; game.BeginCharge();
        }
        public void OnPointerUp(PointerEventData e)
        {
            if (e.pointerId != pointer) return;
            pointer = int.MinValue; game.ReleaseCharge();
        }
        public void OnPointerExit(PointerEventData e)
        {
            if (e.pointerId == pointer) { pointer = int.MinValue; game.CancelCharge(); }
        }
        void OnDisable() { pointer = int.MinValue; if (game) game.CancelCharge(); }
    }
}
