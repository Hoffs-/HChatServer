using System.Threading.Tasks;

namespace CoreServer.HMessaging.HAuthentication
{
    // TODO: Rework authentication to be more universal
    class HAuthenticator
    {
        private AuthenticatorBackend _backend;

        public HAuthenticator(AuthenticatorBackend backend)
        {
            _backend = backend;
        }

        public async Task<AuthenticationResponse> TryPasswordAuthenticationTask(HChatClient client, string password)
        {
            
            return new AuthenticationResponse(true, client.Id, "", "0000");
        }

        public async Task<AuthenticationResponse> TryTokenAuthenticationTask(HChatClient client, string token)
        {
            return new AuthenticationResponse(true, client.Id, "", "0000");
        }

        public async Task<bool> DeauthenticateClientTask(HChatClient client)
        {
            return true;
        }

    }

    /// <summary>
    /// Available authentication backends.
    /// </summary>
    enum AuthenticatorBackend
    {
        SQLite,
        PostgreSQL,
        None,
    }

    struct AuthenticationResponse
    {
        public bool Success;
        public string Id;
        public string DisplayName;
        public string Token;

        public AuthenticationResponse(bool success, string id, string displayName, string token)
        {
            Success = success;
            Id = id;
            DisplayName = displayName;
            Token = token;
        }
    }
}
