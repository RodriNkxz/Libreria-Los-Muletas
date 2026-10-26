using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libreria_Los_Muletas.Interfaces
{
    /// <summary>
    /// Contrato para entidades que pueden prestarse (ej. Libro).
    /// </summary>
    public interface IPrestable
    {
        int EjemplaresTotales { get; }
        int EjemplaresDisponibles { get; }

        /// <summary>
        /// Indica si hay al menos un ejemplar disponible para prestar.
        /// </summary>
        bool HayDisponibilidad();

        /// <summary>
        /// Descuenta un ejemplar disponible. Lanza excepción si no hay stock.
        /// </summary>
        void PrestarEjemplar();

        /// <summary>
        /// Devuelve un ejemplar al stock. Lanza excepción si se supera el total.
        /// </summary>
        void DevolverEjemplar();
    }
}
