#nullable enable
namespace MameTools.Net48.Machines.Inputs;

public partial class Control
{
    public enum ControlTypes
    {
        unknown,
        dial,
        doublejoy,  // From release 0.101
        gambling,
        hanafuda,
        joy,
        keyboard,
        keypad,
        lightgun,
        mahjong,
        mouse,
        only_buttons,
        paddle,
        pedal,
        positional,
        stick,
        trackball,
        triplejoy,
    }
}
