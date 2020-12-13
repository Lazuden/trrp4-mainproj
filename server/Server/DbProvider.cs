using System.Collections.Generic;
using System.Data.SQLite;

namespace Server
{
    public class DbProvider
    {
        private readonly string _connectionString;

        public DbProvider(string path)
        {
            _connectionString = new SQLiteConnectionStringBuilder
            {
                DataSource = path,
            }.ConnectionString;
        }

        public List<Host> GetHosts()
        {
            using var conn = new SQLiteConnection(_connectionString);
            var sCommand = new SQLiteCommand()
            {
                Connection = conn,
                CommandText = @"SELECT name, address FROM hosts;"
            };

            conn.Open();
            var reader = sCommand.ExecuteReader();

            var result = new List<Host>();

            while (reader.Read())
            {
                string name = (string)reader["name"];
                string address = (string)reader["address"];

                result.Add(new Host() { Address = address, Name = name });
            }

            return result;
        }

        public void SaveHosts(List<Host> hosts)
        {
            using var conn = new SQLiteConnection(_connectionString);

            conn.Open();

            var sCommand = new SQLiteCommand()
            {
                Connection = conn,
                CommandText = @"DELETE FROM hosts"
            };
            sCommand.ExecuteNonQuery();

            if (hosts.Count == 0) return;

            var sb = new string[hosts.Count];
            for (int i = 0; i < hosts.Count; i++)
            {
                sb[i] = $"(@name{i}, @address{i})";
            }
            var parameters = string.Join(", ", sb);

            using var cmd = new SQLiteCommand(@"INSERT INTO hosts (name, address) VALUES " + parameters, conn);

            for (int i = 0; i < hosts.Count; i++)
            {
                cmd.Parameters.AddWithValue($"@name{i}", hosts[i].Name);
                cmd.Parameters.AddWithValue($"@address{i}", hosts[i].Address);
            }
            cmd.ExecuteNonQuery();
        }
    }
}
