using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Libreria_Los_Muletas.Enums;

namespace Libreria_Los_Muletas.Modelos
{
    /// <summary>
    /// Préstamo de un libro a un socio.
    /// Contiene el método recursivo CalcularDiasRetraso.
    /// </summary>
    public class Prestamo
    {
        public const int DIAS_PRESTAMO = 7;
        public const int DIAS_RENOVACION = 7;

        public int Id { get; set; }
        public int IdLibro { get; set; }
        public int IdUsuario { get; set; }

        public DateTime FechaPrestamo { get; set; }
        public DateTime FechaVencimiento { get; set; }

        /// <summary>
        /// Null mientras no se devuelva el libro.
        /// </summary>
        public DateTime? FechaDevolucion { get; set; }

        public bool Renovado { get; set; }

        public EstadoPrestamo Estado { get; set; }

        // ---------- Constructores sobrecargados ----------

        public Prestamo() { }

        /// <summary>
        /// Sobrecarga #1: usa el plazo por defecto (7 días).
        /// </summary>
        public Prestamo(int id, int idLibro, int idUsuario, DateTime fechaPrestamo)
            : this(id, idLibro, idUsuario, fechaPrestamo, DIAS_PRESTAMO) { }

        /// <summary>
        /// Sobrecarga #2: permite indicar días personalizados.
        /// </summary>
        public Prestamo(int id, int idLibro, int idUsuario, DateTime fechaPrestamo, int dias)
        {
            if (dias <= 0)
                throw new ArgumentException("Los días de préstamo deben ser mayores a cero.");

            Id = id;
            IdLibro = idLibro;
            IdUsuario = idUsuario;
            FechaPrestamo = fechaPrestamo;
            FechaVencimiento = fechaPrestamo.AddDays(dias);
            Estado = EstadoPrestamo.Activo;
            Renovado = false;
        }

        // ---------- Propiedades calculadas ----------

        [JsonIgnore]
        public bool EstaVencido
        {
            get
            {
                return Estado == EstadoPrestamo.Activo
                    && DateTime.Today > FechaVencimiento.Date;
            }
        }

        [JsonIgnore]
        public bool PuedeRenovar
        {
            get { return Estado == EstadoPrestamo.Activo && !Renovado && !EstaVencido; }
        }

        /// <summary>
        /// Días que faltan para el vencimiento (negativo = ya vencido).
        /// </summary>
        [JsonIgnore]
        public int DiasParaVencer
        {
            get { return (FechaVencimiento.Date - DateTime.Today).Days; }
        }

        // ---------- Métodos ----------

        public void Renovar()
        {
            if (!PuedeRenovar)
                throw new InvalidOperationException(
                    "El préstamo no puede renovarse (ya fue renovado, está vencido o devuelto).");

            FechaVencimiento = FechaVencimiento.AddDays(DIAS_RENOVACION);
            Renovado = true;
        }

        public void RegistrarDevolucion()
        {
            if (Estado == EstadoPrestamo.Devuelto)
                throw new InvalidOperationException("El préstamo ya fue devuelto.");

            FechaDevolucion = DateTime.Today;
            Estado = EstadoPrestamo.Devuelto;
        }

        /// <summary>
        /// 🔁 RECURSIVIDAD: calcula los días de retraso acumulando de a 1 día
        /// hasta llegar a la fecha de vencimiento (caso base).
        /// Si no hay retraso, retorna 0.
        /// </summary>
        public int CalcularDiasRetraso()
        {
            if (Estado == EstadoPrestamo.Devuelto)
            {
                // Si ya se devolvió, calculamos con la fecha real de devolución
                return CalcularDiasRetrasoRecursivo(FechaVencimiento, FechaDevolucion.Value.Date);
            }
            return CalcularDiasRetrasoRecursivo(FechaVencimiento, DateTime.Today);
        }

        /// <summary>
        /// Método recursivo privado: recorre día a día hacia atrás.
        /// Caso base: la fecha de referencia no supera el vencimiento → 0 días.
        /// Paso recursivo: 1 + (días entre vencimiento y el día anterior).
        /// </summary>
        private static int CalcularDiasRetrasoRecursivo(DateTime vencimiento, DateTime referencia)
        {
            if (referencia.Date <= vencimiento.Date) return 0;              // caso base
            return 1 + CalcularDiasRetrasoRecursivo(vencimiento,
                                                    referencia.AddDays(-1)); // paso recursivo
        }

        public override string ToString()
        {
            return string.Format("Préstamo #{0} — Libro {1} / Socio {2} — {3} — vence {4:dd/MM/yyyy}{5}",
                Id, IdLibro, IdUsuario, Estado,
                FechaVencimiento,
                Renovado ? " (renovado)" : string.Empty);
        }
    }
}