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

        private BindingSource bindingSourcePetani = new BindingSource();



        public FormPetani()
        {
            InitializeComponent();
            lblTotalRecord = this.Controls.Find("lblTotalRecord", true).FirstOrDefault() as Label;
            HitungTotalData();
        }


        // ==========================================
        // 1. TAMPIL & CARI DATA (READ)
        // ==========================================
        private void LoadData(string pencarian)
        {
            string query = "";
            DataTable dt;


            if (string.IsNullOrEmpty(pencarian))
            {
                // SYARAT UCP 2: Menggunakan VIEW untuk Select data
                query = "SELECT * FROM vw_data_petani";
                dt = db.FetchAll(query);
            }
            else
            {
                // SYARAT UCP 2: Menggunakan STORED PROCEDURE untuk Search
                query = "CALL sp_search_petani(@cari)";
                dt = db.FetchAll(query, new { cari = pencarian });
            }

            // SYARAT UCP 2: Memanfaatkan Binding DataGridView
            bindingSourcePetani.DataSource = dt;
            dgvPetani.DataSource = bindingSourcePetani;

            // Pastikan kamu sudah men-drag "BindingNavigator" dari Toolbox ke Form
            // Lalu ganti nama defaultnya atau sesuaikan dengan kode di bawah ini:
            bindingNavigator1.BindingSource = bindingSourcePetani;
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

            string query = "CALL sp_insert_petani(@nik, @nama, @jk, @tgl, @telp, @alamat, @tanaman)";

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
                if (dgvPetani.Columns.Contains("tanggal_lahir_format"))
                {
                    // Beri tahu C# bahwa format teks dari View adalah "hari-bulan-tahun" (dd-MM-yyyy)
                    string tglDariDatabase = row.Cells["tanggal_lahir_format"].Value.ToString();
                    dtpTglLahir.Value = DateTime.ParseExact(tglDariDatabase, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                }
                else if (dgvPetani.Columns.Contains("tgl_lahir"))
                {
                    // Jika kolom asli (tgl_lahir) yang terbaca
                    dtpTglLahir.Value = Convert.ToDateTime(row.Cells["tgl_lahir"].Value);
                }
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

            string query = "CALL sp_update_petani(@id, @nik, @nama, @jk, @tgl, @telp, @alamat, @tanaman)";

            bool sukses = db.Execute(query, new
            {    nik = txtNIK.Text,
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
               bool sukses = db.Execute("CALL sp_delete_petani(@id)", new { id = idPetaniTerpilih });

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
                    string query = "SELECT * FROM vw_data_petani";
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
            DialogResult dialog = MessageBox.Show(
         "PERINGATAN: Ini akan mendemonstrasikan SQL Injection tipe 'Data Leak'. Kita akan menipu database agar membocorkan SEMUA data tanpa terkecuali menggunakan trik OR 1=1. Lanjutkan?",
         "Simulasi Serangan SQL Injection",
         MessageBoxButtons.YesNo,
         MessageBoxIcon.Warning);

            if (dialog == DialogResult.Yes)
            {
                try
                {
                    // PAYLOAD SAKTI: Membuat logika WHERE menjadi selalu BENAR (TRUE)
                    string payloadSengajaRentan = "' OR '1'='1";

                    // Query yang rentan (String Concatenation)
                    string queryRentan = "SELECT * FROM vw_data_petani WHERE nama_petani = '" + payloadSengajaRentan + "'";

                    // Mengeksekusi query bajakan menggunakan class Database kamu
                    DataTable dtBocor = db.FetchAll(queryRentan);

                    // Memaksa tabel menampilkan data curian
                    dgvPetani.DataSource = dtBocor;

                    MessageBox.Show(
                        "💥 Serangan Berhasil! Logika database berhasil ditipu.\n\nQuery yang tereksekusi di belakang layar menjadi:\nSELECT * FROM vw_data_petani WHERE nama_petani = '' OR '1'='1'\n\nKarena 1 selalu sama dengan 1, database menyerahkan SEMUA datanya!",
                        "System Hacked",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: \n" + ex.Message);
                }
            }
        }

        private void btnResetData_Click(object sender, EventArgs e)
        {
            // Tambahkan konfirmasi agar tidak tidak sengaja ter-reset
            DialogResult dialog = MessageBox.Show("Kembalikan semua data petani ke kondisi semula?", "Konfirmasi Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialog == DialogResult.Yes)
            {
                try
                {
                    // Panggil koneksi menggunakan class jembatan database milikmu
                    using (MySqlConnection conn = db.GetConnection())
                    {
                        conn.Open();

                        // Query MySQL: Kosongkan tabel petani, lalu isi dari tabel petani_backup
                        // Kita gunakan TRUNCATE agar id_petani (AUTO_INCREMENT) kembali berurut dari angka 1
                        string query = @"
                    TRUNCATE TABLE petani;
                    
                    INSERT INTO petani (nik, nama_petani, jenis_kelamin, tgl_lahir, no_telp, alamat, jenis_tanaman)
                    SELECT nik, nama_petani, jenis_kelamin, tgl_lahir, no_telp, alamat, jenis_tanaman 
                    FROM petani_backup;";

                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Data berhasil direset! Kondisi database telah kembali normal.", "Reset Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Refresh tampilan tabel dan total record menggunakan fungsi yang sudah kamu buat
                    LoadData("");
                    HitungTotalData();
                }
                catch (Exception ex)
                {
                    // Pesan error akan muncul jika tabel petani_backup belum dibuat di MySQL
                    MessageBox.Show("Gagal mereset data. Pastikan tabel 'petani_backup' sudah dibuat di MySQL! \n\nDetail Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
    }
    
