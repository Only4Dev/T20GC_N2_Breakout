using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;

[DisplayStringFormat("Screen Half")]
public class ScreenHalfComposite : InputBindingComposite<float>
{
    [InputControl(layout = "Button")]
    public int press;

    [InputControl(layout = "Axis")]
    public int positionX;

    public override float ReadValue(ref InputBindingCompositeContext context)
    {
        bool isPressed = context.ReadValueAsButton(press);

        if (!isPressed)
            return 0f;

        float screenX = context.ReadValue<float>(positionX);
        return screenX < Screen.width * 0.5f ? -1f : 1f;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        InputSystem.RegisterBindingComposite<ScreenHalfComposite>("ScreenHalf");
    }
}