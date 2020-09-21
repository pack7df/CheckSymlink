using Spt.Shared.Core.Commands;
using System.Threading.Tasks;


namespace Spt.Shared.Logic
{
    public interface ICommandRouter
    {
        /// <summary>
        /// Executes a command and waits until complete
        /// </summary>
        /// <typeparam name="TResponse">Command result</typeparam>
        /// <param name="command">Command</param>
        /// <returns>The command execution result</returns>
        Task<TResponse> ExecuteAsync<TResponse>(Command<TResponse> command);

        /// <summary>
        /// Executes a command and waits until complete, returning the command result
        /// </summary>
        /// <typeparam name="TRequest">Command body</typeparam>
        /// <typeparam name="TResponse">Command result</typeparam>
        /// <param name="command">Command</param>
        /// <returns>The command execution result</returns>
        Task<TResponse> ExecuteAsync<TRequest, TResponse>(Command<TRequest, TResponse> command);

        /// <summary>
        /// Fire and forget command (Do not wait, runs async)
        /// </summary>
        /// <typeparam name="T">Command body</typeparam>
        /// <param name="command">Command</param>
        /// <returns></returns>
        Task SendAsync<T>(Command<T> command);

        /// <summary>
        /// Fire and forget command (Do not wait, runs async)
        /// </summary>
        /// <typeparam name="T">Command body</typeparam>
        /// <param name="command">Command</param>
        /// <returns></returns>
        Task SendAsync<TRequest, TResponse>(Command<TRequest, TResponse> command);
    }
}
