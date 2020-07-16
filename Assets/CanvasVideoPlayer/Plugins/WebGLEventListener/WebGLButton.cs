using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CanvasVideoPlayer.DamacServices {
  public class WebGLButton : Button {

#if UNITY_WEBGL
    // Sends onClick when pointer up so WebGL can pick native html onclick
    public override void OnPointerDown(PointerEventData eventData) {
      base.OnPointerDown(eventData);
      WebGLEventListener.AddListener(WebGLEventListener.ONCLICK, () => onClick?.Invoke());
    }

    // Blocks old onClick
    public override void OnPointerClick(PointerEventData eventData) { }
#endif
  }
}