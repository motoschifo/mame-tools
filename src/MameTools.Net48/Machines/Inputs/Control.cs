#nullable enable
using System.Linq;
using MameTools.Net48.Extensions;

namespace MameTools.Net48.Machines.Inputs;

public partial class Control
{
    public string Type { get; set; } = default!;
    public int Player { get; set; }
    public int Buttons { get; set; }
    public int RequiredButtons { get; set; }
    public int Minimum { get; set; }
    public int Maximum { get; set; }
    public int Sensitivity { get; set; }
    public int KeyDelta { get; set; }
    public bool Reverse { get; set; }
    public int Ways { get; set; }
    /// <summary>
    /// WARNING: Legacy release, up to 0.100
    /// </summary>
    public int Ways2 { get; set; }
    /// <summary>
    /// WARNING: Legacy release, up to 0.100
    /// </summary>
    public int Ways3 { get; set; }

    public static ControlTypes ParseType(string? value) => value.ToEnum(ControlTypes.unknown, ControlTypes.unknown);
    public static ControlWays ParseWays(string? value) => ParseControlWays(value, ControlWays.unknown, ControlWays.unknown);
    public static ControlWays ParseWays2(string? value) => ParseControlWays(value, ControlWays.unknown, ControlWays.unknown);
    public static ControlWays ParseWays3(string? value) => ParseControlWays(value, ControlWays.unknown, ControlWays.unknown);
    public bool IsOfType(ControlTypes type) => ParseType(Type) == type;
    public bool IsOneOfType(params ControlTypes[] types) => types.Contains(ParseType(Type));
    public bool IsJoystick() => IsJoystick(ParseType(Type));

}
