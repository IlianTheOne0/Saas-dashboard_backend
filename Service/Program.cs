namespace Service;

using SecuritySerive = Base.Services.SecurityService;
using Work = Worker.Worker;
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
            Logger.Info(TAG, "Loading Security Keys...");
            await SecuritySerive.LoadKeys();

            Logger.Info(TAG, "Supabase Ready.");

            Logger.Info(TAG, "Initializing Worker...");
            var worker = new Work();

            Logger.Info(TAG, "Starting Worker Run Loop...");
            await worker.Run();
        }
        catch (Exception error) { Logger.Fatal(TAG, "Application Crash", error); }
    }
}