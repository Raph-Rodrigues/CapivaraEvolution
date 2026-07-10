using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
  public Vector2 PointerPosition
  {
    get
    {
      if (Mouse.current != null)
      {
        return Mouse.current.position.ReadValue();
      }
      return Vector2.zero;
    }
  }

  public bool IsPointerPressed
  {
    get
    {
      if (Mouse.current == null)
      {
        Debug.LogError("ALERTA: O Mouse não foi detectado! Verifique em Edit > Project Settings > Player > Active Input Handling e garanta que está como 'Input System Package (New)' ou 'Both'.");
        return false;
      }
      return Mouse.current.leftButton.isPressed;
    }
  }

  public bool WasPointerReleasedThisFrame
  {
    get
    {
      if (Mouse.current != null)
      {
        return Mouse.current.leftButton.wasReleasedThisFrame;
      }
      return false;
    }
  }
}
