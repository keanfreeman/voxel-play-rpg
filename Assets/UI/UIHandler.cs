using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class UIHandler : MonoBehaviour
{
    public abstract void SetDisplayState(bool isVisible);
    public abstract void HandleNavigate(InputAction.CallbackContext obj);
    public abstract void HandleCancelNavigate(InputAction.CallbackContext obj);
    public abstract void HandleSubmit(InputAction.CallbackContext obj);
    public abstract void HandleCancel(InputAction.CallbackContext obj);
}
