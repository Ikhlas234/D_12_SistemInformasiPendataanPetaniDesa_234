using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySqlConnector;
using DataPetaniDesa;

namespace DataPetaniDesa
{
    public partial class FormRekap : Form
    {
        private PetaniDAL petaniDAL = new PetaniDAL();
        public FormRekap()
        {
            InitializeComponent();
        }

        private void FormRekap_Load(object sender, EventArgs e)
        {
            // Atur nilai default ComboBox saat form pertama kali dibuka
            cbFilterKelamin.SelectedItem = "Semua";
            cbFilterTanaman.SelectedItem = "Semua";

            
        }

        private void LoadDataRekap()
        {
            try
            {
                // Ambil teks dari ComboBox (jika kosong, anggap "Semua")
                string filterKelamin = string.IsNullOrWhiteSpace(cbFilterKelamin.Text) ? "Semua" : cbFilterKelamin.Text;
                string filterTanaman = string.IsNullOrWhiteSpace(cbFilterTanaman.Text) ? "Semua" : cbFilterTanaman.Text;

                // Panggil fungsi pintar dari DAL
                DataTable dtRekap = petaniDAL.GetRekapPetani(filterKelamin, filterTanaman);

                // Tampilkan ke tabel
                dgvRekap.DataSource = dtRekap;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat rekap data: " + ex.Message);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            // Panggil fungsi rekap saat tombol LOAD diklik
            LoadDataRekap();
        }

        private void btnCetak_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Ambil nilai filter yang sedang dipilih saat ini
                string filterKelamin = string.IsNullOrWhiteSpace(cbFilterKelamin.Text) ? "Semua" : cbFilterKelamin.Text;
                string filterTanaman = string.IsNullOrWhiteSpace(cbFilterTanaman.Text) ? "Semua" : cbFilterTanaman.Text;

                // 2. Panggil DAL untuk mengambil datanya dari database (sesuai filter)
                DataTable dtLaporan = petaniDAL.GetRekapPetani(filterKelamin, filterTanaman);

                // Cek apakah datanya ada atau kosong
                if (dtLaporan.Rows.Count == 0)
                {
                    MessageBox.Show("Tidak ada data yang bisa dicetak dengan filter tersebut!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 3. Buka FormCetak dan lemparkan datanya ke sana!
                FormCetak frmCetak = new FormCetak(dtLaporan);
                frmCetak.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan saat menyiapkan cetak: " + ex.Message);
            }
        }
    }
}

