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
        /// Hash PBKDF2-SHA256 con sal (nunca se guarda en texto plano).
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

        private const string PREFIJO_HASH = "PBKDF2";
        private const int ITERACIONES = 100000;

        /// <summary>
        /// Hashea la contraseña con PBKDF2-SHA256 y una sal aleatoria.
        /// Formato guardado: PBKDF2$iteraciones$sal(base64)$hash(base64).
        /// </summary>
        public static string Hashear(string passwordPlano)
        {
            if (string.IsNullOrEmpty(passwordPlano))
                throw new ArgumentException("La contraseña no puede estar vacía.");

            byte[] sal = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(sal);

            byte[] hash = Pbkdf2(passwordPlano, sal, ITERACIONES);
            return string.Format("{0}${1}${2}${3}", PREFIJO_HASH, ITERACIONES,
                                 Convert.ToBase64String(sal), Convert.ToBase64String(hash));
        }

        private static byte[] Pbkdf2(string password, byte[] sal, int iteraciones)
        {
            using (var kdf = new Rfc2898DeriveBytes(password, sal, iteraciones, HashAlgorithmName.SHA256))
                return kdf.GetBytes(32);
        }

        /// <summary>Hash SHA-256 simple (formato anterior, sin sal). Solo para verificar datos viejos.</summary>
        private static string HashearSha256Antiguo(string passwordPlano)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(passwordPlano));
                var sb = new StringBuilder();
                foreach (var b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        /// <summary>True si el hash guardado es del formato antiguo (SHA-256 sin sal).</summary>
        [JsonIgnore]
        public bool UsaHashAntiguo
        {
            get { return string.IsNullOrEmpty(PasswordHash) || !PasswordHash.StartsWith(PREFIJO_HASH + "$"); }
        }

        /// <summary>
        /// Valida la contraseña contra el hash almacenado (formato nuevo o antiguo).
        /// </summary>
        public bool VerificarPassword(string passwordPlano)
        {
            if (string.IsNullOrEmpty(passwordPlano) || string.IsNullOrEmpty(PasswordHash))
                return false;

            if (UsaHashAntiguo)
                return string.Equals(HashearSha256Antiguo(passwordPlano), PasswordHash,
                                     StringComparison.OrdinalIgnoreCase);

            var partes = PasswordHash.Split('$');
            int iteraciones;
            if (partes.Length != 4 || !int.TryParse(partes[1], out iteraciones) || iteraciones < 1)
                return false;

            try
            {
                byte[] sal = Convert.FromBase64String(partes[2]);
                byte[] esperado = Convert.FromBase64String(partes[3]);
                byte[] obtenido = Pbkdf2(passwordPlano, sal, iteraciones);
                return IgualesEnTiempoConstante(obtenido, esperado);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static bool IgualesEnTiempoConstante(byte[] x, byte[] y)
        {
            if (x.Length != y.Length) return false;
            int diff = 0;
            for (int i = 0; i < x.Length; i++) diff |= x[i] ^ y[i];
            return diff == 0;
        }

        public override string MostrarResumen()
        {
            string rol = EsAdmin ? "Administrador" : "Bibliotecario";
            return string.Format("{0} - {1} ({2})", NombreCompleto, NombreUsuario, rol);
        }
    }
}
