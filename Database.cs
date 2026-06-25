using System;
using System.Data;
using MySqlConnector;

namespace DataPetaniDesa
{
    public class Database
    {
        // =============================================
        //  Konfigurasi Koneksi XAMPP (Windows Lokal)
        // =============================================
        private readonly string _connectionString = new MySqlConnectionStringBuilder
        {
            Server = "",
            Database = "db_petani_desa",
            UserID = "root",
            Password = "Ikhlas1702.",         
            Port = 3306,
        }.ToString();

        // ─────────────────────────────────────────────
        //  Buka koneksi
        // ─────────────────────────────────────────────
        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        // ─────────────────────────────────────────────
        //  Tes koneksi (panggil sekali saat app start)
        // ─────────────────────────────────────────────
        public bool TestConnection()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    Console.WriteLine("✅ Koneksi MySQL berhasil!");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Koneksi gagal: {ex.Message}");
                return false;
            }
        }

        // ─────────────────────────────────────────────
        //  SELECT — ambil banyak baris
        // ─────────────────────────────────────────────
        public DataTable FetchAll(string query, object parameters = null)
        {
            var dt = new DataTable();
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        AddParameters(cmd, parameters);
                        using (var adapter = new MySqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FetchAll error: {ex.Message}");
            }
            return dt;
        }

        // ─────────────────────────────────────────────
        //  SELECT — ambil satu baris
        // ─────────────────────────────────────────────
        public DataRow FetchOne(string query, object parameters = null)
        {
            var dt = FetchAll(query, parameters);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        // ─────────────────────────────────────────────
        //  INSERT / UPDATE / DELETE
        // ─────────────────────────────────────────────
        public bool Execute(string query, object parameters = null)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        AddParameters(cmd, parameters);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Execute error: {ex.Message}");
                return false;
            }
        }

        // ─────────────────────────────────────────────
        //  Helper: masukkan parameter ke command
        // ─────────────────────────────────────────────
        private static void AddParameters(MySqlCommand cmd, object parameters)
        {
            if (parameters == null) return;

            foreach (var prop in parameters.GetType().GetProperties())
            {
                cmd.Parameters.AddWithValue("@" + prop.Name, prop.GetValue(parameters, null) ?? DBNull.Value);
            }
        }
    }
}