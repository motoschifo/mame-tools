#nullable enable
namespace ArcadeDatabaseSdk.Net48.Common;

public class Constants
{
    public const string HomePage = "https://adb.arcadeitalia.net";
    public const string ApiUrl = "https://adb.arcadeitalia.net/api";
    public const string BaseApiUrl = $"{ApiUrl}";
    public const string MameApiUrl = $"{ApiUrl}/mame";
    public const string ClassificationsApiUrl = $"{ApiUrl}/classifications";
    public const string EmulatorsApiUrl = $"{ApiUrl}/emulators";
    
    public const string LegacyApiUrl = "https://adb.arcadeitalia.net";
    public const string LegacyAjaxParameter = "ajax";
    public const string LegacyServiceScraperUrl = $"{LegacyApiUrl}/service_scraper.php";
    public const string LegacyServiceDownloadUrl = $"{LegacyApiUrl}/download_file.php";
}
