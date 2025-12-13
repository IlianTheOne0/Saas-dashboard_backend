using SecuritySerive = Base.Services.SecurityService;
using Work = Service.Worker.Worker;

namespace Service;

using Data.Entities;
using Data.Entities.Interfaces;
using Utils;

internal class Program
{
    private const string TAG = "SERVICE-BOOT";

    public static async Task Main(string[] args)
    {
        Logger.Info(TAG, "=============================================");
        Logger.Info(TAG, "     SAAS DASHBOARD BACKEND v18.11.2025      ");
        Logger.Info(TAG, "=============================================");

        try
        {
            await SecuritySerive.LoadKeys();

            IHomeReader homeReader = new HomeReader();
            ISettingsReader settingsReader = new SettingsReader();

            var worker = new Work(homeReader, settingsReader);
            await worker.Run();
        }
        catch (Exception error) { Logger.Fatal(TAG, "Application Crash", error); }
    }
}