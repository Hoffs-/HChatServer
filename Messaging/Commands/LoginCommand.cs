﻿namespace ChatServer.Messaging.Commands
{
    using System;
    using System.Threading.Tasks;

    using ChatProtos.Networking;
    using ChatProtos.Networking.Messages;

    using Google.Protobuf;

    using HServer.Networking;

    using JetBrains.Annotations;

    /// <inheritdoc />
    /// <summary>
    /// The Login server command.
    /// </summary>
    public class LoginServerCommand : IChatServerCommand
    {
        /// <summary>
        /// The server client manager.
        /// </summary>
        [NotNull]
        private readonly HClientManager _clientManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginServerCommand"/> class.
        /// </summary>
        /// <param name="clientManager">
        /// The Server client manager.
        /// </param>
        public LoginServerCommand([NotNull] HClientManager clientManager)
        {
            _clientManager = clientManager;
        }

        /// <inheritdoc />
        public async Task ExecuteTask(HChatClient client, RequestMessage message)
        {
            if (client.Authenticated)
            {
                // If client is already authenticated return error.
                await SendErrorMessageTask(client).ConfigureAwait(false);
                return;
            }

            var loginRequest = LoginRequest.Parser.ParseFrom(message.Message);
            var result = await client.TryAuthenticatingTask(loginRequest.Username, loginRequest.Password, loginRequest.Token).ConfigureAwait(false);
            Console.WriteLine("[SERVER] After login for client {0}: {1} {2}", client.Id, result.Item1, result.Item2);
            if (client.Authenticated)
            {
                // TODO: Set id/token/etc for client somewhere.
                await _clientManager.AddItemTask(client).ConfigureAwait(false);
                await SendSuccessMessageTask(client).ConfigureAwait(false);
            }
            else
            {
                await SendErrorMessageTask(client).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Sends success message to client.
        /// </summary>
        /// <param name="client">
        /// The receiving client.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private static async Task SendSuccessMessageTask([NotNull] HChatClient client)
        {
            var response = new LoginResponse { Token = client.Token, UserId = client.Id.ToString() }.ToByteString();
            await client.SendResponseTask(ResponseStatus.Success, RequestType.Login, response).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends error message to client.
        /// </summary>
        /// <param name="client">
        /// The receiving client.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private static async Task SendErrorMessageTask([NotNull] HChatClient client)
        {
            await client.SendResponseTask(ResponseStatus.Error, RequestType.Login, ByteString.Empty)
                .ConfigureAwait(false);
        }
    }
}