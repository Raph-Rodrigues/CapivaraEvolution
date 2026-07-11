using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
  public Vector2 PointerPosition
  {
    get
    {
      if (Pointer.current != null)
      {
        return Pointer.current.position.ReadValue();
      }
      return Vector2.zero;
    }
  }

  public bool IsPointerPressed
  {
    get
    {
      if (Pointer.current == null)
      {
        Debug.LogError("ALERTA: O Mouse não foi detectado! Verifique em Edit > Project Settings > Player > Active Input Handling e garanta que está como 'Input System Package (New)' ou 'Both'.");
        return false;
      }
      return Pointer.current.press.isPressed;
    }
  }

  public bool WasPointerReleasedThisFrame
  {
    get
    {
      if (Pointer.current != null)
      {
        return Pointer.current.press.wasReleasedThisFrame;
      }
      return false;
    }
  }
}
