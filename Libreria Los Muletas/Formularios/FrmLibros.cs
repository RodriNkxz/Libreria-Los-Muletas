using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Libreria_Los_Muletas.Modelos;

namespace Libreria_Los_Muletas.Formularios
{
    public class FrmLibros : Form
    {
        private DataGridView grid;
        private TextBox txtBusqueda, txtTitulo, txtAutor, txtIsbn, txtAnio, txtGenero, txtEjemplares;
        private ComboBox cmbTipoBusqueda;
        private int? _idEditando;

        public FrmLibros()
        {
            this.Text = "PGB - Gestión de Libros";
            this.Size = new Size(960, 640);
            this.StartPosition = FormStartPosition.CenterParent;

            // ----- Buscador -----
            var lblBuscar = new Label { Text = "Buscar:", Location = new Point(10, 15), Size = new Size(55, 22) };
            cmbTipoBusqueda = new ComboBox
            {
                Location = new Point(65, 12),
                Size = new Size(160, 24),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbTipoBusqueda.Items.AddRange(new object[]
            {
                "Título o autor", "ISBN exacto", "Rango de años"
            });
            cmbTipoBusqueda.SelectedIndex = 0;

            txtBusqueda = new TextBox { Location = new Point(235, 12), Size = new Size(300, 24) };
            txtBusqueda.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; BtnBuscar_Click(null, null); } };

            var btnBuscar = new Button { Text = "Buscar", Location = new Point(545, 11), Size = new Size(80, 26) };
            btnBuscar.Click += BtnBuscar_Click;

            var btnRefrescar = new Button { Text = "Mostrar todos", Location = new Point(630, 11), Size = new Size(110, 26) };
            btnRefrescar.Click += (s, e) => CargarGrid(null);

            // ----- Grid -----
            grid = new DataGridView
            {
                Location = new Point(10, 45),
                Size = new Size(925, 340),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            grid.Columns.Add("Id", "ID");
            grid.Columns.Add("Titulo", "Título");
            grid.Columns.Add("Autor", "Autor");
            grid.Columns.Add("Isbn", "ISBN");
            grid.Columns.Add("Anio", "Año");
            grid.Columns.Add("Genero", "Género");
            grid.Columns.Add("Disp", "Disponibles");
            grid.Columns.Add("Total", "Total");
            grid.SelectionChanged += Grid_SelectionChanged;

            // ----- Formulario de edición -----
            var grpDatos = new GroupBox
            {
                Text = "Datos del libro",
                Location = new Point(10, 395),
                Size = new Size(925, 170)
            };

            grpDatos.Controls.Add(new Label { Text = "Título:", Location = new Point(15, 30), Size = new Size(60, 22) });
            txtTitulo = new TextBox { Location = new Point(80, 28), Size = new Size(300, 24) };
            grpDatos.Controls.Add(txtTitulo);

            grpDatos.Controls.Add(new Label { Text = "Autor:", Location = new Point(400, 30), Size = new Size(50, 22) });
            txtAutor = new TextBox { Location = new Point(455, 28), Size = new Size(300, 24) };
            grpDatos.Controls.Add(txtAutor);

            grpDatos.Controls.Add(new Label { Text = "ISBN:", Location = new Point(15, 65), Size = new Size(60, 22) });
            txtIsbn = new TextBox { Location = new Point(80, 63), Size = new Size(300, 24) };
            grpDatos.Controls.Add(txtIsbn);

            grpDatos.Controls.Add(new Label { Text = "Año:", Location = new Point(400, 65), Size = new Size(50, 22) });
            txtAnio = new TextBox { Location = new Point(455, 63), Size = new Size(100, 24) };
            grpDatos.Controls.Add(txtAnio);

            grpDatos.Controls.Add(new Label { Text = "Género:", Location = new Point(580, 65), Size = new Size(60, 22) });
            txtGenero = new TextBox { Location = new Point(645, 63), Size = new Size(110, 24) };
            grpDatos.Controls.Add(txtGenero);

            grpDatos.Controls.Add(new Label { Text = "Ejemplares:", Location = new Point(15, 100), Size = new Size(70, 22) });
            txtEjemplares = new TextBox { Location = new Point(90, 98), Size = new Size(80, 24) };
            grpDatos.Controls.Add(txtEjemplares);

            var btnNuevo = new Button { Text = "Nuevo", Location = new Point(15, 135), Size = new Size(90, 28) };
            btnNuevo.Click += BtnNuevo_Click;
            grpDatos.Controls.Add(btnNuevo);

            var btnGuardar = new Button { Text = "Guardar", Location = new Point(115, 135), Size = new Size(90, 28) };
            btnGuardar.Click += BtnGuardar_Click;
            grpDatos.Controls.Add(btnGuardar);

            var btnEliminar = new Button { Text = "Eliminar", Location = new Point(215, 135), Size = new Size(90, 28) };
            btnEliminar.Click += BtnEliminar_Click;
            grpDatos.Controls.Add(btnEliminar);

            var btnLimpiar = new Button { Text = "Limpiar", Location = new Point(315, 135), Size = new Size(90, 28) };
            btnLimpiar.Click += (s, e) => LimpiarFormulario();
            grpDatos.Controls.Add(btnLimpiar);

            this.Controls.Add(lblBuscar);
            this.Controls.Add(cmbTipoBusqueda);
            this.Controls.Add(txtBusqueda);
            this.Controls.Add(btnBuscar);
            this.Controls.Add(btnRefrescar);
            this.Controls.Add(grid);
            this.Controls.Add(grpDatos);

            CargarGrid(null);
            LimpiarFormulario();
        }

        private void CargarGrid(List<Libro> libros)
        {
            var g = Servicios.GestorBiblioteca.Instancia;
            var lista = libros ?? g.Libros;

            grid.Rows.Clear();
            foreach (var l in lista)
                grid.Rows.Add(l.Id, l.Titulo, l.Autor, l.Isbn,
                              l.AnioPublicacion, l.Genero,
                              l.EjemplaresDisponibles, l.EjemplaresTotales);

            // Sin resultados no queda ninguna fila seleccionada: se suelta el libro que
            // estaba en edición para que Guardar/Eliminar no actúen sobre uno que ya no se ve.
            if (grid.Rows.Count == 0)
            {
                _idEditando = null;
                txtTitulo.Text = ""; txtAutor.Text = ""; txtIsbn.Text = "";
                txtAnio.Text = ""; txtGenero.Text = ""; txtEjemplares.Text = "1";
            }
        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            var g = Servicios.GestorBiblioteca.Instancia;
            string texto = txtBusqueda.Text.Trim();
            int idx = cmbTipoBusqueda.SelectedIndex;

            try
            {
                if (idx == 0) CargarGrid(g.Buscar(texto));
                else if (idx == 1) CargarGrid(g.Buscar(texto, true));
                else // rango de años
                {
                    if (texto.Contains("-"))
                    {
                        var partes = texto.Split('-');
                        int min, max;
                        if (int.TryParse(partes[0].Trim(), out min) &&
                            int.TryParse(partes[1].Trim(), out max))
                            CargarGrid(g.Buscar(min, max));
                        else CargarGrid(null);
                    }
                    else
                    {
                        int anio;
                        if (int.TryParse(texto, out anio))
                            CargarGrid(g.Buscar(anio, anio));
                        else CargarGrid(null);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0) return;
            var row = grid.SelectedRows[0];
            if (row.Cells["Id"].Value == null) return;

            var g = Servicios.GestorBiblioteca.Instancia;
            int id = Convert.ToInt32(row.Cells["Id"].Value);
            var libro = g.Libros.Find(l => l.Id == id);
            if (libro == null) return;

            _idEditando = libro.Id;
            txtTitulo.Text = libro.Titulo;
            txtAutor.Text = libro.Autor;
            txtIsbn.Text = libro.Isbn;
            txtAnio.Text = libro.AnioPublicacion.ToString();
            txtGenero.Text = libro.Genero;
            txtEjemplares.Text = libro.EjemplaresTotales.ToString();
        }

        private void BtnNuevo_Click(object sender, EventArgs e) { LimpiarFormulario(); }

        private void LimpiarFormulario()
        {
            _idEditando = null;
            txtTitulo.Text = ""; txtAutor.Text = ""; txtIsbn.Text = "";
            txtAnio.Text = ""; txtGenero.Text = ""; txtEjemplares.Text = "1";
            grid.ClearSelection();
            txtTitulo.Focus();
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                var g = Servicios.GestorBiblioteca.Instancia;
                int anio = 0, ejemplares = 1;
                int.TryParse(txtAnio.Text.Trim(), out anio);
                int.TryParse(txtEjemplares.Text.Trim(), out ejemplares);
                if (ejemplares < 1) ejemplares = 1;

                if (_idEditando.HasValue)
                {
                    var l = new Libro(txtTitulo.Text, txtAutor.Text, txtIsbn.Text, ejemplares)
                    {
                        Id = _idEditando.Value,
                        AnioPublicacion = anio,
                        Genero = txtGenero.Text.Trim()
                    };
                    g.ActualizarLibro(l);
                    MessageBox.Show("Libro actualizado.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    var l = new Libro(txtTitulo.Text, txtAutor.Text, txtIsbn.Text, ejemplares)
                    {
                        AnioPublicacion = anio,
                        Genero = txtGenero.Text.Trim()
                    };
                    g.AgregarLibro(l);
                    MessageBox.Show("Libro agregado.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            if (!_idEditando.HasValue)
            {
                MessageBox.Show("Seleccione un libro.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show("¿Eliminar este libro?", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                Servicios.GestorBiblioteca.Instancia.EliminarLibro(_idEditando.Value);
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
