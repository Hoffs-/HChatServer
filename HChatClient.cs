using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using ChatServer.HAuthentication;
using HServer;

namespace ChatServer
{
    public class HChatClient
    {
        public HConnection Connection { get; }
        public bool Authenticated { get; private set; }
        public string Id { get; private set; }
        public string Token { get; private set; } = string.Empty;
        public string Username { get; private set; } = string.Empty;
        public string DisplayName { get; private set; } = string.Empty;
        private readonly ConcurrentDictionary<string, HChannel> _channels = new ConcurrentDictionary<string, HChannel>();
        
        public HChatClient(HConnection connection)
        {
            Connection = connection;
        }

        public string GetDisplayName()
        {
            if (DisplayName != string.Empty)
            {
                return DisplayName;
            }

            return (Username != string.Empty) ? Username : Id;
        }

        public void AddChannel(HChannel channel)
        {
            try
            {
                _channels.TryAdd(channel.Guid.ToString(), channel);
            }
            catch (ArgumentException)
            {
                Console.WriteLine("[SERVER] User already in channel.");
                throw;
            }
        }

        public void RemoveChannel(HChannel channel)
        {
            _channels.TryRemove(channel.Guid.ToString(), out channel);
        }

        public static Predicate<HChatClient> ByIdPredicate(string id)
        {
            return hClient => hClient.Id == id;
        }

        
        public static Predicate<HChatClient> ByDisplayNamePredicate(string displayName)
        {
            return hClient => hClient.DisplayName == displayName;
        }

        
        public static Predicate<HChatClient> ByChannel(HChannel channel)
        {
            return hClient => hClient._channels.Values.Any(joinedChannel => joinedChannel.Equals(channel)); // Actually change to use actual channel objects  
        }

        public async Task<Tuple<string, string>> TryAuthenticatingTask(string username, string password = null,
            string token = null)
        {
            var authenticator = new HAuthenticator(AuthenticatorBackend.None);
            var authenticationResponse = new AuthenticationResponse(false, null, null, null);
            if (password != null)
            {
                authenticationResponse = await authenticator.TryPasswordAuthenticationTask(this, password);
            }
            else if (token != null)
            {
                authenticationResponse = await authenticator.TryTokenAuthenticationTask(this, token);
            }

            if (authenticationResponse.Success)
            {
                // Username = username;
                Id = authenticationResponse.Id;
                // DisplayName = authenticationResponse.DisplayName;
                Authenticated = true;
            }

            return Tuple.Create(Id, Token);
        }

        public async Task TryDeauthenticatingTask()
        {
            var authenticator = new HAuthenticator(AuthenticatorBackend.None);
            var success = await authenticator.DeauthenticateClientTask(this);
            if (success)
            {
                await CloseAsync();
            }
        }

        public async Task CloseAsync()
        {
            await Connection.CloseTask();
        }
    }
}