namespace ChatServer.Messaging.Commands
{
    using System.Threading.Tasks;

    using ChatProtos.Networking;
    using ChatProtos.Networking.Messages;

    using Google.Protobuf;

    using HServer;
    using HServer.Networking;

    /// <inheritdoc />
    /// <summary>
    /// The update display name command.
    /// </summary>
    public class UpdateDisplayNameCommand : IChatServerCommand
    {
        /// <inheritdoc />
        public async Task ExecuteTask(HChatClient client, RequestMessage message)
        {
            if (!client.Authenticated)
            {
                // TODO: Send response.
                return;
            }

            var parsed = ProtobufHelper.TryParse(UpdateDisplayRequest.Parser, message.Message, out var request);
            if (!parsed)
            {
                // TODO: Send response.
                return;
            }

            client.UpdateDisplayName(request.DisplayName);
            
            var response = new UpdateDisplayResponse
            {
                UserId = client.Id.ToString(),
                DisplayName = client.GetDisplayName()
            }.ToByteString();

            await client.SendResponseTask(ResponseStatus.Success, RequestType.UpdateDisplayName, response)
                .ConfigureAwait(false);
        }
    }
}