using imarket.models;
using imarket.service.IService;
using imarket.utils;
using MySql.Data.MySqlClient;
using System.Data;
namespace imarket.service.Service
{
    public class MessageService : IMessageService
    {
        private readonly Database _database;
        public MessageService(Database database)
        {
            _database = database;
        }
        public async Task<IEnumerable<MessageModels>> GetMessagesBySenderIdAsync(string userId)
        {
            var messages = new List<MessageModels>();
            var query = "SELECT * FROM Messages WHERE SenderId = @SenderId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@SenderId",userId )
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                messages.Add(new MessageModels
                {
                    Id = row["Id"].ToString()!,
                    SenderId = row["SenderId"].ToString()!,
                    ReceiverId = row["ReceiverId"].ToString()!,
                    Content = row["Content"].ToString()!,
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                });
            }
            return messages;
        }

        public async Task<IEnumerable<MessageModels>> GetMessagesByReceiverIdAsync(string userId)
        {
            var messages = new List<MessageModels>();
            var query = "SELECT * FROM Messages WHERE ReceiverId = @ReceiverId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@ReceiverId",userId)
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                messages.Add(new MessageModels
                {
                    Id = row["Id"].ToString()!,
                    SenderId = row["SenderId"].ToString()!,
                    ReceiverId = row["ReceiverId"].ToString()!,
                    Content = row["Content"].ToString()!,
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                });
            }
            return messages;
        }

        public async Task<MessageModels?> GetMessageByIdAsync(string id)
        {
            var query = "SELECT * FROM Messages WHERE Id = @Id";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id",id )
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            if (result.Rows.Count == 0)
            {
                return null;
            }
            var row = result.Rows[0];
            return new MessageModels
            {
                Id = row["Id"].ToString()!,
                SenderId = row["SenderId"].ToString()!,
                ReceiverId = row["ReceiverId"].ToString()!,
                Content = row["Content"].ToString()!,
                CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
            };
        }

        public async Task<int> CreateMessageAsync(MessageModels message)
        {
            var query = "INSERT INTO Messages (Id, SenderId, ReceiverId, Content, CreatedAt) VALUES (@Id, @SenderId, @ReceiverId, @Content, @CreatedAt)";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", message.Id),
                new MySqlParameter("@SenderId", message.SenderId),
                new MySqlParameter("@ReceiverId", message.ReceiverId),
                new MySqlParameter("@Content", message.Content),
                new MySqlParameter("@CreatedAt", message.CreatedAt),
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> DeleteMessageByIdAsync(string id)
        {
            var query = "DELETE FROM Messages WHERE Id = @Id";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", id)
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> DeleteMessageByReceiverIdAsync(string receiverId)
        {
            var query = "DELETE FROM Messages WHERE ReceiverId = @ReceiverId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@ReceiverId", receiverId )
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> DeleteMessageBySenderIdAsync(string senderId)
        {
            var query = "DELETE FROM Messages WHERE SenderId = @SenderId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@SenderId", senderId )
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> DeleteMessageBySenderToReceiverIdAsync(string senderId, string receiverid)
        {
            var query = "DELETE FROM Messages WHERE SenderId = @SenderId AND ReceiverId = @ReceiverId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@SenderId", senderId),
                new MySqlParameter("@ReceiverId",receiverid)
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
    }
}
