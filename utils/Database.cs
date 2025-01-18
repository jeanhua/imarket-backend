using MySql.Data.MySqlClient; // 使用 MySQL 的 NuGet 包
using System.Data;

/*
 * 数据库操作示例

 * 查询操作
var query = "SELECT * FROM Posts";
var posts = Database.getInstance().ExecuteQuery(query, CommandType.Text);

 * 非查询操作 (插入)
var query = "INSERT INTO Posts (Title, Content) VALUES (@Title, @Content)";
var parameters = new SqlParameter[]
{
    new SqlParameter("@Title", SqlDbType.NVarChar) { Value = "New Post" },
    new SqlParameter("@Content", SqlDbType.NVarChar) { Value = "This is a new post" }
};
int rowsAffected = Database.getInstance().ExecuteNonQuery(query, CommandType.Text, parameters);

 * 事务操作
 Database.getInstance().ExecuteTransaction(connection =>
{
    // 在事务中执行多个操作
    var command1 = new SqlCommand("UPDATE Posts SET Title = 'Updated Title' WHERE Id = 1", connection, connection.BeginTransaction());
    command1.ExecuteNonQuery();

    var command2 = new SqlCommand("INSERT INTO Posts (Title, Content) VALUES ('New Post', 'Content')", connection, connection.BeginTransaction());
    command2.ExecuteNonQuery();
});

 */

namespace imarket.utils
{
    public class Database
    {
        private readonly string? connectionString;
        private readonly ILogger<Database> _logger;

        public Database(IConfiguration configuration, ILogger<Database> logger)
        {
            this._logger = logger;
            connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Database connection string is empty");
                Environment.Exit(1);
            }
        }

        // 数据库连接
        private MySqlConnection GetConnection()
        {
            var connection = new MySqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        // 查询
        public async Task<DataTable> ExecuteQuery(string query, CommandType commandType, MySqlParameter[] parameters = null)
        {
            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = commandType;
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var dataTable = new DataTable();
                        dataTable.Load(reader);
                        return dataTable;
                    }
                }
            }
        }

        // 执行非查询命令
        public async Task<int> ExecuteNonQuery(string query, CommandType commandType, MySqlParameter[] parameters = null)
        {
            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = commandType;
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    return await command.ExecuteNonQueryAsync();
                }
            }
        }

        // 执行事务
        public async Task ExecuteTransaction(Action<MySqlConnection> action)
        {
            using (var connection = GetConnection())
            {
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        action(connection);
                        await transaction.CommitAsync(); // 提交事务
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(); // 发生异常时回滚事务
                        throw new Exception("Transaction failed", ex);
                    }
                }
            }
        }

        // 获取单个值
        public async Task<object?> ExecuteScalar(string query, CommandType commandType, MySqlParameter[] parameters = null)
        {
            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = commandType;
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    try
                    {
                        return await command.ExecuteScalarAsync();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("ExecuteScalar failed", ex);
                    }
                }
            }
        }
    }
}