using System.Threading.Tasks;
using ChatProtos.Networking;
using HServer.Networking;

namespace ChatServer.Messaging.Commands
{
    public interface IChatServerCommand
    {
        Task ExecuteTask(HChatClient client, RequestMessage message);
    }
}