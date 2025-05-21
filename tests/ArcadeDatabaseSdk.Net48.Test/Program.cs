#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using ArcadeDatabaseSdk.Net48.Services;
using static ArcadeDatabaseSdk.Net48.Common.ApiResponse;
using static ArcadeDatabaseSdk.Net48.Services.Categories.ClassificationsApiResult;

namespace ArcadeDatabaseSdk.Net48.Test;

internal class Program
{
    static void Main(string[] args)
    {
        // fix async task in net48
        RunAsync().GetAwaiter().GetResult();
    }

    static async Task RunAsync()
    {
        var adbGenres = await ClassificationsService.Get(classificationType: ClassificationType.Genre, language: LanguageKind.en);
        Console.WriteLine($"Found {adbGenres.Data.Count} genres");
        foreach (var genre in adbGenres.Data.OrderBy(x => x.Title))
        {
            Console.WriteLine($"- {genre.Title}");
        }

        var currentLanguage = GetCurrentLanguage();
        if (currentLanguage != LanguageKind.en)
        {
            adbGenres = await ClassificationsService.Get(classificationType: ClassificationType.Genre, language: currentLanguage);
            Console.WriteLine();
            Console.WriteLine($"Translated to {currentLanguage}");
            foreach (var genre in adbGenres.Data.OrderBy(x => x.Title))
            {
                Console.WriteLine($"- {genre.Title}");
            }
        }
    }
}
