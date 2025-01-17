using imarket.models;
using imarket.service.IService;
using imarket.utils;
using Microsoft.Data.SqlClient;
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
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@SenderId", SqlDbType.Char) { Value = userId }
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
            var parameters = new SqlParameter[]
            {
            new SqlParameter("@ReceiverId", SqlDbType.Char) { Value = userId }
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
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", SqlDbType.Char) { Value = id }
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
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", SqlDbType.Char) { Value = message.Id },
                new SqlParameter("@SenderId", SqlDbType.Char) { Value = message.SenderId },
                new SqlParameter("@ReceiverId", SqlDbType.Char) { Value = message.ReceiverId },
                new SqlParameter("@Content", SqlDbType.NVarChar) { Value = message.Content },
                new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = message.CreatedAt },
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> DeleteMessageByIdAsync(string id)
        {
            var query = "DELETE FROM Messages WHERE Id = @Id";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", SqlDbType.Char) { Value = id }
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> DeleteMessageByReceiverIdAsync(string receiverId)
        {
            var query = "DELETE FROM Messages WHERE ReceiverId = @ReceiverId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@ReceiverId", SqlDbType.Char) { Value = receiverId }
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> DeleteMessageBySenderToReceiverIdAsync(string senderId, string receiverid)
        {
            var query = "DELETE FROM Messages WHERE SenderId = @SenderId AND ReceiverId = @ReceiverId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@SenderId", SqlDbType.Char) { Value = senderId },
                new SqlParameter("@ReceiverId", SqlDbType.Char) { Value = receiverid }
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
    }
}
