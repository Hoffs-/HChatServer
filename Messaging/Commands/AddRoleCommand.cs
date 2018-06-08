namespace ChatServer.Messaging.Commands
{
    using System.Threading.Tasks;

    using HServer.Networking;

    /// <inheritdoc />
    /// <summary>
    /// The add role server command.
    /// </summary>
    public class AddRoleServerCommand : IChatServerCommand
    {
        /// <inheritdoc />
        public async Task ExecuteTaskAsync(HChatClient client, RequestMessage message)
        {
            throw new System.NotImplementedException();
        }
    }
}