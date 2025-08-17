#nullable enable
using System;
using System.Collections.Generic;

namespace MameTools.Net48.Machines.Inputs;

public partial class Control
{
    public class ControlSpec
    {
        public ControlTypes Type { get; set; } = ControlTypes.unknown;
        public ControlWays? Ways { get; set; }
        public ControlWays? SecondWays { get; set; }
        public ControlWays? ThirdWays { get; set; }
    }

    public static ControlSpec? Parse(string type, string? ways, string? secondWays, string? thirdWays)
    {
        if (!string.IsNullOrWhiteSpace(type))
            return null;

        ControlTypes parsedType;
        ControlWays? parsedWays;

        if (_legacyMap.TryGetValue(type, out var legacy))
        {
            parsedType = legacy.type;
            parsedWays = legacy.ways;
        }
        else
        {
            // Fallback: provo a interpretarlo come "type" normale
            parsedType = ParseType(type);
            parsedWays = ParseWays(ways);
        }

        return new ControlSpec
        {
            Type = parsedType,
            Ways = parsedWays,
            SecondWays = IsJoystick(parsedType) && !string.IsNullOrEmpty(secondWays) ? ParseWays(secondWays) : null,
            ThirdWays = IsJoystick(parsedType) && !string.IsNullOrEmpty(thirdWays) ? ParseWays(thirdWays) : null
        };
    }

    // Mappa legacy: control -> (type, ways)
    private static readonly Dictionary<string, (ControlTypes type, ControlWays? ways)> _legacyMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["dial"] = (ControlTypes.dial, null),
        ["lightgun"] = (ControlTypes.lightgun, null),
        ["paddle"] = (ControlTypes.paddle, null),
        ["stick"] = (ControlTypes.stick, null),
        ["trackball"] = (ControlTypes.trackball, null),

        ["doublejoy4way"] = (ControlTypes.doublejoy, ControlWays.fourWays),
        ["doublejoy8way"] = (ControlTypes.doublejoy, ControlWays.eightWays),
        ["doublejoy2way"] = (ControlTypes.doublejoy, ControlWays.twoWaysHorizontal),
        ["vdoublejoy2way"] = (ControlTypes.doublejoy, ControlWays.twoWaysVertical),

        ["joy2way"] = (ControlTypes.joy, ControlWays.twoWaysHorizontal),
        ["joy4way"] = (ControlTypes.joy, ControlWays.fourWays),
        ["joy8way"] = (ControlTypes.joy, ControlWays.eightWays),
        ["vjoy2way"] = (ControlTypes.joy, ControlWays.twoWaysVertical),
    };


}
