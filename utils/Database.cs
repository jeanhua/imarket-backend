using imarket.service.IService;
using MySql.Data.MySqlClient;
using System.Data;

namespace imarket.utils
{
    public class Database
    {
        private readonly string? connectionString;
        private readonly ILogger<Database> _logger;
        private readonly IUserService userService;
        IConfiguration configuration;

        public Database(IConfiguration configuration, ILogger<Database> logger, IUserService userService)
        {
            this._logger = logger;
            this.configuration = configuration;
            this.userService = userService;
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
                var query = File.ReadAllText("./create_tables_script.sql");
                using (var command = new MySqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
                // 检查是否有管理员账户，没有则创建
                query = "SELECT * FROM Users WHERE Role = 'admin'";
                var result = await ExecuteQuery(query, CommandType.Text);
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
                    await userService.CreateUserAsync(new models.UserModels
                    {
                        Id = Guid.NewGuid().ToString(),
                        Username = username,
                        Nickname = username,
                        PasswordHash = passwordHash,
                        Avatar = "/images/defaultAvatar.png",
                        Email = Email,
                        Role = "admin",
                        CreatedAt = DateTime.Now,
                        Status = 1
                    });
                }
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