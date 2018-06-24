namespace ChatServer
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using ChatProtos.Data;
    using ChatProtos.Networking;

    using ChatServer.HAuthentication;

    using Google.Protobuf;

    using HServer;
    using HServer.Networking;

    using JetBrains.Annotations;

    public class HChatClient
    {
        /// <summary>
        /// Joined channels.
        /// </summary>
        [NotNull]
        private readonly ConcurrentDictionary<Guid, HChannel> _channels = new ConcurrentDictionary<Guid, HChannel>();

        /// <summary>
        /// Client friends.
        /// </summary>
        [NotNull]
        private readonly ConcurrentDictionary<Guid, HChatClient> _friends =
            new ConcurrentDictionary<Guid, HChatClient>();

        /// <summary>
        /// Gets the display name.
        /// </summary>
        [NotNull]
        private string _displayName = string.Empty;

        /// <summary>
        /// Joined communities.
        /// </summary>
        [NotNull]
        private readonly ConcurrentDictionary<Guid, HCommunity> _communities =
            new ConcurrentDictionary<Guid, HCommunity>();

        /// <summary>
        /// Initializes a new instance of the <see cref="HChatClient"/> class.
        /// </summary>
        /// <param name="connection">
        /// The <see cref="HConnection"/>.
        /// </param>
        /// <param name="created">
        /// The date time when client was created.
        /// </param>
        public HChatClient([NotNull] HConnection connection, DateTime created)
        {
            Connection = connection;
            Created = created;
        }

        /// <summary>
        /// Gets the underlying <see cref="HConnection"/>.
        /// </summary>
        [NotNull]
        public HConnection Connection { get; }

        /// <summary>
        /// Gets a value indicating whether <see cref="HChatClient"/> is authenticated.
        /// </summary>
        public bool Authenticated { get; private set; }

        /// <summary>
        /// Gets the ID.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the login token.
        /// </summary>
        [NotNull]
        public string Token { get; private set; } = string.Empty; // Maybe remove

        /// <summary>
        /// Gets the username.
        /// </summary>
        [NotNull]
        public string Username { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the date time when client was created.
        /// </summary>
        [NotNull]
        public DateTime Created { get; private set; }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [NotNull]
        public string GetDisplayName()
        {
            if (_displayName != string.Empty)
            {
                return _displayName;
            }

            return (Username != string.Empty) ? Username : Id.ToString();
        }

        /// <summary>
        /// Update clients display name.
        /// </summary>
        /// <param name="name">
        /// New display name.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool UpdateDisplayName([CanBeNull] string name)
        {
            // TODO: Some validation of illegal characters.
            if (name == null)
            {
                return false;
            }

            _displayName = name;
            return true;
        }

        /// <summary>
        /// Add <see cref="HChannel"/> to joined channel dictionary.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool AddChannel([NotNull] HChannel channel)
        {
            return _channels.TryAdd(channel.Id, channel);
        }

        /// <summary>
        /// Removes <see cref="HChannel"/> from joined channel dictionary.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool RemoveChannel([NotNull] HChannel channel)
        {
            return _channels.TryRemove(channel.Id, out channel);
        }

        /// <summary>
        /// Gets user channels.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/> channels.
        /// </returns>
        public IEnumerable<HChannel> GetChannels()
        {
            return _channels.Values.ToArray();
        }

        /// <summary>
        /// Add <see cref="HCommunity"/> to joined channel dictionary.
        /// </summary>
        /// <param name="community">
        /// The channel.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool AddCommunity([NotNull] HCommunity community)
        {
            return _communities.TryAdd(community.Id, community);
        }

        /// <summary>
        /// Removes <see cref="HCommunity"/> from joined channel dictionary.
        /// </summary>
        /// <param name="community">
        /// The channel.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool RemoveCommunity([NotNull] HCommunity community)
        {
            return _communities.TryRemove(community.Id, out community);
        }

        /// <summary>
        /// Gets user communities.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/> communities.
        /// </returns>
        public IEnumerable<HCommunity> GetCommunities()
        {
            return _communities.Values.ToArray();
        }

        /// <summary>
        /// Sends <see cref="ResponseMessage"/> to client.
        /// </summary>
        /// <param name="status">
        ///     Response status.
        /// </param>
        /// <param name="type">
        ///     Request type.
        /// </param>
        /// <param name="message">
        ///     Message bytes.
        /// </param>
        /// <param name="nonce">
        ///     Message nonce.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public Task SendResponseTaskAsync(
            [NotNull] ResponseStatus status,
            [NotNull] RequestType type,
            [CanBeNull] ByteString message,
            [CanBeNull] string nonce = "")
        {
            return Connection.SendMessageTask(
                new ResponseMessage { Status = status, Type = (int)type, Nonce = nonce, Message = message }
                    .ToByteArray());
        }

        /// <summary>
        /// Sends <see cref="ResponseMessage"/> to client.
        /// </summary>
        /// <param name="status">
        /// The <see cref="ResponseStatus"/>.
        /// </param>
        /// <param name="message">
        /// The response message <see cref="ByteString"/>.
        /// </param>
        /// <param name="request">
        /// The <see cref="RequestMessage"/>.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public Task SendResponseTaskAsync(
            [NotNull] ResponseStatus status,
            [CanBeNull] ByteString message,
            [NotNull] RequestMessage request)
        {
            return Connection.SendMessageTask(
                new ResponseMessage { Status = status, Type = request.Type, Nonce = request.Nonce, Message = message }
                    .ToByteArray());
        }

        /* 
        public static Predicate<HChatClient> ByIdPredicate(string id)
        {
            return hClient => hClient.Id.ToString() == id;
        }

        public static Predicate<HChatClient> ByDisplayNamePredicate(string displayName)
        {
            return hClient => hClient.DisplayName == displayName;
        }

        public static Predicate<HChatClient> ByChannel(HChannel channel)
        {
            return hClient => hClient._channels.Values.Any(joinedChannel => joinedChannel.Equals(channel)); // Actually change to use actual channel objects  
        }*/

        [SuppressMessage(
            "StyleCop.CSharp.DocumentationRules",
            "SA1600:ElementsMustBeDocumented",
            Justification = "Should be reworked")] // TODO: Rework authentication
        public async Task<Tuple<string, string>> TryAuthenticatingTask(
            [CanBeNull] string username,
            [CanBeNull] string password = null,
            [CanBeNull] string token = null)
        {
            var authenticator = new HAuthenticator(AuthenticatorBackend.None);
            var authenticationResponse = new AuthenticationResponse(false, null, null, null);
            if (password != null)
            {
                authenticationResponse =
                    await authenticator.TryPasswordAuthenticationTask(this, password).ConfigureAwait(false);
            }
            else if (token != null)
            {
                authenticationResponse =
                    await authenticator.TryTokenAuthenticationTask(this, token).ConfigureAwait(false);
            }

            if (authenticationResponse.Success)
            {
                // Username = username;
                if (Guid.TryParse(authenticationResponse.Id, out var result))
                {
                    Id = result;
                    Authenticated = true;
                    Token = authenticationResponse.Token;
                }
                else
                {
                    Console.WriteLine("[SERVER] Coulnd't parse GUID for user {0}", username);
                }

                // DisplayName = authenticationResponse.DisplayName;
            }

            return Tuple.Create(Id.ToString(), Token);
        }

        /// <summary>
        /// Tries de-authenticating user.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task TryDeauthenticatingTask()
        {
            var authenticator = new HAuthenticator(AuthenticatorBackend.None);
            var success = await authenticator.DeauthenticateClientTask(this).ConfigureAwait(false);
            if (success)
            {
                await CloseAsyncTask().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Client closing task.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task CloseAsyncTask()
        {
            await Connection.CloseTask().ConfigureAwait(false);
        }

        /// <summary>
        /// The get as protobuf user.
        /// </summary>
        /// <returns>
        /// The <see cref="User"/>.
        /// </returns>
        [NotNull]
        public User GetAsUserProto()
        {
            // TODO: Add user channels/friends
            return new User
                       {
                           Id = Id.ToString(),
                           Created = Created.ToString(CultureInfo.InvariantCulture),
                           DisplayName = GetDisplayName(),
                           Communities = { _communities.Values.Select(community => community.GetAsProtobuf()).Append(getHomeCommunity()) },
                           Roles = { },
                       };
        }

        private Community getHomeCommunity()
        {
            var channels = _friends.Values.Select(
                friend => new Channel { Id = friend.Id.ToString(), Name = friend.GetDisplayName(), });
            return new Community { Id = "~", Name = "Home", Channels = { channels } };
        }
    }
}