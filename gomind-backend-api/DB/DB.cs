using MySqlConnector;
using System.Data;

namespace gomind_backend_api.DB
{
    public interface IMariaDbConnection
    {
        Task<MySqlConnection> GetConnectionAsync();
        MySqlConnection GetConnection();
        Task<T> ExecuteScalarAsync<T>(string query, Dictionary<string, object> parameters = null);
        Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters = null);
        Task<List<T>> ExecuteQueryAsync<T>(string query, Func<MySqlDataReader, T> mapper, Dictionary<string, object> parameters = null);
        Task<bool> TestConnectionAsync();
    }

    public class MariaDbConnection : IMariaDbConnection, IDisposable
    {
        private readonly string _connectionString;
        private MySqlConnection _connection;
        private bool _disposed = false;

        public MariaDbConnection(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MariaDb")
                ?? throw new InvalidOperationException("MariaDb connection string not found");
        }

        public MariaDbConnection(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public MySqlConnection GetConnection()
        {
            if (_connection == null || _connection.State == ConnectionState.Closed)
            {
                _connection = new MySqlConnection(_connectionString);
                _connection.Open();
            }
            return _connection;
        }

        public async Task<MySqlConnection> GetConnectionAsync()
        {
            if (_connection == null || _connection.State == ConnectionState.Closed)
            {
                _connection = new MySqlConnection(_connectionString);
                await _connection.OpenAsync();
            }
            return _connection;
        }

        public async Task<T> ExecuteScalarAsync<T>(string query, Dictionary<string, object> parameters = null)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand(query, connection);

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
                    }
                }

                var result = await command.ExecuteScalarAsync();
                return result == null || result == DBNull.Value ? default(T) : (T)Convert.ChangeType(result, typeof(T));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error executing scalar query: {ex.Message}", ex);
            }
        }

        public async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters = null)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand(query, connection);

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
                    }
                }

                return await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error executing non-query: {ex.Message}", ex);
            }
        }

        public async Task<List<T>> ExecuteQueryAsync<T>(string query, Func<MySqlDataReader, T> mapper, Dictionary<string, object> parameters = null)
        {
            var results = new List<T>();

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand(query, connection);

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
                    }
                }

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    results.Add(mapper(reader));
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error executing query: {ex.Message}", ex);
            }

            return results;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                return connection.State == ConnectionState.Open;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _connection?.Close();
                _connection?.Dispose();
                _disposed = true;
            }
        }

        ~MariaDbConnection()
        {
            Dispose(false);
        }
    }

    // Clase de configuración para opciones adicionales
    public class MariaDbOptions
    {
        public string ConnectionString { get; set; }
        public int CommandTimeout { get; set; } = 30;
        public int ConnectionTimeout { get; set; } = 15;
        public bool Pooling { get; set; } = true;
        public int MinPoolSize { get; set; } = 0;
        public int MaxPoolSize { get; set; } = 100;
    }

    // Builder para crear connection string
    public class MariaDbConnectionStringBuilder
    {
        private readonly MySqlConnectionStringBuilder _builder;

        public MariaDbConnectionStringBuilder()
        {
            _builder = new MySqlConnectionStringBuilder();
        }

        public MariaDbConnectionStringBuilder Server(string server)
        {
            _builder.Server = server;
            return this;
        }

        public MariaDbConnectionStringBuilder Database(string database)
        {
            _builder.Database = database;
            return this;
        }

        public MariaDbConnectionStringBuilder UserId(string userId)
        {
            _builder.UserID = userId;
            return this;
        }

        public MariaDbConnectionStringBuilder Password(string password)
        {
            _builder.Password = password;
            return this;
        }

        public MariaDbConnectionStringBuilder Port(uint port)
        {
            _builder.Port = port;
            return this;
        }

        public MariaDbConnectionStringBuilder Pooling(bool pooling)
        {
            _builder.Pooling = pooling;
            return this;
        }

        public MariaDbConnectionStringBuilder ConnectionTimeout(uint timeout)
        {
            _builder.ConnectionTimeout = timeout;
            return this;
        }

        public string Build()
        {
            return _builder.ConnectionString;
        }
    }
}
