namespace ChatServer.Messaging.Commands
{
    using System.Globalization;
    using System.Threading.Tasks;

    using ChatProtos.Data;
    using ChatProtos.Networking;
    using ChatProtos.Networking.Messages;

    using Google.Protobuf;

    using HServer.Networking;

    using JetBrains.Annotations;

    /// <inheritdoc />
    /// <summary>
    /// The user info server command.
    /// </summary>
    public class UserInfoServerCommand : IChatServerCommand
    {
        /// <inheritdoc />
        public async Task ExecuteTask(HChatClient client, RequestMessage message)
        {
            if (client.Authenticated)
            {
                await SendInfoTask(client).ConfigureAwait(false);
            }
            else
            {
                await SendErrorTask(client).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The send info task.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private static async Task SendInfoTask([NotNull] HChatClient client)
        {
            var response = new UserInfoResponse
            {
                UserId = client.Id.ToString(), 
                User = new User
                {
                    Id = client.Id.ToString(),
                    DisplayName = client.GetDisplayName(),
                    Created = client.Created.ToString(CultureInfo.InvariantCulture),
                    JoinedChannels = { },
                    Roles = { }
                }
            }.ToByteString();
            await client.SendResponseTask(ResponseStatus.Success, RequestType.UserInfo, response).ConfigureAwait(false);
        }

        /// <summary>
        /// The send error task.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private static async Task SendErrorTask(HChatClient client)
        {
            var response = new ResponseMessage
            {
                Status = ResponseStatus.Unauthorized,
                Type = (int)RequestType.UserInfo
            };
            await client.Connection.SendMessageTask(response.ToByteArray());
        }
    }
}