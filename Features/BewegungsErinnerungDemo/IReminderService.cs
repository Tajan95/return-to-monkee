using System.Threading.Tasks;

namespace ReturnToMonkee.Features.BewegungsErinnerungDemo
{
    public interface IReminderService
    {
        Task StartAsync();
        Task StopAsync();
    }
}
