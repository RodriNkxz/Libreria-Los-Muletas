using System;
using System.Drawing;
using System.Windows.Forms;


namespace Libreria_Los_Muletas.Formularios
{
    public class FrmLogin : Form
    {
        private TextBox txtUsuario, txtPassword;
        private Label lblError;

        public FrmLogin()
        {
            this.Text = "PGB - Iniciar sesión";
            this.Size = new Size(360, 250);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lblTitulo = new Label
            {
                Text = "Gestor de Biblioteca",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 15),
                Size = new Size(300, 30)
            };

            var lblUsuario = new Label { Text = "Usuario:", Location = new Point(40, 65), Size = new Size(80, 25) };
            txtUsuario = new TextBox { Location = new Point(130, 65), Size = new Size(180, 25) };

            var lblPassword = new Label { Text = "Contraseña:", Location = new Point(40, 105), Size = new Size(80, 25) };
            txtPassword = new TextBox
            {
                Location = new Point(130, 105),
                Size = new Size(180, 25),
                UseSystemPasswordChar = true
            };

            var btnLogin = new Button { Text = "Ingresar", Location = new Point(130, 145), Size = new Size(85, 30) };
            btnLogin.Click += BtnLogin_Click;

            var btnCancelar = new Button
            {
                Text = "Cancelar",
                Location = new Point(225, 145),
                Size = new Size(85, 30),
                DialogResult = DialogResult.Cancel
            };

            lblError = new Label { ForeColor = Color.Red, Location = new Point(40, 182), Size = new Size(280, 20), Text = "" };

            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblUsuario);
            this.Controls.Add(txtUsuario);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnLogin);
            this.Controls.Add(btnCancelar);
            this.Controls.Add(lblError);

            this.AcceptButton = btnLogin;
            this.CancelButton = btnCancelar;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string u = txtUsuario.Text.Trim();
            string p = txtPassword.Text;
            if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(p))
            {
                lblError.Text = "Ingrese usuario y contraseña.";
                return;
            }
            try
            {
                if (Servicios.GestorBiblioteca.Instancia.Login(u, p))
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    lblError.Text = "Credenciales incorrectas.";
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
            catch (Exception ex) { lblError.Text = ex.Message; }
        }
    }
}
