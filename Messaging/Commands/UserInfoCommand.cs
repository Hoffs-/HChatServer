namespace ChatServer.Messaging.Commands
{
    using System.Threading.Tasks;

    using ChatProtos.Networking;
    using ChatProtos.Networking.Messages;

    using Google.Protobuf;

    using HServer;
    using HServer.Networking;

    using JetBrains.Annotations;

    /// <inheritdoc />
    /// <summary>
    /// The user info server command.
    /// </summary>
    public class UserInfoServerCommand : IChatServerCommand
    {
        /// <inheritdoc />
        public async Task ExecuteTaskAsync([NotNull] HChatClient client, [NotNull] RequestMessage message)
        {
            if (!client.Authenticated)
            {
                await client.SendResponseTaskAsync(
                    ResponseStatus.Unauthorized,
                    RequestType.UserInfo,
                    ByteString.Empty,
                    message.Nonce).ConfigureAwait(false);
                return;
            }

            var didParse = ProtobufHelper.TryParse(UserInfoRequest.Parser, message.Message, out var parsed);
            if (!didParse)
            {
                await client.SendResponseTaskAsync(
                    ResponseStatus.Error,
                    RequestType.UserInfo,
                    ByteString.Empty,
                    message.Nonce).ConfigureAwait(false);
            }

            if (parsed.UserId.Length == 0 || parsed.UserId == client.Id.ToString())
            {
                var response =
                    new UserInfoResponse { UserId = client.Id.ToString(), User = client.GetAsUserProto(), }
                        .ToByteString();
                await client.SendResponseTaskAsync(
                    ResponseStatus.Success,
                    RequestType.UserInfo,
                    response,
                    message.Nonce).ConfigureAwait(false);
                return;
            }

            foreach (var community in client.GetCommunities())
            {
                var foundClient = community.GetItem(parsed.UserId);
                if (foundClient == null)
                {
                    continue;
                }

                var response =
                    new UserInfoResponse { UserId = foundClient.Id.ToString(), User = foundClient.GetAsUserProto(), }
                        .ToByteString();
                await client.SendResponseTaskAsync(
                    ResponseStatus.Success,
                    RequestType.UserInfo,
                    response,
                    message.Nonce).ConfigureAwait(false);
                return;
            }

            await client.SendResponseTaskAsync(
                ResponseStatus.NotFound,
                RequestType.UserInfo,
                ByteString.Empty,
                message.Nonce).ConfigureAwait(false);
        }
    }
}