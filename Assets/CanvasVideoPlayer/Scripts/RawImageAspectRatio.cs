using UnityEngine;
using UnityEngine.UI;

public enum FitType {
  FitHorizontally,
  FitVertically
};

[RequireComponent(typeof(RawImage))]
public class RawImageAspectRatio : MonoBehaviour {

  public RectTransform BaseRect;

  public FitType FitType;

  private RawImage m_rawImage;

  // Start is called before the first frame update
  void Start() {
    m_rawImage = GetComponent<RawImage>();
    var size = CalcSize(BaseRect.rect.size, (float)m_rawImage.texture.width / m_rawImage.texture.height, FitType);
    m_rawImage.rectTransform.SetSizeWithCurrentAnchors(
      RectTransform.Axis.Horizontal, size.x);
    m_rawImage.rectTransform.SetSizeWithCurrentAnchors(
      RectTransform.Axis.Vertical, size.y);
  }

  // Update is called once per frame
  void Update() {

  }

  static Vector2 CalcSize(Vector2 baseSize, float aspectRatio, FitType fitType) {
    switch (fitType) {
      case FitType.FitHorizontally:
        return new Vector2(
          baseSize.x,
          baseSize.x / aspectRatio
        );
      case FitType.FitVertically:
        return new Vector2(
          baseSize.y * aspectRatio,
          baseSize.y
        );
      default:
        throw new System.NotImplementedException();
    }
  }
}
