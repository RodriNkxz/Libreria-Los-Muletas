using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Libreria_Los_Muletas.Formularios;

namespace Libreria_Los_Muletas
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                Servicios.GestorBiblioteca.Instancia.CargarTodo();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar datos: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var login = new FrmLogin())
            {
                if (login.ShowDialog() != DialogResult.OK) return;
            }

            Application.Run(new FrmPrincipal());
        }
    }
}
