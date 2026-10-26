using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Libreria_Los_Muletas.Modelos
{
    /// <summary>
    /// Socio de la biblioteca. Hereda de Persona.
    /// </summary>
    public class Usuario : Persona
    {
        public bool Activo { get; set; }

        public DateTime FechaAlta { get; set; }

        /// <summary>
        /// Se calcula dinámicamente desde GestorBiblioteca al consultar préstamos.
        /// No se serializa para evitar datos duplicados.
        /// </summary>
        [JsonIgnore]
        public int PrestamosActivos { get; set; }

        /// <summary>
        /// Se calcula dinámicamente desde GestorBiblioteca al consultar préstamos vencidos.
        /// </summary>
        [JsonIgnore]
        public bool EsMoroso { get; set; }

        public Usuario()
        {
            Activo = true;
            FechaAlta = DateTime.Today;
        }

        /// <summary>
        /// Constructor sobrecargado (conveniencia).
        /// </summary>
        public Usuario(int id, string nombre, string apellido, string email)
        {
            Id = id;
            Nombre = nombre;
            Apellido = apellido;
            Email = email;
            Activo = true;
            FechaAlta = DateTime.Today;
        }

        public override string MostrarResumen()
        {
            string estado = Activo ? "activo" : "inactivo";
            string moroso = EsMoroso ? " | MOROSO" : string.Empty;
            return string.Format("Socio #{0} - {1} ({2}){3}", Id, NombreCompleto, estado, moroso);
        }
    }
}
