using imarket.service.IService;
using MySql.Data.MySqlClient;
using System.Data;

namespace imarket.utils
{
    public class Database
    {
        private readonly string? connectionString;
        private readonly ILogger<Database> _logger;
        IConfiguration configuration;

        public Database(IConfiguration configuration, ILogger<Database> logger)
        {
            this._logger = logger;
            this.configuration = configuration;
            connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Database connection string is empty");
                Environment.Exit(1);
            }
        }
        public async void InitDatabase()
        {
            using (var connection = GetConnection())
            {
                _logger.LogInformation("Database connected");
                // 创建表
                _logger.LogInformation("Checking tables");
                var query = File.ReadAllText("./create_tables_script.sql");
                using (var command = new MySqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
                // 检查是否有超级管理员账户，没有则创建
                query = "SELECT * FROM Users WHERE Role = 'admin' AND Username = @Username";
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@Username",configuration["admin:Username"])
                };
                var result = await ExecuteQuery(query, CommandType.Text,parameters);
                if (result.Rows.Count == 0)
                {
                    var username = configuration["admin:Username"];
                    var password = configuration["admin:Password"];
                    var Email = configuration["admin:Email"];
                    if (string.IsNullOrEmpty(password))
                    {
                        _logger.LogError("Admin password is empty");
                        Environment.Exit(1);
                    }
                    if (string.IsNullOrEmpty(username))
                    {
                        _logger.LogError("Admin username is empty");
                        Environment.Exit(1);
                    }
                    if (string.IsNullOrEmpty(Email))
                    {
                        _logger.LogError("Admin Email is empty");
                        Environment.Exit(1);
                    }
                    var passwordHash = SHA256Encryptor.Encrypt(password);
                    // 创建管理员账户
                    query = "INSERT INTO Users (Username, Nickname, PasswordHash, Avatar, Email, Role, CreatedAt, Status) VALUES (@Username, @Nickname, @PasswordHash, @Avatar, @Email, @Role, @CreatedAt, @Status)";
                    parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@Username", username),
                        new MySqlParameter("@Nickname", "admin"),
                        new MySqlParameter("@PasswordHash", passwordHash),
                        new MySqlParameter("@Avatar", "/images/defaultAvatar.svg"),
                        new MySqlParameter("@Email", Email),
                        new MySqlParameter("@Role", "admin"),
                        new MySqlParameter("@CreatedAt", DateTime.Now),
                        new MySqlParameter("@Status", 1)
                    };
                    var cu = await ExecuteNonQuery(query, CommandType.Text, parameters);
                    if(cu == 0)
                    {
                        _logger.LogError("Can's create the admin");
                        Environment.Exit(1);
                    }
                    _logger.LogInformation("Admin account created");
                }
            }
        }

        // 数据库连接
        private MySqlConnection GetConnection()
        {
            var connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                _logger.LogError("Database connection failed", ex);
                Environment.Exit(1);
            }
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
        public async Task<(int result,ulong)> ExecuteNonQueryWithId(string query, CommandType commandType, MySqlParameter[] parameters = null)
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
                    ulong id = Convert.ToUInt64(await command.ExecuteScalarAsync());
                    return (await command.ExecuteNonQueryAsync(),id);
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