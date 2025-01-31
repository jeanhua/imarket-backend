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
        public async Task<IEnumerable<MessageModels>> GetMessagesBySenderIdAsync(ulong userId, int page, int pageSize)
        {
            if (page < 1 || pageSize < 1)
            {
                page = 1;
                pageSize = 10;
            }
            if (pageSize > 20)
            {
                pageSize = 20;
            }
            var messages = new List<MessageModels>();
            var query = "SELECT * FROM Messages WHERE SenderId = @SenderId LIMIT @Offset, @PageSize";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@SenderId",userId),
                new MySqlParameter("@Offset",(page - 1) * pageSize),
                new MySqlParameter("@PageSize",pageSize)
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                messages.Add(new MessageModels
                {
                    Id = ulong.Parse(row["Id"].ToString()!),
                    SenderId = ulong.Parse(row["SenderId"].ToString()!),
                    ReceiverId = ulong.Parse(row["ReceiverId"].ToString()!),
                    Content = row["Content"].ToString()!,
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                });
            }
            return messages;
        }

        public async Task<IEnumerable<MessageModels>> GetMessagesByReceiverIdAsync(ulong userId, int page, int pageSize)
        {
            if (page < 1 || pageSize < 1)
            {
                page = 1; pageSize = 10;
            }
            if (pageSize > 20)
            { 
                pageSize = 20;
            }
            var messages = new List<MessageModels>();
            var query = "SELECT * FROM Messages WHERE ReceiverId = @ReceiverId LIMIT @Offset, @PageSize";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@ReceiverId",userId),
                new MySqlParameter("@Offset",(page - 1) * pageSize),
                new MySqlParameter("@PageSize",pageSize)
            };
            var result = await _database.ExecuteQuery(query, CommandType.Text, parameters);
            foreach (DataRow row in result.Rows)
            {
                messages.Add(new MessageModels
                {
                    Id = ulong.Parse(row["Id"].ToString()!),
                    SenderId = ulong.Parse(row["SenderId"].ToString()!),
                    ReceiverId = ulong.Parse(row["ReceiverId"].ToString()!),
                    Content = row["Content"].ToString()!,
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
                });
            }
            return messages;
        }

        public async Task<MessageModels?> GetMessageByIdAsync(ulong id)
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
                Id = ulong.Parse(row["Id"].ToString()!),
                SenderId = ulong.Parse(row["SenderId"].ToString()!),
                ReceiverId = ulong.Parse(row["ReceiverId"].ToString()!),
                Content = row["Content"].ToString()!,
                CreatedAt = Convert.ToDateTime(row["CreatedAt"]!),
            };
        }

        public async Task<int> CreateMessageAsync(MessageModels message)
        {
            var query = "INSERT INTO Messages (SenderId, ReceiverId, Content, CreatedAt) VALUES (@SenderId, @ReceiverId, @Content, @CreatedAt)";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@SenderId", message.SenderId),
                new MySqlParameter("@ReceiverId", message.ReceiverId),
                new MySqlParameter("@Content", message.Content),
                new MySqlParameter("@CreatedAt", message.CreatedAt),
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> DeleteMessageByIdAsync(ulong id)
        {
            var query = "DELETE FROM Messages WHERE Id = @Id";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Id", id)
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> DeleteMessageByReceiverIdAsync(ulong receiverId)
        {
            var query = "DELETE FROM Messages WHERE ReceiverId = @ReceiverId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@ReceiverId", receiverId )
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> DeleteMessageBySenderIdAsync(ulong senderId)
        {
            var query = "DELETE FROM Messages WHERE SenderId = @SenderId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@SenderId", senderId )
            };
            return await _database.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public async Task<int> DeleteMessageBySenderToReceiverIdAsync(ulong senderId, ulong receiverid)
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
