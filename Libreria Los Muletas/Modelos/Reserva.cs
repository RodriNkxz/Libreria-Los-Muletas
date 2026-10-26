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
    /// Reserva de un libro en la cola FIFO.
    /// El orden se define por FechaReserva (ascendente).
    /// </summary>
    public class Reserva
    {
        public int Id { get; set; }
        public int IdLibro { get; set; }
        public int IdUsuario { get; set; }

        public DateTime FechaReserva { get; set; }

        /// <summary>
        /// Momento en el que el libro se liberó y quedó disponible para retiro.
        /// Null hasta que un ejemplar se descuenta del stock.
        /// </summary>
        public DateTime? FechaDisponible { get; set; }

        public EstadoReserva Estado { get; set; }

        // ---------- Constructores sobrecargados ----------

        public Reserva() { }

        /// <summary>
        /// Sobrecarga #1: reserva con fecha actual.
        /// </summary>
        public Reserva(int id, int idLibro, int idUsuario)
            : this(id, idLibro, idUsuario, DateTime.Now) { }

        /// <summary>
        /// Sobrecarga #2: reserva con fecha explícita (útil en tests).
        /// </summary>
        public Reserva(int id, int idLibro, int idUsuario, DateTime fechaReserva)
        {
            Id = id;
            IdLibro = idLibro;
            IdUsuario = idUsuario;
            FechaReserva = fechaReserva;
            Estado = EstadoReserva.Pendiente;
        }

        // ---------- Propiedades calculadas ----------

        /// <summary>
        /// Una reserva "disponible" expira a las 24 h si no se retira.
        /// </summary>
        [JsonIgnore]
        public bool Expiro
        {
            get
            {
                return Estado == EstadoReserva.Disponible
                    && FechaDisponible.HasValue
                    && DateTime.Now > FechaDisponible.Value.AddHours(24);
            }
        }

        // ---------- Métodos ----------

        public void MarcarDisponible()
        {
            if (Estado != EstadoReserva.Pendiente)
                throw new InvalidOperationException(
                    "Solo se pueden marcar como disponibles reservas pendientes.");
            Estado = EstadoReserva.Disponible;
            FechaDisponible = DateTime.Now;
        }

        public void Completar()
        {
            if (Estado != EstadoReserva.Disponible)
                throw new InvalidOperationException(
                    "Solo se pueden completar reservas disponibles.");
            Estado = EstadoReserva.Completada;
        }

        public void Cancelar()
        {
            if (Estado == EstadoReserva.Completada)
                throw new InvalidOperationException(
                    "No se puede cancelar una reserva ya completada.");
            Estado = EstadoReserva.Cancelada;
        }

        public override string ToString()
        {
            return string.Format("Reserva #{0} — Libro {1} / Socio {2} — {3}",
                Id, IdLibro, IdUsuario, Estado);
        }
    }
}
