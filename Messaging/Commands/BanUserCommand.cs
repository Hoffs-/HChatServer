using System.Threading.Tasks;
using ChatProtos.Networking;
using HServer.Networking;

namespace ChatServer.Messaging.Commands
{
    public class BanUserServerCommand : IChatServerCommand
    {
        public async Task ExecuteTask(HChatClient client, RequestMessage message)
        {
            throw new System.NotImplementedException();
        }
    }
}