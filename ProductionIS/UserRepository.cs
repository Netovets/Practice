using System.Collections.Generic;
using Npgsql;
using ProductionIS.Config;
using ProductionIS.Entities;

namespace ProductionIS.Data
{
    public static class UserRepository
    {
        public static User? FindByLogin(string login)
        {
            using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(
                "SELECT id, login, password_hash, role, is_blocked, failed_attempts " +
                "FROM users WHERE login = @login", conn);
            cmd.Parameters.AddWithValue("login", login);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return MapUser(reader);
        }

        public static List<User> GetAllUsers()
        {
            var list = new List<User>();
            using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(
                "SELECT id, login, password_hash, role, is_blocked, failed_attempts " +
                "FROM users ORDER BY id", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(MapUser(reader));
            return list;
        }

        public static bool LoginExists(string login) => FindByLogin(login) != null;

        public static void AddUser(string login, string password, string role)
        {
            string hash = BCrypt.Net.BCrypt.HashPassword(password);
            using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(
                "INSERT INTO users (login, password_hash, role) VALUES (@login, @hash, @role)", conn);
            cmd.Parameters.AddWithValue("login", login);
            cmd.Parameters.AddWithValue("hash",  hash);
            cmd.Parameters.AddWithValue("role",  role);
            cmd.ExecuteNonQuery();
        }

        public static void UpdateUser(int userId, string login, string role)
        {
            using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(
                "UPDATE users SET login = @login, role = @role WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("login", login);
            cmd.Parameters.AddWithValue("role",  role);
            cmd.Parameters.AddWithValue("id",    userId);
            cmd.ExecuteNonQuery();
        }

        public static void IncrementFailedAttempts(int userId)
        {
            using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(
                "UPDATE users SET failed_attempts = failed_attempts + 1 WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", userId);
            cmd.ExecuteNonQuery();
        }

        public static void ResetFailedAttempts(int userId)
        {
            using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(
                "UPDATE users SET failed_attempts = 0 WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", userId);
            cmd.ExecuteNonQuery();
        }

        public static void BlockUser(int userId)
        {
            using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(
                "UPDATE users SET is_blocked = TRUE WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", userId);
            cmd.ExecuteNonQuery();
        }

        public static void UnblockUser(int userId)
        {
            using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(
                "UPDATE users SET is_blocked = FALSE, failed_attempts = 0 WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", userId);
            cmd.ExecuteNonQuery();
        }

        private static User MapUser(NpgsqlDataReader r) => new User
        {
            Id             = r.GetInt32(0),
            Login          = r.GetString(1),
            PasswordHash   = r.GetString(2),
            Role           = r.GetString(3),
            IsBlocked      = r.GetBoolean(4),
            FailedAttempts = r.GetInt32(5)
        };
    }
}
