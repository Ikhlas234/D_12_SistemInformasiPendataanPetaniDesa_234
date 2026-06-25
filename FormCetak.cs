using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataPetaniDesa // Sesuaikan dengan namespace project-mu
{
    public partial class FormCetak : Form
    {
        // Ubah form ini agar wajib menerima kiriman DataTable saat dibuka
        public FormCetak(DataTable dtKiriman)
        {
            InitializeComponent();

            try
            {
                // 1. Panggil desain kertas laporan yang sudah kamu buat tadi
                rptPetani laporan = new rptPetani();

                // 2. Masukkan data yang difilter ke dalam laporan
                laporan.SetDataSource(dtKiriman);

                // 3. Tampilkan laporannya di layar (Viewer)
                crystalReportViewer1.ReportSource = laporan;
                crystalReportViewer1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat laporan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}