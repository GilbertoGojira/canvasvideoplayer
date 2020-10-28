#if UNITY_WEBGL && !UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
#endif

using System;

namespace CanvasVideoPlayer.DamacServices {
  /// <summary>
  /// Allows for listening for events from html such as mouse events
  /// </summary>
  public static class WebGLEventListener {
    public const string ONCLICK = "onclick";
    public const string ONMOUSEUP = "onmouseup";
    public const string ONMOUSEDOWN = "onmousedown";
    public const string ONTOUCHEND = "touchend";

#if UNITY_WEBGL && !UNITY_EDITOR
    static int LastActionID;
    static Dictionary<int, Action> ActionMap = new Dictionary<int, Action>();

    [DllImport("__Internal")]
    static extern void AddListener_(string eventName, int callbackID, Action<int> callback);

    public static void AddListener(string eventName, Action callback) {
      LastActionID = (int)Mathf.Repeat(++LastActionID, 10);
      ActionMap.Add(LastActionID, callback);
      AddListener_(eventName, LastActionID, Callback);
    }

    [MonoPInvokeCallback(typeof(Action<int>))]
    static void Callback(int key) {
      ActionMap[key].Invoke();
      ActionMap.Remove(key);
    }
#else
    public static void AddListener(string eventName, Action callback) =>
     callback.Invoke();
#endif
  }
}