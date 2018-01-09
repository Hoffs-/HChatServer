using System.Threading.Tasks;
using ChatProtos.Networking;
using CoreServer;
using CoreServer.HMessaging;

namespace ChatServer.Messaging.Commands
{
    public class AddRoleServerCommand : IChatServerCommand
    {
        public async Task ExecuteTask(HChatClient client, RequestMessage message)
        {
            throw new System.NotImplementedException();
        }
    }
}