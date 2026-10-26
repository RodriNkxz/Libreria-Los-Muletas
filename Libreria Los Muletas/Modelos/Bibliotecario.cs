using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Libreria_Los_Muletas.Modelos
{
    /// <summary>
    /// Bibliotecario que opera el sistema. Hereda de Persona.
    /// </summary>
    public class Bibliotecario : Persona
    {
        public string NombreUsuario { get; set; }

        /// <summary>
        /// Hash SHA-256 de la contraseña (nunca se guarda en texto plano).
        /// </summary>
        public string PasswordHash { get; set; }

        public bool EsAdmin { get; set; }

        /// <summary>
        /// Constructor sobrecargado #1: sin password (para deserialización).
        /// </summary>
        public Bibliotecario() { }

        /// <summary>
        /// Constructor sobrecargado #2: con password en claro, la hashea.
        /// </summary>
        public Bibliotecario(int id, string nombre, string apellido,
                             string nombreUsuario, string passwordPlano, bool esAdmin)
        {
            Id = id;
            Nombre = nombre;
            Apellido = apellido;
            NombreUsuario = nombreUsuario;
            PasswordHash = Hashear(passwordPlano);
            EsAdmin = esAdmin;
        }

        /// <summary>
        /// Sobrecarga de Hashear: con password explícito.
        /// </summary>
        public static string Hashear(string passwordPlano)
        {
            if (string.IsNullOrEmpty(passwordPlano))
                throw new ArgumentException("La contraseña no puede estar vacía.");

            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(passwordPlano));
                var sb = new StringBuilder();
                foreach (var b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        /// <summary>
        /// Sobrecarga de VerificarPassword: valida contra el hash almacenado.
        /// </summary>
        public bool VerificarPassword(string passwordPlano)
        {
            if (string.IsNullOrEmpty(passwordPlano) || string.IsNullOrEmpty(PasswordHash))
                return false;
            return string.Equals(Hashear(passwordPlano), PasswordHash,
                                 StringComparison.OrdinalIgnoreCase);
        }

        public override string MostrarResumen()
        {
            string rol = EsAdmin ? "Administrador" : "Bibliotecario";
            return string.Format("{0} - {1} ({2})", NombreCompleto, NombreUsuario, rol);
        }
    }
}
