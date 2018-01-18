using System.Threading.Tasks;
using ChatProtos.Networking;
using ChatProtos.Networking.Messages;
using Google.Protobuf;
using HServer.Networking;

namespace ChatServer.Messaging.Commands
{
    public class UserInfoServerCommand : IChatServerCommand
    {
        public async Task ExecuteTask(HChatClient client, RequestMessage message)
        {
            if (client.Authenticated)
            {
                await SendInfoToUserTask(client);
            }
            else
            {
                await SendErrorTask(client);
            }
        }

        private async Task SendInfoToUserTask(HChatClient client)
        {
            var response = new ResponseMessage
            {
                Status = ResponseStatus.Success,
                Type = (int)RequestType.UserInfo,
                Message = new UserInfoResponse
                {
                    UserId = client.Id,
                    User = null
                }.ToByteString()
            };
            // await connection.SendAyncTask()
        }

        private async Task SendErrorTask(HChatClient client)
        {
            var response = new ResponseMessage
            {
                Status = ResponseStatus.Unauthorized,
                Type = (int)RequestType.UserInfo
            };
            await client.Connection.SendAyncTask(response.ToByteArray());
        }
    }
}