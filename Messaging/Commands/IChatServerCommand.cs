namespace ChatServer.Messaging.Commands
{
    using System.Threading.Tasks;

    using HServer.Networking;

    using JetBrains.Annotations;

    /// <summary>
    /// Interface for ChatServerCommand.
    /// </summary>
    public interface IChatServerCommand
    {
        /// <summary>
        /// The command execution task.
        /// </summary>
        /// <param name="client">
        /// The ChatClient who sent the command.
        /// </param>
        /// <param name="message">
        /// The sent message.
        /// </param>
        /// <returns>
        /// Task <see cref="Task"/>.
        /// </returns>
        [NotNull]
        Task ExecuteTaskAsync([NotNull] HChatClient client, [NotNull] RequestMessage message);
    }
}