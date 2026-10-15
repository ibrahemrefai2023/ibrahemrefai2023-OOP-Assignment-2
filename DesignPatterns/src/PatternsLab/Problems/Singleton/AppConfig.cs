namespace PatternsLab.Problems.Singleton;

public sealed class AppConfig
{
    private static readonly Lazy<AppConfig> _instance = new Lazy<AppConfig>(() => new AppConfig());

    public static int LoadCount { get; private set; }
    public static AppConfig Instance => _instance.Value;

    public string DbConnection { get; set; }
    public string Theme { get; set; }

    private AppConfig()
    {
        LoadCount++;
        Console.WriteLine($"[AppConfig] Loading settings from disk... (load #{LoadCount})");
        Thread.Sleep(300);
        DbConnection = "Server=localhost;Db=School";
        Theme = "Light";
    }
}

public class DatabaseService
{
    public AppConfig Config { get; } = AppConfig.Instance;

    public void Connect()
    {
        Console.WriteLine($"Connecting to {Config.DbConnection}");
    }
}

public class UiService
{
    public AppConfig Config { get; } = AppConfig.Instance;

    public void Render()
    {
        Console.WriteLine($"UI is using theme: {Config.Theme}");
    }
}
