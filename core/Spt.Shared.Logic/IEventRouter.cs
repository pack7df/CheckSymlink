using Spt.Shared.Core.Events;
using System.Threading.Tasks;

namespace Spt.Shared.Logic
{
    public interface IEventRouter
    {
        /// <summary>
        /// Sends the integration event through a Bus (used to communicate multiple microservices)
        /// </summary>
        /// <param name="event">Event content</param>
        /// <returns></returns>
        Task SendAsync<T>(T @event) where T : Event;
    }
}
