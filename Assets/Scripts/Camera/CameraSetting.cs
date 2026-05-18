using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSetting : MonoBehaviour
{
    public InputAction click;

    void Awake()
    {
        click = InputSystem.actions.FindAction("Player/Attack");
    }
    void Update()
    {
        dragSCene();
    }
    void dragSCene()
    {
        if(click.IsPressed())
        {
          transform.LookAt(Input.mousePosition);
        }
    }
}
