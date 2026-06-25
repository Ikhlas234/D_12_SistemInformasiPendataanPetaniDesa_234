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
using System.IO;
using ExcelDataReader;

namespace DataPetaniDesa
{
    public partial class FormPetani : Form
    {
        private PetaniDAL petaniDAL = new PetaniDAL();
        private Database db = new Database();

        private string lokasiFoto = "";
        private string idPetaniTerpilih = "";
        private BindingSource bindingSourcePetani = new BindingSource();

        public FormPetani()
        {
            InitializeComponent();
            lblTotalRecord = this.Controls.Find("lblTotalRecord", true).FirstOrDefault() as Label;
            HitungTotalData();
            TampilGrafik();
        }

        // ==========================================
        // 1. TAMPIL & CARI DATA (READ) + RENDER GAMBAR CLOUD
        // ==========================================
        private void LoadData(string pencarian)
        {
            DataTable dt;

            if (string.IsNullOrEmpty(pencarian))
            {
                dt = petaniDAL.GetAllPetani();
            }
            else
            {
                dt = petaniDAL.SearchPetani(pencarian);
            }

            // === MAGIS: RENDER GAMBAR DARI URL KE DALAM DATATABLE ===
            // 1. Buat kolom virtual baru bertipe Image
            if (!dt.Columns.Contains("gambar_asli"))
            {
                dt.Columns.Add("gambar_asli", typeof(Image));
            }

            // 2. Loop setiap baris data untuk mendownload gambar dari link URL
            foreach (DataRow row in dt.Rows)
            {
                string urlFoto = row["foto"].ToString();
                if (!string.IsNullOrEmpty(urlFoto) && (urlFoto.StartsWith("http") || urlFoto.StartsWith("https")))
                {
                    try
                    {
                        // Download gambar dari internet secara langsung
                        System.Net.WebRequest request = System.Net.WebRequest.Create(urlFoto);
                        using (System.Net.WebResponse response = request.GetResponse())
                        using (System.IO.Stream stream = response.GetResponseStream())
                        {
                            Image img = Image.FromStream(stream);
                            // Perkecil ukuran gambar ke 100x100 pixel agar RAM komputer tidak jebol
                            row["gambar_asli"] = new Bitmap(img, new Size(100, 100));
                        }
                    }
                    catch
                    {
                        // Jika URL mati, typo, atau gagal didownload, biarkan sel kosong tanpa error
                    }
                }
            }

            // Atur tinggi baris agar gambar yang muncul tidak terpotong
            dgvPetani.RowTemplate.Height = 100;

            bindingSourcePetani.DataSource = dt;
            dgvPetani.DataSource = bindingSourcePetani;
            bindingNavigator1.BindingSource = bindingSourcePetani;

            // Percantik Tampilan Tabel
            if (dgvPetani.Columns.Contains("foto"))
            {
                dgvPetani.Columns["foto"].Visible = false; // Sembunyikan kolom teks URL yang panjang
            }
            if (dgvPetani.Columns.Contains("gambar_asli"))
            {
                DataGridViewImageColumn imgCol = (DataGridViewImageColumn)dgvPetani.Columns["gambar_asli"];
                imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom; // Agar proporsional
                imgCol.HeaderText = "Foto Petani"; // Judul Kolom
                imgCol.DisplayIndex = 0; // Pindahkan kolom foto ini ke urutan paling kiri!
            }
        }

        // ==========================================
        // 2. SIMPAN DATA (CREATE)
        // ==========================================
        private void btnSimpan_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNIK.Text) || string.IsNullOrWhiteSpace(txtNama.Text) || string.IsNullOrWhiteSpace(txtTelp.Text))
            {
                MessageBox.Show("Semua kolom wajib diisi! Tidak boleh ada yang kosong.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtNIK.Text.Length != 16)
            {
                MessageBox.Show("Format NIK salah! NIK harus berjumlah tepat 16 digit angka.", "Validasi Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtNIK.Focus();
                return;
            }

            if (txtTelp.Text.Length < 10)
            {
                MessageBox.Show("Nomor telepon tidak valid! Minimal harus 10 digit angka.", "Validasi Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtTelp.Focus();
                return;
            }

            try
            {
                petaniDAL.InsertPetani(
                    txtNIK.Text,
                    txtNama.Text,
                    cbJenisKelamin.SelectedItem?.ToString() ?? "",
                    dtpTglLahir.Value.ToString("yyyy-MM-dd"),
                    txtTelp.Text,
                    txtAlamat.Text,
                    cbTanaman.Text, // <-- Sekarang menggunakan Dropdown ComboBox cbTanaman
                    txtUrlFoto.Text // <-- Sekarang menyimpan langsung Link URL
                );

                MessageBox.Show("Data berhasil disimpan!");
                BersihkanForm();
                LoadData("");
                HitungTotalData();
                TampilGrafik();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menyimpan data: " + ex.Message);
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
                idPetaniTerpilih = row.Cells["id_petani"].Value.ToString();

                txtNIK.Text = row.Cells["nik"].Value.ToString();
                txtNama.Text = row.Cells["nama_petani"].Value.ToString();
                cbJenisKelamin.SelectedItem = row.Cells["jenis_kelamin"].Value.ToString();
                cbTanaman.Text = row.Cells["jenis_tanaman"].Value.ToString(); // Set data ke ComboBox

                // Ambil URL foto dari database
                string pathFoto = row.Cells["foto"].Value.ToString();
                txtUrlFoto.Text = pathFoto; // Tampilkan URL-nya di kotak teks
                lokasiFoto = pathFoto;

                // Tampilkan gambar ke PictureBox secara Asynchronous (Agar form tidak lag)
                if (!string.IsNullOrEmpty(pathFoto) && (pathFoto.StartsWith("http") || pathFoto.StartsWith("https")))
                {
                    try { pbFoto.LoadAsync(pathFoto); } catch { pbFoto.Image = null; }
                }
                else
                {
                    pbFoto.Image = null;
                }

                if (dgvPetani.Columns.Contains("tanggal_lahir_format"))
                {
                    string tglDariDatabase = row.Cells["tanggal_lahir_format"].Value.ToString();
                    dtpTglLahir.Value = DateTime.ParseExact(tglDariDatabase, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                }
                else if (dgvPetani.Columns.Contains("tgl_lahir"))
                {
                    dtpTglLahir.Value = Convert.ToDateTime(row.Cells["tgl_lahir"].Value);
                }
                txtTelp.Text = row.Cells["no_telp"].Value.ToString();
                txtAlamat.Text = row.Cells["alamat"].Value.ToString();
            }
        }

        // ==========================================
        // FITUR PREVIEW GAMBAR VIA URL
        // ==========================================
        private void btnUpload_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUrlFoto.Text))
            {
                MessageBox.Show("Silakan paste Link/URL gambar ke kotak URL terlebih dahulu!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // LoadAsync akan mendownload gambar tanpa membuat aplikasi macet/berhenti
                pbFoto.LoadAsync(txtUrlFoto.Text);
                lokasiFoto = txtUrlFoto.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat gambar dari URL tersebut. Pastikan link-nya valid dan berakhiran ekstensi gambar (.jpg/.png)\n\nError: " + ex.Message, "Error URL", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            idPetaniTerpilih = "";
            txtNIK.Clear();
            txtNama.Clear();
            txtTelp.Clear();
            txtAlamat.Clear();
            cbTanaman.SelectedIndex = -1; // Kosongkan ComboBox Tanaman
            txtUrlFoto.Clear(); // Kosongkan Input URL
            cbJenisKelamin.SelectedIndex = -1;
            dtpTglLahir.Value = DateTime.Now;

            lokasiFoto = "";
            if (pbFoto.Image != null)
            {
                pbFoto.Image.Dispose();
                pbFoto.Image = null;
            }
        }

        private void btnEdit_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(idPetaniTerpilih))
            {
                MessageBox.Show("Silakan klik salah satu data di tabel terlebih dahulu!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult dialog = MessageBox.Show("Apakah Anda yakin ingin menyimpan perubahan data ini?", "Konfirmasi Edit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialog == DialogResult.No)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNIK.Text) || string.IsNullOrWhiteSpace(txtNama.Text) || string.IsNullOrWhiteSpace(txtTelp.Text))
            {
                MessageBox.Show("Semua kolom wajib diisi! Tidak boleh ada yang kosong.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (txtNIK.Text.Length != 16)
            {
                MessageBox.Show("Format NIK salah! NIK harus berjumlah tepat 16 digit angka.", "Validasi Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtNIK.Focus();
                return;
            }
            if (txtTelp.Text.Length < 10)
            {
                MessageBox.Show("Nomor telepon tidak valid! Minimal harus 10 digit angka.", "Validasi Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtTelp.Focus();
                return;
            }

            try
            {
                petaniDAL.UpdatePetani(
                    idPetaniTerpilih,
                    txtNIK.Text,
                    txtNama.Text,
                    cbJenisKelamin.SelectedItem?.ToString() ?? "",
                    dtpTglLahir.Value.ToString("yyyy-MM-dd"),
                    txtTelp.Text,
                    txtAlamat.Text,
                    cbTanaman.Text, // <-- Menggunakan ComboBox
                    txtUrlFoto.Text // <-- Menyimpan Link URL Foto
                );

                MessageBox.Show("Data berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                BersihkanForm();
                LoadData("");
                HitungTotalData();
                TampilGrafik();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal mengedit data: " + ex.Message);
            }
        }

        private void btnHapus_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(idPetaniTerpilih))
            {
                MessageBox.Show("Silakan klik salah satu data di tabel terlebih dahulu!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult dialog = MessageBox.Show("Apakah Anda yakin ingin menghapus data petani ini?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialog == DialogResult.Yes)
            {
                try
                {
                    petaniDAL.DeletePetani(idPetaniTerpilih);
                    MessageBox.Show("Data berhasil dihapus!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    BersihkanForm();
                    LoadData("");
                    HitungTotalData();
                    TampilGrafik();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal menghapus data: " + ex.Message);
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
                        // Eksekusi DataReader sesuai instruksi tugas
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader); // Memasukkan hasil bacaan DataReader ke DataTable
                        }
                    }
                }

                // === TAMBAHKAN MAGIS RENDER GAMBAR DI SINI ===
                if (!dt.Columns.Contains("gambar_asli"))
                {
                    dt.Columns.Add("gambar_asli", typeof(Image));
                }

                foreach (DataRow row in dt.Rows)
                {
                    string urlFoto = row["foto"].ToString();
                    if (!string.IsNullOrEmpty(urlFoto) && (urlFoto.StartsWith("http") || urlFoto.StartsWith("https")))
                    {
                        try
                        {
                            System.Net.WebRequest request = System.Net.WebRequest.Create(urlFoto);
                            using (System.Net.WebResponse response = request.GetResponse())
                            using (System.IO.Stream stream = response.GetResponseStream())
                            {
                                Image img = Image.FromStream(stream);
                                row["gambar_asli"] = new Bitmap(img, new Size(100, 100)); // Perkecil gambar
                            }
                        }
                        catch { } // Abaikan jika link rusak
                    }
                }

                // Atur tinggi baris agar gambar muat
                dgvPetani.RowTemplate.Height = 100;
                // ==============================================

                dgvPetani.DataSource = dt;
                HitungTotalData(); // Update label jumlah data

                // PERCANTIK TABEL: Sembunyikan URL, Tampilkan Gambar
                if (dgvPetani.Columns.Contains("foto"))
                {
                    dgvPetani.Columns["foto"].Visible = false;
                }
                if (dgvPetani.Columns.Contains("gambar_asli"))
                {
                    DataGridViewImageColumn imgCol = (DataGridViewImageColumn)dgvPetani.Columns["gambar_asli"];
                    imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
                    imgCol.HeaderText = "Foto Petani";
                    imgCol.DisplayIndex = 0;
                }

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
                using (var conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM petani";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
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
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
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
                    string payloadSengajaRentan = "' OR '1'='1";
                    string queryRentan = "SELECT * FROM vw_data_petani WHERE nama_petani = '" + payloadSengajaRentan + "'";
                    DataTable dtBocor = db.FetchAll(queryRentan);
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
            DialogResult dialog = MessageBox.Show("Kembalikan semua data petani ke kondisi semula?", "Konfirmasi Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialog == DialogResult.Yes)
            {
                try
                {
                    using (MySqlConnection conn = db.GetConnection())
                    {
                        conn.Open();
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
                    LoadData("");
                    HitungTotalData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal mereset data. Pastikan tabel 'petani_backup' sudah dibuat di MySQL! \n\nDetail Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void TampilGrafik()
        {
            try
            {
                DataTable dtStatistik = petaniDAL.GetStatistikTanaman();

                chartTanaman.Series.Clear();
                chartTanaman.Series.Add("Jumlah Petani");
                chartTanaman.DataSource = dtStatistik;
                chartTanaman.Series["Jumlah Petani"].XValueMember = "jenis_tanaman";
                chartTanaman.Series["Jumlah Petani"].YValueMembers = "jumlah";
                chartTanaman.Series["Jumlah Petani"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
                chartTanaman.DataBind();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat grafik: " + ex.Message);
            }
        }

        private void btnImportExcel_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
            ofd.Title = "Pilih Data Excel Petani";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                    using (var stream = File.Open(ofd.FileName, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                            {
                                ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                            });

                            DataTable dtExcel = result.Tables[0];

                            foreach (DataRow row in dtExcel.Rows)
                            {
                                string nik = row[0].ToString();
                                if (!string.IsNullOrEmpty(nik))
                                {
                                    string nama = row[1].ToString();
                                    string jk = row[2].ToString();

                                    string tgl = "";
                                    if (DateTime.TryParse(row[3].ToString(), out DateTime parsedDate))
                                    {
                                        tgl = parsedDate.ToString("yyyy-MM-dd");
                                    }

                                    string telp = row[4].ToString();
                                    string alamat = row[5].ToString();
                                    string tanaman = row[6].ToString(); // Excel akan dibaca sebagai string untuk ComboBox
                                    string foto = "";

                                    petaniDAL.InsertPetani(nik, nama, jk, tgl, telp, alamat, tanaman, foto);
                                }
                            }
                        }
                    }

                    MessageBox.Show("Import data dari Excel berhasil diselesaikan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData("");
                    HitungTotalData();
                    TampilGrafik();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal mengimport Excel. Pastikan file Excel sedang TIDAK DIBUKA di aplikasi lain. \n\nDetail Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnRekap_Click(object sender, EventArgs e)
        {
            FormRekap frm = new FormRekap();
            frm.Show();
            this.Hide();
        }
    }
}
    
