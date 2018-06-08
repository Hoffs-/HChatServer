namespace ChatServer.Messaging.Commands
{
    using System.Threading.Tasks;

    using HServer.Networking;

    /// <inheritdoc />
    /// <summary>
    /// The remove role server command.
    /// </summary>
    public class RemoveRoleCommand : IChatServerCommand
    {
        /// <inheritdoc />
        public async Task ExecuteTaskAsync(HChatClient client, RequestMessage message)
        {
            throw new System.NotImplementedException();
        }
    }
}