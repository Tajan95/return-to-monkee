using System.Threading.Tasks;

namespace ReturnToMonkee.Services
{
    public interface IReminderService
    {
        Task StartAsync();
        Task StopAsync();
    }
}
