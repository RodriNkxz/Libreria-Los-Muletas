using System;
using System.Drawing;
using System.Windows.Forms;
using Libreria_Los_Muletas.Modelos;

namespace Libreria_Los_Muletas.Formularios
{
    public class FrmUsuarios : Form
    {
        private DataGridView grid;
        private TextBox txtNombre, txtApellido, txtEmail, txtBusqueda;
        private CheckBox chkActivo;
        private int? _idEditando;

        public FrmUsuarios()
        {
            this.Text = "PGB - Gestión de Socios";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            var lblBuscar = new Label { Text = "Buscar:", Location = new Point(10, 15), Size = new Size(55, 22) };
            txtBusqueda = new TextBox { Location = new Point(70, 12), Size = new Size(300, 24) };
            txtBusqueda.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; BtnBuscar_Click(null, null); } };
            var btnBuscar = new Button { Text = "Buscar", Location = new Point(380, 11), Size = new Size(80, 26) };
            btnBuscar.Click += BtnBuscar_Click;
            var btnTodos = new Button { Text = "Mostrar todos", Location = new Point(465, 11), Size = new Size(110, 26) };
            btnTodos.Click += (s, e) => CargarGrid(null);

            grid = new DataGridView
            {
                Location = new Point(10, 45),
                Size = new Size(865, 320),
                ReadOnly = true,
                AllowUserToAddRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            grid.Columns.Add("Id", "ID");
            grid.Columns.Add("Nombre", "Nombre completo");
            grid.Columns.Add("Email", "Email");
            grid.Columns.Add("Activo", "Activo");
            grid.Columns.Add("FechaAlta", "Alta");
            grid.Columns.Add("Activos", "Préstamos activos");
            grid.Columns.Add("Moroso", "Moroso");
            grid.SelectionChanged += Grid_SelectionChanged;

            var grp = new GroupBox { Text = "Datos del socio", Location = new Point(10, 375), Size = new Size(865, 160) };

            grp.Controls.Add(new Label { Text = "Nombre:", Location = new Point(15, 30), Size = new Size(60, 22) });
            txtNombre = new TextBox { Location = new Point(80, 28), Size = new Size(280, 24) };
            grp.Controls.Add(txtNombre);

            grp.Controls.Add(new Label { Text = "Apellido:", Location = new Point(375, 30), Size = new Size(60, 22) });
            txtApellido = new TextBox { Location = new Point(440, 28), Size = new Size(280, 24) };
            grp.Controls.Add(txtApellido);

            grp.Controls.Add(new Label { Text = "Email:", Location = new Point(15, 65), Size = new Size(60, 22) });
            txtEmail = new TextBox { Location = new Point(80, 63), Size = new Size(280, 24) };
            grp.Controls.Add(txtEmail);

            chkActivo = new CheckBox { Text = "Activo", Location = new Point(440, 65), Size = new Size(80, 24), Checked = true };
            grp.Controls.Add(chkActivo);

            var btnNuevo = new Button { Text = "Nuevo", Location = new Point(15, 115), Size = new Size(90, 28) };
            btnNuevo.Click += (s, e) => LimpiarFormulario();
            grp.Controls.Add(btnNuevo);

            var btnGuardar = new Button { Text = "Guardar", Location = new Point(115, 115), Size = new Size(90, 28) };
            btnGuardar.Click += BtnGuardar_Click;
            grp.Controls.Add(btnGuardar);

            var btnEliminar = new Button { Text = "Eliminar", Location = new Point(215, 115), Size = new Size(90, 28) };
            btnEliminar.Click += BtnEliminar_Click;
            grp.Controls.Add(btnEliminar);

            this.Controls.Add(lblBuscar);
            this.Controls.Add(txtBusqueda);
            this.Controls.Add(btnBuscar);
            this.Controls.Add(btnTodos);
            this.Controls.Add(grid);
            this.Controls.Add(grp);

            CargarGrid(null);
            LimpiarFormulario();
        }

        private void CargarGrid(System.Collections.Generic.List<Usuario> usuarios)
        {
            var g = Servicios.GestorBiblioteca.Instancia;
            g.ActualizarEstadoUsuarios();
            var lista = usuarios ?? g.Usuarios;

            grid.Rows.Clear();
            foreach (var u in lista)
                grid.Rows.Add(u.Id, u.NombreCompleto, u.Email,
                              u.Activo ? "Sí" : "No",
                              u.FechaAlta.ToString("dd/MM/yyyy"),
                              u.PrestamosActivos,
                              u.EsMoroso ? "Sí" : "No");
        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            CargarGrid(Servicios.GestorBiblioteca.Instancia.BuscarUsuarios(txtBusqueda.Text));
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0) return;
            var row = grid.SelectedRows[0];
            if (row.Cells["Id"].Value == null) return;

            int id = Convert.ToInt32(row.Cells["Id"].Value);
            var u = Servicios.GestorBiblioteca.Instancia.Usuarios.Find(x => x.Id == id);
            if (u == null) return;

            _idEditando = u.Id;
            txtNombre.Text = u.Nombre;
            txtApellido.Text = u.Apellido;
            txtEmail.Text = u.Email;
            chkActivo.Checked = u.Activo;
        }

        private void LimpiarFormulario()
        {
            _idEditando = null;
            txtNombre.Text = ""; txtApellido.Text = ""; txtEmail.Text = "";
            chkActivo.Checked = true;
            grid.ClearSelection();
            txtNombre.Focus();
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                var g = Servicios.GestorBiblioteca.Instancia;
                if (_idEditando.HasValue)
                {
                    var u = new Usuario
                    {
                        Id = _idEditando.Value,
                        Nombre = txtNombre.Text,
                        Apellido = txtApellido.Text,
                        Email = txtEmail.Text,
                        Activo = chkActivo.Checked
                    };
                    g.ActualizarUsuario(u);
                    MessageBox.Show("Socio actualizado.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    var u = new Usuario(0, txtNombre.Text, txtApellido.Text, txtEmail.Text)
                    {
                        Activo = chkActivo.Checked
                    };
                    g.AgregarUsuario(u);
                    MessageBox.Show("Socio agregado.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                CargarGrid(null);
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (!_idEditando.HasValue) return;
            if (MessageBox.Show("¿Eliminar este socio?", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                Servicios.GestorBiblioteca.Instancia.EliminarUsuario(_idEditando.Value);
                CargarGrid(null);
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
