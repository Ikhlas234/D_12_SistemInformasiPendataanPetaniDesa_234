using DataPetaniDesa;
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

namespace DataPetaniDesa
{
    public partial class FormLogin : Form
    {
        // Panggil jembatan database
        private Database db = new Database();

        public FormLogin()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtPassword.Text))
            {
                MessageBox.Show("Username dan Password tidak boleh kosong!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // CEK LOGIN DENGAN CLASS DATABASE
            var admin = db.FetchOne(
                "SELECT * FROM admin WHERE username=@u AND password=@p",
                new { u = txtUsername.Text, p = txtPassword.Text }
            );

            if (admin != null) // Jika data ditemukan
            {
                //MessageBox.Show("Login Berhasil!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                FormPetani formPetani = new FormPetani();
                formPetani.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Username atau Password salah!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormLogin_Load(object sender, EventArgs e)
        {
            // Menggunakan fungsi TestConnection yang sudah kita buat di Database.cs
            if (db.TestConnection())
            {
                lblStatusKoneksi.Text = "Status: Terhubung ke Database ✅";
                lblStatusKoneksi.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                lblStatusKoneksi.Text = "Status: Gagal Terhubung ❌";
                lblStatusKoneksi.ForeColor = System.Drawing.Color.Red;
            }
        }
    }
}

