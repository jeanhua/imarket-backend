using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;

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
        private static Database _instance;
        private readonly string connectionString;
        public static Database getInstance()
        {
            if (_instance == null)
            {
                _instance = new Database();
            }
            return _instance;
        }
        private Database()
        {
            try
            {
                JsonDocument config = JsonDocument.Parse(File.ReadAllText("appsettings.json"));
                connectionString = config.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnection").GetString()!;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database config error");
                // 结束程序
                Environment.Exit(1);
            }
            if(connectionString == null)
            {
                Console.WriteLine("Database config null");
                // 结束程序
                Environment.Exit(1);
            }
        }

        // 数据库连接
        private SqlConnection GetConnection()
        {
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        // 查询
        public async Task<DataTable> ExecuteQuery(string query, CommandType commandType, SqlParameter[] parameters = null)
        {
            using (var connection = GetConnection())
            {
                using (var command = new SqlCommand(query, connection))
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
        public async Task<int> ExecuteNonQuery(string query, CommandType commandType, SqlParameter[] parameters = null)
        {
            using (var connection = GetConnection())
            {
                using (var command = new SqlCommand(query, connection))
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
        public async Task ExecuteTransaction(Action<SqlConnection> action)
        {
            using (var connection = GetConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        action(connection);
                        await transaction.CommitAsync(); // 提交事务
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); // 发生异常时回滚事务
                        throw new Exception("Transaction failed", ex);
                    }
                }
            }
        }

        // 获取单个值
        public async Task<object?> ExecuteScalar(string query, CommandType commandType, SqlParameter[] parameters = null)
        {
            using (var connection = GetConnection())
            {
                using (var command = new SqlCommand(query, connection))
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
