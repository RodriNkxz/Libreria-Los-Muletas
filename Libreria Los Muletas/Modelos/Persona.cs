using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Libreria_Los_Muletas.Modelos
{
    /// <summary>
    /// Clase base abstracta que representa a cualquier persona del sistema.
    /// Aplica herencia y abstracción (POO).
    /// </summary>
    public abstract class Persona
    {
        private string _nombre;
        private string _apellido;

        public int Id { get; set; }

        public string Nombre
        {
            get { return _nombre; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El nombre no puede estar vacío.");
                _nombre = value.Trim();
            }
        }

        public string Apellido
        {
            get { return _apellido; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El apellido no puede estar vacío.");
                _apellido = value.Trim();
            }
        }

        public string Email { get; set; }

        /// <summary>
        /// Propiedad calculada: no se serializa, se genera al vuelo.
        /// </summary>
        [JsonIgnore]
        public string NombreCompleto
        {
            get
            {
                return string.Format("{0} {1}", _nombre, _apellido).Trim();
            }
        }

        /// <summary>
        /// Método abstracto: cada subclase define su propio resumen.
        /// Aplica polimorfismo.
        /// </summary>
        public abstract string MostrarResumen();

        public override string ToString()
        {
            return MostrarResumen();
        }
    }
}