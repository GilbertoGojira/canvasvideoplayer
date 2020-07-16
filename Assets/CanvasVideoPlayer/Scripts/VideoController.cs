using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

namespace CanvasVideoPlayer.DamacServices {

  [RequireComponent(typeof(VideoPlayer))]
  public class VideoController : MonoBehaviour {
    public Button PlayButton;
    public Button PauseButton;
    public Button ExitButton;
    public RectTransform Knob;
    public RectTransform ProgressBar;
    public RectTransform ProgressBarBackground;
    public TMP_Text MessagePlaceHoholder;
    public GameObject LoadingObject;

    public float MaxProgress =>
      Vector2.Scale(ProgressBarBackground.rect.size, ProgressBarBackground.localScale).x;

    public float MinProgress =>
      -Knob.sizeDelta.x * .5f;

    public float NormalizedKnobValue =>
      Knob.anchoredPosition.x / MaxProgress;

    public float Progress {
      get => MaxProgress * NormalizedProgress;
      set => Frame = (long)(m_videoPlayer.frameCount * value / MaxProgress);
    }

    public float NormalizedProgress {
      get => Frame > 0 ? Frame / (float)m_videoPlayer.frameCount : float.Epsilon;
      set => Frame = (long)(m_videoPlayer.frameCount * value);
    }

    long mlastPlayedFrame;
    long m_frame;
    public long Frame {
      get => m_frame;
      private set {
        m_frame = Mathf.Clamp((int)value, 0, (int)m_videoPlayer.frameCount);
        m_videoPlayer.frame = m_frame;
        UpdateProgressBar();
      }
    }

    public string Url {
      get => m_videoPlayer.url;
      set {
        m_videoPlayer.url = value;
        Frame = 0;
        if (AutoPlay)
          m_videoStates.SetState(PLAY_STATE);
      }
    }

    public bool AutoPlay {
      get => m_videoPlayer.playOnAwake;
      set => m_videoPlayer.playOnAwake = value;
    }

    public event Action OnPlay;
    public event Action OnPause;
    public event Action OnExit;
    public event Action OnUpdate;

    const string LOADING_STATE = "LOADING";
    const string PREPARED_STATE = "PREPARED";
    const string PLAY_STATE = "PLAY";
    const string PAUSE_STATE = "PAUSE";
    const string SKIP_STATE = "SKIP";
    const string ERROR_STATE = "ERROR";
    const string EXIT_STATE = "EXIT";

    private VideoPlayer m_videoPlayer;
    private SimpleStateMachine m_videoStates = new SimpleStateMachine();

    private void Awake() {
      m_videoPlayer = GetComponent<VideoPlayer>();
      PlayButton.onClick.AddListener(Play);
      PauseButton.onClick.AddListener(Pause);
      ExitButton.onClick.AddListener(Exit);
      var knobInput = Knob.gameObject
        .AddComponent<ControlInput>();
      knobInput.OnDown = KnobOnDown;
      knobInput.OnUp = KnobOnUp;
      knobInput.OnDrag = KnobOnDrag;

      var progressInput = ProgressBarBackground.gameObject
        .AddComponent<ControlInput>();
      progressInput.OnDown = KnobOnDown;
      progressInput.OnUp = KnobOnUp;
      progressInput.OnDrag = KnobOnDrag;

      m_videoPlayer.prepareCompleted += OnVideoPrepared;
      m_videoPlayer.errorReceived += OnVideoError;
      m_videoPlayer.loopPointReached += OnLoopPointReached;

      m_videoStates.CreateState(
        LOADING_STATE,
        () => {
          m_videoPlayer.targetTexture?.Release();
          UpdateUI();
        });

      m_videoStates.CreateState(
        PREPARED_STATE,
        () => {
          if (AutoPlay)
            Play();
          UpdateUI();
        });
      m_videoStates.CreateState(
        PLAY_STATE,
        () => {
          if (!m_videoPlayer.isPrepared)
            Prepare();
          else {
            OnPlay?.Invoke();
            m_videoPlayer.Play();
            if (!m_videoPlayer.isPlaying)
              Pause();
          }
          UpdateUI();
        });
      m_videoStates.CreateState(
        PAUSE_STATE,
        () => {
          if (!m_videoPlayer.isPrepared)
            Prepare();
          else {
            OnPause?.Invoke();
            m_videoPlayer.Pause();
            UpdateProgressBar();
            UpdateUI();
          }
        });
      m_videoStates.CreateState(
        SKIP_STATE,
        () => {
          m_videoPlayer.Pause();
        });
      m_videoStates.CreateState(
        ERROR_STATE,
        () => UpdateUI());
      m_videoStates.CreateState(
        EXIT_STATE,
        () => {
          OnExit?.Invoke();
          Destroy(gameObject);
        });

      m_videoStates.SetState(LOADING_STATE);
    }

    private void OnLoopPointReached(VideoPlayer source) {
      m_videoStates.SetState(PAUSE_STATE);
    }

    private void OnVideoPrepared(VideoPlayer source) {
      m_videoStates.SetState(PREPARED_STATE);
    }

    private void OnVideoError(VideoPlayer source, string message) {
      m_videoStates.SetState(ERROR_STATE);
      MessagePlaceHoholder?.SetText(message);
      Debug.LogError(message);
    }

    private void OnDestroy() {
      PlayButton.onClick.RemoveListener(Play);
      PauseButton.onClick.RemoveListener(Pause);
      ExitButton.onClick.RemoveListener(Exit);
    }

    private void Update() =>
      OnUpdate?.Invoke();

    void LateUpdate() {
      if (m_videoStates.CurrentState == PLAY_STATE && m_videoPlayer.frame != mlastPlayedFrame) {
        UpdateProgressBar();
        m_frame = m_videoPlayer.frame;
        mlastPlayedFrame = m_videoPlayer.frame;
      }
    }

    void UpdateProgressBar() {
      ProgressBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Progress);
      Knob.anchoredPosition = new Vector2(Progress + MinProgress, Knob.anchoredPosition.y);
    }

    void UpdateUI() {
      PlayButton.gameObject.SetActive(m_videoStates.CurrentState != PLAY_STATE);
      PauseButton.gameObject.SetActive(m_videoStates.CurrentState == PLAY_STATE);
      LoadingObject.SetActive(m_videoStates.CurrentState == LOADING_STATE);
      MessagePlaceHoholder?.gameObject.SetActive(m_videoStates.CurrentState == ERROR_STATE);
      UpdateProgressBar();
    }

    void KnobOnDown(Component sender, Vector2 position) {
      if ((sender.transform as RectTransform) == Knob)
        m_videoStates.SetState(SKIP_STATE);
    }

    void KnobOnUp(Component sender, Vector2 position) {
      if ((sender.transform as RectTransform) == Knob)
        m_videoStates.GoToPreviousState();
    }

    void KnobOnDrag(Component sender, Vector2 position) {
      if (RectTransformUtility.ScreenPointToLocalPointInRectangle(ProgressBarBackground, position, null, out var newKnobPos))
        Progress = newKnobPos.x + ProgressBarBackground.rect.width * .5f;
    }

    public void Prepare() =>
      m_videoStates.SetState(LOADING_STATE);

    public void Pause() =>
      m_videoStates.SetState(PAUSE_STATE);

    public void Play() =>
      m_videoStates.SetState(PLAY_STATE);

    public void Exit() =>
      m_videoStates.SetState(EXIT_STATE);
  }

  #region Input
  internal class ControlInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {

    public Action<Component, Vector2> OnDown;
    public Action<Component, Vector2> OnUp;
    public Action<Component, Vector2> OnDrag;

    bool m_dragging;

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
      OnDown?.Invoke(this, eventData.position);
      m_dragging = true;
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
      OnUp?.Invoke(this, eventData.position);
      m_dragging = false;
    }

    private void Update() {
      if (m_dragging)
        OnDrag?.Invoke(this, Input.mousePosition);
    }

    void IDragHandler.OnDrag(PointerEventData eventData) { }
  }
  #endregion Input

  #region StateMachine
  internal class SimpleStateMachine {

    Dictionary<string, Action> m_states = new Dictionary<string, Action>();
    public string CurrentState {
      get;
      private set;
    }

    public string PreviousState {
      get;
      private set;
    }

    public void CreateState(string state, Action action, bool set = false) {
      m_states.Add(state, action);
      if (set)
        SetState(state);
    }

    public void Dispose() {
      m_states.Clear();
    }

    public void SetState(string state) {
      if (state == CurrentState)
        return;
      PreviousState = CurrentState;
      CurrentState = state;
      m_states[CurrentState].Invoke();
    }

    public void GoToPreviousState() {
      SetState(PreviousState);
    }
  }

  #endregion StateMachine
}
