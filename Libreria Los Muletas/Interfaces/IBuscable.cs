using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libreria_Los_Muletas.Interfaces
{
    /// <summary>
    /// Contrato para entidades que pueden ser buscadas por un criterio de texto libre.
    /// Permite aplicar polimorfismo en el módulo de búsqueda.
    /// </summary>
    public interface IBuscable<T> where T : class
    {
        /// <summary>
        /// Retorna true si el objeto coincide con el criterio de búsqueda.
        /// </summary>
        bool CoincideCon(string criterio);
    }
}
