using DataPetaniDesa;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PetaniDesa
{
    public partial class FormPetani : Form
    {
        // Panggil jembatan database
        private Database db = new Database();

        // Simpan ID untuk keperluan Edit dan Hapus
        private string idPetaniTerpilih = "";

        public FormPetani()
        {
            InitializeComponent();
            lblTotalRecord = this.Controls.Find("lblTotalRecord", true).FirstOrDefault() as Label;
        }


        // ==========================================
        // 1. TAMPIL & CARI DATA (READ)
        // ==========================================
        private void LoadData(string pencarian)
        {
            string query = "SELECT id_petani, nik, nama_petani, jenis_kelamin, tgl_lahir, no_telp, alamat, jenis_tanaman FROM petani";
            DataTable dt;

            if (string.IsNullOrEmpty(pencarian))
            {
                dt = db.FetchAll(query); // Tampil semua
            }
            else
            {
                query += " WHERE nama_petani LIKE @cari OR nik LIKE @cari";
                dt = db.FetchAll(query, new { cari = "%" + pencarian + "%" }); // Cari data
            }

            dgvPetani.DataSource = dt;
        }

        // ==========================================
        // 2. SIMPAN DATA (CREATE)
        // ==========================================
        private void btnSimpan_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNIK.Text) || string.IsNullOrWhiteSpace(txtNama.Text) || string.IsNullOrWhiteSpace(txtTelp.Text))
            {
                MessageBox.Show("Semua kolom wajib diisi! Tidak boleh ada yang kosong.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Hentikan proses simpan
            }

            // 2. Cek NIK harus tepat 16 digit
            if (txtNIK.Text.Length != 16)
            {
                MessageBox.Show("Format NIK salah! NIK harus berjumlah tepat 16 digit angka.", "Validasi Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtNIK.Focus(); // Arahkan kursor kembali ke kotak NIK
                return;
            }

            // 3. Cek No. Telepon minimal 10 digit
            if (txtTelp.Text.Length < 10)
            {
                MessageBox.Show("Nomor telepon tidak valid! Minimal harus 10 digit angka.", "Validasi Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtTelp.Focus();
                return;
            }

            string query = "INSERT INTO petani (nik, nama_petani, jenis_kelamin, tgl_lahir, no_telp, alamat, jenis_tanaman) " +
                           "VALUES (@nik, @nama, @jk, @tgl, @telp, @alamat, @tanaman)";

            bool sukses = db.Execute(query, new
            {
                nik = txtNIK.Text,
                nama = txtNama.Text,
                jk = cbJenisKelamin.SelectedItem?.ToString() ?? "",
                tgl = dtpTglLahir.Value.ToString("yyyy-MM-dd"),
                telp = txtTelp.Text,
                alamat = txtAlamat.Text,
                tanaman = txtTanaman.Text
            });

            if (sukses)
            {
                MessageBox.Show("Data berhasil disimpan!");
                BersihkanForm();
                LoadData("");
                HitungTotalData();
            }
        }

        // ==========================================
        // 3. AMBIL DATA KE FORM SAAT TABEL DIKLIK
        // ==========================================
        private void dgvPetani_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvPetani.Rows[e.RowIndex];

                // Ambil ID untuk patokan edit/hapus
                idPetaniTerpilih = row.Cells["id_petani"].Value.ToString();

                // Tampilkan data ke TextBox agar siap diedit
                txtNIK.Text = row.Cells["nik"].Value.ToString();
                txtNama.Text = row.Cells["nama_petani"].Value.ToString();
                cbJenisKelamin.SelectedItem = row.Cells["jenis_kelamin"].Value.ToString();
                dtpTglLahir.Value = Convert.ToDateTime(row.Cells["tgl_lahir"].Value);
                txtTelp.Text = row.Cells["no_telp"].Value.ToString();
                txtAlamat.Text = row.Cells["alamat"].Value.ToString();
                txtTanaman.Text = row.Cells["jenis_tanaman"].Value.ToString();
            }
        }
       
        private void btnCari_Click(object sender, EventArgs e)
        {
            LoadData(txtCari.Text);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            BersihkanForm();
        }

        private void BersihkanForm()
        {
            idPetaniTerpilih = "";       // Reset ID agar tidak salah edit/hapus
            txtNIK.Clear();
            txtNama.Clear();
            txtTelp.Clear();
            txtAlamat.Clear();
            txtTanaman.Clear();
            cbJenisKelamin.SelectedIndex = -1; // Kosongkan pilihan ComboBox
            dtpTglLahir.Value = DateTime.Now;  // Kembalikan tanggal ke hari ini
        }

        private void btnEdit_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(idPetaniTerpilih))
            {
                MessageBox.Show("Silakan klik salah satu data di tabel terlebih dahulu!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ==== TAMBAHKAN KONFIRMASI INI SESUAI SOAL ====
            DialogResult dialog = MessageBox.Show("Apakah Anda yakin ingin menyimpan perubahan data ini?", "Konfirmasi Edit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialog == DialogResult.No)
            {
                return; // Batal edit jika pilih No
            }

            string query = "UPDATE petani SET nik=@nik, nama_petani=@nama, jenis_kelamin=@jk, " +
                           "tgl_lahir=@tgl, no_telp=@telp, alamat=@alamat, jenis_tanaman=@tanaman " +
                           "WHERE id_petani=@id";

            bool sukses = db.Execute(query, new
            {
                nik = txtNIK.Text,
                nama = txtNama.Text,
                jk = cbJenisKelamin.SelectedItem?.ToString() ?? "",
                tgl = dtpTglLahir.Value.ToString("yyyy-MM-dd"),
                telp = txtTelp.Text,
                alamat = txtAlamat.Text,
                tanaman = txtTanaman.Text,
                id = idPetaniTerpilih
            });

            if (sukses)
            {
                MessageBox.Show("Data berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                BersihkanForm(); // Panggil fungsi clear form kamu
                LoadData("");    // Refresh tabel
                HitungTotalData();
            }
        }

        private void btnHapus_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(idPetaniTerpilih))
            {
                MessageBox.Show("Silakan klik salah satu data di tabel terlebih dahulu!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Tampilkan kotak dialog konfirmasi
            DialogResult dialog = MessageBox.Show("Apakah Anda yakin ingin menghapus data petani ini?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialog == DialogResult.Yes)
            {
                bool sukses = db.Execute("DELETE FROM petani WHERE id_petani=@id", new { id = idPetaniTerpilih });

                if (sukses)
                {
                    MessageBox.Show("Data berhasil dihapus!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    BersihkanForm(); // Panggil fungsi clear form kamu
                    LoadData("");    // Refresh tabel
                    HitungTotalData();
                }
            }
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = new DataTable();
                using (var conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT id_petani, nik, nama_petani, jenis_kelamin, tgl_lahir, no_telp, alamat, jenis_tanaman FROM petani";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        // Eksekusi DataReader sesuai instruksi soal
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader); // Memasukkan hasil bacaan DataReader ke DataTable
                        }
                    }
                }
                dgvPetani.DataSource = dt;
                HitungTotalData(); // Update label jumlah data
                MessageBox.Show("Data berhasil dimuat dengan DataReader!", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error baca data: " + ex.Message);
            }
        }


        private void HitungTotalData()
        {
            try
            {
                // Panggil GetConnection dari class Database kamu
                using (var conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM petani";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        // Eksekusi ExecuteScalar sesuai permintaan soal
                        int total = Convert.ToInt32(cmd.ExecuteScalar());
                        lblTotalRecord.Text = "Total Record: " + total.ToString() + " Petani";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menghitung data: " + ex.Message);
            }
        }

        private void txtNIK_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Hanya izinkan angka (Digit) dan tombol Backspace (Control)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Tolak inputan jika bukan angka
            }
        }

        private void txtTelp_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void btnSqlInjection_Click(object sender, EventArgs e)
        {
            // Tambahkan peringatan agar kamu tidak tidak sengaja mengkliknya
            DialogResult dialog = MessageBox.Show(
                "PERINGATAN: Ini akan mendemonstrasikan SQL Injection yang meretas database dan mengubah semua status game menjadi 'Tamat'. Lanjutkan eksekusi?",
                "Simulasi Serangan SQL Injection",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (dialog == DialogResult.Yes)
            {
                try
                {
                    // NOTE: Ganti connection string di bawah dengan connection string SQL Server yang valid
                    // Jika Anda menggunakan MySQL, gunakan MySqlConnection/MySqlCommand dan sesuaikan stored procedure/SQL.
                    using (var conn = new SqlConnection("Data Source=SERVER;Initial Catalog=DATABASE;Integrated Security=True;"))
                    using (var cmd = new SqlCommand("sp_SearchGame", conn))
                    {
                        conn.Open();

                        // Ini adalah Payload SQL Injection-nya
                        // Akan memotong query LIKE milik pencarian, menutupnya, lalu menyisipkan query UPDATE
                        string payload = "x%'; UPDATE Games SET status_main = 'Tamat'; --";

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@keyword", payload);

                        // Kita pakai ExecuteNonQuery karena payload berisi query UPDATE yang memanipulasi data
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show(
                        "💥 Serangan Berhasil! Semua status game telah diubah menjadi 'Tamat' secara paksa. Lihat perubahannya di tabel.",
                        "System Hacked",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);

                    // Refresh tabel untuk melihat kerusakan yang terjadi
                    btnRead.PerformClick();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal melakukan simulasi: " + ex.Message);
                }
            }
        }
    }
    }
