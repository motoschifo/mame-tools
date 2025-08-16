#nullable enable
using System;
using System.Collections.Generic;

namespace MameTools.Net48.Machines.Inputs;

public partial class Control
{
    public enum ControlWays
    {
        unknown,
        oneWay,
        twoWaysHorizontal,
        twoWaysVertical,
        twoWaysStrange,
        threeWays,
        treeHaldFourWay,
        fourWays,
        fiveWays,
        fiveHalfEightWay,
        eightWays,
        sixteenWays
    }

    private static readonly Dictionary<string, ControlWays> _map = new(StringComparer.OrdinalIgnoreCase)
    {
        { "1", ControlWays.oneWay },
        { "2", ControlWays.twoWaysHorizontal },
        { "vertical2", ControlWays.twoWaysVertical },
        { "strange2", ControlWays.twoWaysStrange },
        { "3", ControlWays.threeWays },
        { "3 (half4)", ControlWays.treeHaldFourWay },
        { "4", ControlWays.fourWays },
        { "5 (half8)", ControlWays.fiveHalfEightWay },
        { "8", ControlWays.eightWays },
        { "16", ControlWays.sixteenWays }
    };

    public static ControlWays ParseControlWays(string? value, ControlWays fallbackValue, ControlWays defaultValue = default!)
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;
        return _map.TryGetValue(value!.Trim(), out var result)
            ? result
            : fallbackValue;
    }
}