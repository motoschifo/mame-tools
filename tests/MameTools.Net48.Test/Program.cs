#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MameTools.Net48.Test;

internal class Program
{
    static void Main(string[] args)
    {
        // fix async task in net48
        RunAsync().GetAwaiter().GetResult();
    }

    static async Task RunAsync()
    {
        var mame = new Mame();
        await Imports.ImportMachines.LoadFromFile(mame, "gamelist.xml", progressUpdate: UpdateInfo);
        Console.WriteLine("Total machines: " + mame.Machines.Count);
        Console.WriteLine("Total parent machines: " + mame.Machines.Count(x => x.IsParentMachine));
        Console.WriteLine("Total working machines: " + mame.Machines.Count(x => x.Driver.Status == Machines.Drivers.Driver.DriverStatusKind.good));
    }

    static private void UpdateInfo(string? info) => Console.WriteLine($"Loading... {info}");
}
