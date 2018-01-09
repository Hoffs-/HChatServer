using System.Threading.Tasks;
using ChatProtos.Networking;
using CoreServer;
using CoreServer.HMessaging;

namespace ChatServer.Messaging.Commands
{
    public interface IChatServerCommand
    {
        Task ExecuteTask(HChatClient client, RequestMessage message);
    }
}