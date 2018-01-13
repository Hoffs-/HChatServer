using System.Threading.Tasks;
using HServer.ChatProtos.Networking;

namespace ChatServer.Messaging.Commands
{
    public interface IChatServerCommand
    {
        Task ExecuteTask(HChatClient client, RequestMessage message);
    }
}