using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataPetaniDesa
{
    public class PetaniDAL
    {
        // Memanggil class koneksi utama yang sudah kamu buat sebelumnya
        private Database db = new Database();

        // 1. Fungsi READ (Menampilkan Semua Data dari View)
        public DataTable GetAllPetani()
        {
            string query = "SELECT * FROM vw_data_petani";
            return db.FetchAll(query);
        }

        // 2. Fungsi SEARCH (Mencari Data Petani)
        public DataTable SearchPetani(string keyword)
        {
            string query = "CALL sp_search_petani(@cari)";
            return db.FetchAll(query, new { cari = keyword });
        }

        // 3. Fungsi INSERT (Menambah Data + Parameter Foto)
        public void InsertPetani(string nik, string nama, string jk, string tgl, string telp, string alamat, string tanaman, string foto)
        {
            string query = "CALL sp_insert_petani(@nik, @nama, @jk, @tgl, @telp, @alamat, @tanaman, @foto)";
            db.Execute(query, new { nik = nik, nama = nama, jk = jk, tgl = tgl, telp = telp, alamat = alamat, tanaman = tanaman, foto = foto });
        }

        // 4. Fungsi UPDATE (Mengubah Data + Parameter Foto)
        public void UpdatePetani(string id, string nik, string nama, string jk, string tgl, string telp, string alamat, string tanaman, string foto)
        {
            string query = "CALL sp_update_petani(@id, @nik, @nama, @jk, @tgl, @telp, @alamat, @tanaman, @foto)";
            db.Execute(query, new { id = id, nik = nik, nama = nama, jk = jk, tgl = tgl, telp = telp, alamat = alamat, tanaman = tanaman, foto = foto });
        }

        // 5. Fungsi DELETE (Menghapus Data)
        public void DeletePetani(string id)
        {
            string query = "CALL sp_delete_petani(@id)";
            db.Execute(query, new { id = id });
        }

        public DataTable GetStatistikTanaman()
        {
            string query = "SELECT * FROM vw_statistik_tanaman";
            return db.FetchAll(query);
        }

        // 7. Fungsi REKAP (Untuk Filter Laporan)
        public DataTable GetRekapPetani(string kelamin, string tanaman)
        {
            // Menggunakan trik SQL pintar: Jika parameter 'Semua', maka filter diabaikan.
            string query = @"
        SELECT * FROM vw_data_petani 
        WHERE (@jk = 'Semua' OR jenis_kelamin = @jk) 
        AND (@tanaman = 'Semua' OR jenis_tanaman = @tanaman)";

            return db.FetchAll(query, new { jk = kelamin, tanaman = tanaman });
        }
    }
}
