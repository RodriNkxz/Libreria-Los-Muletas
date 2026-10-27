using Libreria_Los_Muletas.Enums;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Libreria_Los_Muletas.Formularios
{
    public class FrmPrincipal : Form
    {
        private Label lblInfo;
        private ToolStripStatusLabel statusLabel;

        public FrmPrincipal()
        {
            this.Text = "PGB - Menú principal";
            this.Size = new Size(760, 520);
            this.StartPosition = FormStartPosition.CenterScreen;

            var menu = new MenuStrip();

            var mnuArchivo = new ToolStripMenuItem("Archivo");
            var mnuSalir = new ToolStripMenuItem("Salir");
            mnuSalir.Click += (s, e) => this.Close();
            mnuArchivo.DropDownItems.Add(mnuSalir);

            var mnuGestion = new ToolStripMenuItem("Gestión");
            var mnuLibros = new ToolStripMenuItem("Libros");
            mnuLibros.Click += (s, e) => AbrirFormulario<FrmLibros>();
            var mnuUsuarios = new ToolStripMenuItem("Usuarios");
            mnuUsuarios.Click += (s, e) => AbrirFormulario<FrmUsuarios>();
            var mnuPrestamos = new ToolStripMenuItem("Préstamos y Reservas");
            mnuPrestamos.Click += (s, e) => AbrirFormulario<FrmPrestamos>();
            mnuGestion.DropDownItems.AddRange(new ToolStripItem[] { mnuLibros, mnuUsuarios, mnuPrestamos });

            var mnuReportes = new ToolStripMenuItem("Reportes");
            mnuReportes.Click += (s, e) => AbrirFormulario<FrmReportes>();

            menu.Items.Add(mnuArchivo);
            menu.Items.Add(mnuGestion);
            menu.Items.Add(mnuReportes);
            this.MainMenuStrip = menu;

            var lblBienvenida = new Label
            {
                Text = "Bienvenido al Gestor de Biblioteca Los Muletas",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 60),
                Size = new Size(740, 40)
            };

            lblInfo = new Label
            {
                Font = new Font("Segoe UI", 11),
                Location = new Point(60, 130),
                Size = new Size(620, 240),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(15)
            };

            var btnRefrescar = new Button
            {
                Text = "Refrescar estado",
                Location = new Point(60, 390),
                Size = new Size(150, 32)
            };
            btnRefrescar.Click += (s, e) => RefrescarEstado();

            var statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel("Listo");
            statusStrip.Items.Add(statusLabel);

            this.Controls.Add(menu);
            this.Controls.Add(lblBienvenida);
            this.Controls.Add(lblInfo);
            this.Controls.Add(btnRefrescar);
            this.Controls.Add(statusStrip);

            RefrescarEstado();
        }

        private void AbrirFormulario<T>() where T : Form, new()
        {
            try
            {
                var form = new T();
                form.ShowDialog(this);
                RefrescarEstado();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir formulario: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefrescarEstado()
        {
            try
            {
                var g = Servicios.GestorBiblioteca.Instancia;
                g.ActualizarEstadoUsuarios();
                g.ProcesarReservasExpiradas();

                int ejemplares = 0;
                foreach (var l in g.Libros) ejemplares += l.EjemplaresDisponibles;

                int sociosActivos = 0;
                foreach (var u in g.Usuarios) if (u.Activo) sociosActivos++;

                int activos = 0, vencidos = 0;
                foreach (var p in g.Prestamos)
                    if (p.Estado == EstadoPrestamo.Activo)
                    {
                        activos++;
                        if (p.EstaVencido) vencidos++;
                    }

                int reservasPend = 0;
                foreach (var r in g.Reservas)
                    if (r.Estado == EstadoReserva.Pendiente || r.Estado == EstadoReserva.Disponible)
                        reservasPend++;

                string usuario = g.SesionActual != null ? g.SesionActual.NombreCompleto : "(sin sesión)";

                lblInfo.Text =
                    "Usuario activo: " + usuario + Environment.NewLine + Environment.NewLine +
                    "Libros registrados: " + g.Libros.Count + " (ejemplares disponibles: " + ejemplares + ")" + Environment.NewLine +
                    "Socios: " + g.Usuarios.Count + " (" + sociosActivos + " activos)" + Environment.NewLine +
                    "Préstamos activos: " + activos + " (vencidos: " + vencidos + ")" + Environment.NewLine +
                    "Reservas pendientes o disponibles: " + reservasPend;

                statusLabel.Text = "Estado actualizado — " + DateTime.Now.ToString("HH:mm:ss");
            }
            catch (Exception ex) { statusLabel.Text = "Error: " + ex.Message; }
        }
    }
}
