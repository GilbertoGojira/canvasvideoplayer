var WebGLEventListener = {
  $JsCall: {},
  AddListener_: function(eventName, callbackID, callback) {
    JsCall[callbackID] = {
      object : callback,
      event : Pointer_stringify(eventName)
    };
    document[JsCall[callbackID].event] = function() {
      document[JsCall[callbackID].event] = null;
      Runtime.dynCall('vi', JsCall[callbackID].object, [callbackID]);
      delete JsCall[callbackID];
    }
  }
};

autoAddDeps(LibraryManager.library, '$JsCall');
mergeInto(LibraryManager.library, WebGLEventListener);
