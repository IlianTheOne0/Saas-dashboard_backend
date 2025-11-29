using SecuritySerive = Base.Services.SecurityService;
using Work = Service.Worker.Worker;

namespace Service;

internal class Program
{
    public static async Task Main(string[] args)
    {
        await SecuritySerive.LoadKeys();

        var worker = new Work();
        await worker.Run();
    }
}