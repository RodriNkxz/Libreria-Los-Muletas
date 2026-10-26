using System;
using System.Drawing;
using System.Windows.Forms;

namespace Libreria_Los_Muletas.Formularios
{
    public class FrmReportes : Form
    {
        private ComboBox cmbTipo;
        private DataGridView grid;
        private Label lblResumen;

        public FrmReportes()
        {
            this.Text = "PGB - Reportes";
            this.Size = new Size(880, 520);
            this.StartPosition = FormStartPosition.CenterParent;

            var lbl = new Label { Text = "Reporte:", Location = new Point(10, 15), Size = new Size(60, 22) };
            cmbTipo = new ComboBox
            {
                Location = new Point(75, 12),
                Size = new Size(280, 24),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbTipo.Items.AddRange(new object[]
            {
                "Libros más prestados",
                "Préstamos vencidos",
                "Socios morosos"
            });
            cmbTipo.SelectedIndex = 0;

            var btnGenerar = new Button { Text = "Generar reporte", Location = new Point(365, 11), Size = new Size(130, 26) };
            btnGenerar.Click += (s, e) => GenerarReporte();

            lblResumen = new Label
            {
                Location = new Point(510, 15),
                Size = new Size(350, 20),
                ForeColor = Color.DarkSlateGray
            };

            grid = new DataGridView
            {
                Location = new Point(10, 50),
                Size = new Size(845, 420),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            this.Controls.Add(lbl);
            this.Controls.Add(cmbTipo);
            this.Controls.Add(btnGenerar);
            this.Controls.Add(lblResumen);
            this.Controls.Add(grid);

            cmbTipo.SelectedIndexChanged += (s, e) => GenerarReporte();
            GenerarReporte();
        }

        private void GenerarReporte()
        {
            var g = Servicios.GestorBiblioteca.Instancia;
            grid.Columns.Clear();
            grid.Rows.Clear();

            try
            {
                if (cmbTipo.SelectedIndex == 0)
                {
                    grid.Columns.Add("Titulo", "Título");
                    grid.Columns.Add("Autor", "Autor");
                    grid.Columns.Add("Isbn", "ISBN");
                    grid.Columns.Add("Cantidad", "Cantidad de préstamos");

                    var datos = Servicios.Reportes.LibrosMasPrestados(g);
                    foreach (var d in datos)
                        grid.Rows.Add(d.Titulo, d.Autor, d.Isbn, d.CantidadPrestamos);

                    lblResumen.Text = "Total de libros con préstamos: " + datos.Count;
                }
                else if (cmbTipo.SelectedIndex == 1)
                {
                    grid.Columns.Add("Id", "ID");
                    grid.Columns.Add("Titulo", "Libro");
                    grid.Columns.Add("Socio", "Socio");
                    grid.Columns.Add("Vencimiento", "Fecha vencimiento");
                    grid.Columns.Add("Retraso", "Días de retraso");

                    var datos = Servicios.Reportes.PrestamosVencidos(g);
                    foreach (var d in datos)
                        grid.Rows.Add(d.IdPrestamo, d.TituloLibro, d.Socio,
                                      d.FechaVencimiento.ToString("dd/MM/yyyy"), d.DiasRetraso);

                    lblResumen.Text = "Préstamos vencidos: " + datos.Count;
                }
                else
                {
                    grid.Columns.Add("Id", "ID");
                    grid.Columns.Add("Nombre", "Socio");
                    grid.Columns.Add("Email", "Email");
                    grid.Columns.Add("Cant", "Préstamos vencidos");
                    grid.Columns.Add("MaxDias", "Máx. días de atraso");

                    var datos = Servicios.Reportes.SociosMorosos(g);
                    foreach (var d in datos)
                        grid.Rows.Add(d.IdUsuario, d.Nombre, d.Email,
                                      d.PrestamosVencidos, d.MaxDiasRetraso);

                    lblResumen.Text = "Socios morosos: " + datos.Count;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
