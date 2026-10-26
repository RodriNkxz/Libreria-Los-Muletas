using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Libreria_Los_Muletas.Enums;
using Libreria_Los_Muletas.Modelos;

namespace Libreria_Los_Muletas.Formularios
{
    public class FrmPrestamos : Form
    {
        private DataGridView gridPrestamos, gridReservas;
        private ComboBox cmbUsuarios, cmbLibros;
        private NumericUpDown nudDias;
        private TabControl tabs;

        public FrmPrestamos()
        {
            this.Text = "PGB - Préstamos y Reservas";
            this.Size = new Size(1000, 640);
            this.StartPosition = FormStartPosition.CenterParent;

            tabs = new TabControl { Dock = DockStyle.Fill };

            // ================= TAB PRÉSTAMOS =================
            var tabPrestamos = new TabPage("Préstamos");

            var grpNuevo = new GroupBox
            {
                Text = "Registrar nuevo préstamo",
                Location = new Point(10, 10),
                Size = new Size(960, 90)
            };

            grpNuevo.Controls.Add(new Label { Text = "Socio:", Location = new Point(15, 30), Size = new Size(50, 22) });
            cmbUsuarios = new ComboBox
            {
                Location = new Point(70, 28),
                Size = new Size(280, 24),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            grpNuevo.Controls.Add(cmbUsuarios);

            grpNuevo.Controls.Add(new Label { Text = "Libro:", Location = new Point(365, 30), Size = new Size(50, 22) });
            cmbLibros = new ComboBox
            {
                Location = new Point(420, 28),
                Size = new Size(350, 24),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            grpNuevo.Controls.Add(cmbLibros);

            grpNuevo.Controls.Add(new Label { Text = "Días:", Location = new Point(785, 30), Size = new Size(40, 22) });
            nudDias = new NumericUpDown
            {
                Location = new Point(825, 28),
                Size = new Size(60, 24),
                Minimum = 1,
                Maximum = 90,
                Value = 7
            };
            grpNuevo.Controls.Add(nudDias);

            var btnPrestar = new Button
            {
                Text = "Registrar préstamo",
                Location = new Point(15, 55),
                Size = new Size(150, 28),
                BackColor = Color.LightGreen
            };
            btnPrestar.Click += BtnPrestar_Click;
            grpNuevo.Controls.Add(btnPrestar);

            gridPrestamos = new DataGridView
            {
                Location = new Point(10, 110),
                Size = new Size(960, 420),
                ReadOnly = true,
                AllowUserToAddRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            gridPrestamos.Columns.Add("Id", "ID");
            gridPrestamos.Columns.Add("Libro", "Libro");
            gridPrestamos.Columns.Add("Socio", "Socio");
            gridPrestamos.Columns.Add("FPrestamo", "Fecha préstamo");
            gridPrestamos.Columns.Add("FVenc", "Fecha vencimiento");
            gridPrestamos.Columns.Add("Estado", "Estado");
            gridPrestamos.Columns.Add("Renovado", "Renovado");
            gridPrestamos.Columns.Add("DiasRetraso", "Días retraso");

            var btnDevolver = new Button { Text = "Devolver", Location = new Point(10, 540), Size = new Size(120, 30) };
            btnDevolver.Click += BtnDevolver_Click;

            var btnRenovar = new Button { Text = "Renovar (+7 días)", Location = new Point(140, 540), Size = new Size(150, 30) };
            btnRenovar.Click += BtnRenovar_Click;

            var btnRefrescar = new Button { Text = "Refrescar", Location = new Point(300, 540), Size = new Size(100, 30) };
            btnRefrescar.Click += (s, e) => { CargarCombos(); CargarGridPrestamos(); };

            tabPrestamos.Controls.Add(grpNuevo);
            tabPrestamos.Controls.Add(gridPrestamos);
            tabPrestamos.Controls.Add(btnDevolver);
            tabPrestamos.Controls.Add(btnRenovar);
            tabPrestamos.Controls.Add(btnRefrescar);

            // ================= TAB RESERVAS =================
            var tabReservas = new TabPage("Reservas");

            gridReservas = new DataGridView
            {
                Location = new Point(10, 10),
                Size = new Size(960, 500),
                ReadOnly = true,
                AllowUserToAddRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            gridReservas.Columns.Add("Id", "ID");
            gridReservas.Columns.Add("Libro", "Libro");
            gridReservas.Columns.Add("Socio", "Socio");
            gridReservas.Columns.Add("Fecha", "Fecha reserva");
            gridReservas.Columns.Add("Estado", "Estado");
            gridReservas.Columns.Add("DisponibleHasta", "Disponible hasta");

            var btnCancelarReserva = new Button { Text = "Cancelar reserva", Location = new Point(10, 520), Size = new Size(150, 30) };
            btnCancelarReserva.Click += BtnCancelarReserva_Click;

            var btnRefrescarRes = new Button { Text = "Refrescar", Location = new Point(170, 520), Size = new Size(100, 30) };
            btnRefrescarRes.Click += (s, e) => CargarGridReservas();

            tabReservas.Controls.Add(gridReservas);
            tabReservas.Controls.Add(btnCancelarReserva);
            tabReservas.Controls.Add(btnRefrescarRes);

            tabs.TabPages.Add(tabPrestamos);
            tabs.TabPages.Add(tabReservas);

            this.Controls.Add(tabs);

            CargarCombos();
            CargarGridPrestamos();
            CargarGridReservas();
        }

        private void CargarCombos()
        {
            var g = Servicios.GestorBiblioteca.Instancia;

            cmbUsuarios.Items.Clear();
            foreach (var u in g.Usuarios.Where(x => x.Activo).OrderBy(x => x.Nombre))
                cmbUsuarios.Items.Add(new ComboItem(u.Id, u.MostrarResumen()));

            cmbLibros.Items.Clear();
            foreach (var l in g.Libros.OrderBy(x => x.Titulo))
                cmbLibros.Items.Add(new ComboItem(l.Id, l.ToString()));
        }

        private void CargarGridPrestamos()
        {
            var g = Servicios.GestorBiblioteca.Instancia;
            gridPrestamos.Rows.Clear();

            foreach (var p in g.Prestamos.OrderByDescending(x => x.FechaPrestamo))
            {
                var libro = g.Libros.Find(l => l.Id == p.IdLibro);
                var socio = g.Usuarios.Find(u => u.Id == p.IdUsuario);

                string estado = p.Estado.ToString();
                if (p.Estado == EstadoPrestamo.Activo && p.EstaVencido) estado = "VENCIDO";

                int fila = gridPrestamos.Rows.Add(
                    p.Id,
                    libro != null ? libro.Titulo : "(eliminado)",
                    socio != null ? socio.NombreCompleto : "(eliminado)",
                    p.FechaPrestamo.ToString("dd/MM/yyyy"),
                    p.FechaVencimiento.ToString("dd/MM/yyyy"),
                    estado,
                    p.Renovado ? "Sí" : "No",
                    p.Estado == EstadoPrestamo.Activo ? p.CalcularDiasRetraso() : 0);

                if (p.Estado == EstadoPrestamo.Activo && p.EstaVencido)
                    gridPrestamos.Rows[fila].DefaultCellStyle.BackColor = Color.LightCoral;
                else if (p.Estado == EstadoPrestamo.Devuelto)
                    gridPrestamos.Rows[fila].DefaultCellStyle.BackColor = Color.LightGray;
            }
        }

        private void CargarGridReservas()
        {
            var g = Servicios.GestorBiblioteca.Instancia;
            gridReservas.Rows.Clear();

            foreach (var r in g.Reservas.OrderByDescending(x => x.FechaReserva))
            {
                var libro = g.Libros.Find(l => l.Id == r.IdLibro);
                var socio = g.Usuarios.Find(u => u.Id == r.IdUsuario);

                gridReservas.Rows.Add(
                    r.Id,
                    libro != null ? libro.Titulo : "(eliminado)",
                    socio != null ? socio.NombreCompleto : "(eliminado)",
                    r.FechaReserva.ToString("dd/MM/yyyy HH:mm"),
                    r.Estado.ToString(),
                    r.FechaDisponible.HasValue
                        ? r.FechaDisponible.Value.AddHours(24).ToString("dd/MM/yyyy HH:mm")
                        : "-");
            }
        }

        private void BtnPrestar_Click(object sender, EventArgs e)
        {
            try
            {
                var g = Servicios.GestorBiblioteca.Instancia;
                var itemU = cmbUsuarios.SelectedItem as ComboItem;
                var itemL = cmbLibros.SelectedItem as ComboItem;

                if (itemU == null || itemL == null)
                {
                    MessageBox.Show("Seleccione un socio y un libro.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var usuario = g.Usuarios.Find(u => u.Id == itemU.Id);
                var libro = g.Libros.Find(l => l.Id == itemL.Id);
                int dias = (int)nudDias.Value;

                var p = g.RegistrarPrestamo(usuario, libro, dias);
                MessageBox.Show("Préstamo registrado. Vence el " + p.FechaVencimiento.ToString("dd/MM/yyyy"),
                    "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);

                CargarCombos();
                CargarGridPrestamos();
                CargarGridReservas();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No se pudo prestar",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                CargarCombos();
                CargarGridPrestamos();
                CargarGridReservas();
            }
        }

        private void BtnDevolver_Click(object sender, EventArgs e)
        {
            if (gridPrestamos.SelectedRows.Count == 0) return;
            int id = Convert.ToInt32(gridPrestamos.SelectedRows[0].Cells["Id"].Value);

            try
            {
                Servicios.GestorBiblioteca.Instancia.Devolver(id);
                MessageBox.Show("Devolución registrada.", "OK",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarCombos();
                CargarGridPrestamos();
                CargarGridReservas();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnRenovar_Click(object sender, EventArgs e)
        {
            if (gridPrestamos.SelectedRows.Count == 0) return;
            int id = Convert.ToInt32(gridPrestamos.SelectedRows[0].Cells["Id"].Value);

            try
            {
                Servicios.GestorBiblioteca.Instancia.Renovar(id);
                MessageBox.Show("Préstamo renovado por 7 días más.", "OK",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarGridPrestamos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnCancelarReserva_Click(object sender, EventArgs e)
        {
            if (gridReservas.SelectedRows.Count == 0) return;
            int id = Convert.ToInt32(gridReservas.SelectedRows[0].Cells["Id"].Value);

            try
            {
                Servicios.GestorBiblioteca.Instancia.CancelarReserva(id);
                MessageBox.Show("Reserva cancelada.", "OK",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarGridReservas();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Clase auxiliar para ComboBox con ID + texto
        private class ComboItem
        {
            public int Id { get; private set; }
            public string Texto { get; private set; }
            public ComboItem(int id, string texto) { Id = id; Texto = texto; }
            public override string ToString() { return Texto; }
        }
    }
}
