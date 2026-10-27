using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Libreria_Los_Muletas.Modelos;

namespace Libreria_Los_Muletas.Servicios
{
    /// <summary>
    /// Persistencia local en JSON. Crea automáticamente la carpeta datos/
    /// junto al ejecutable. Si un archivo no existe, devuelve lista vacía.
    /// </summary>
    public static class RepositorioJson
    {
        public static string CarpetaDatos
        {
            get
            {
                string ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "datos");
                if (!Directory.Exists(ruta)) Directory.CreateDirectory(ruta);
                return ruta;
            }
        }

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            DateFormatString = "yyyy-MM-ddTHH:mm:ss"
        };

        // ---------- Genéricos ----------

        public static List<T> Cargar<T>(string archivo) where T : class
        {
            string ruta = Path.Combine(CarpetaDatos, archivo);
            if (!File.Exists(ruta)) return new List<T>();

            try
            {
                string json = File.ReadAllText(ruta);
                if (string.IsNullOrWhiteSpace(json)) return new List<T>();
                var lista = JsonConvert.DeserializeObject<List<T>>(json, Settings);
                return lista ?? new List<T>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    string.Format("Error al cargar '{0}': {1}", archivo, ex.Message), ex);
            }
        }

        public static void Guardar<T>(string archivo, List<T> datos) where T : class
        {
            string ruta = Path.Combine(CarpetaDatos, archivo);
            try
            {
                string json = JsonConvert.SerializeObject(datos, Settings);

                // Escritura segura: primero a un archivo temporal y luego se reemplaza el
                // original, dejando una copia .bak. Así un corte a mitad de escritura
                // no deja el JSON corrupto.
                string tmp = ruta + ".tmp";
                File.WriteAllText(tmp, json);
                if (File.Exists(ruta)) File.Replace(tmp, ruta, ruta + ".bak");
                else File.Move(tmp, ruta);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    string.Format("Error al guardar '{0}': {1}", archivo, ex.Message), ex);
            }
        }

        // ---------- Específicos ----------

        public static List<Libro> CargarLibros() => Cargar<Libro>("libros.json");
        public static List<Usuario> CargarUsuarios() => Cargar<Usuario>("usuarios.json");
        public static List<Bibliotecario> CargarBibliotecarios() => Cargar<Bibliotecario>("bibliotecarios.json");
        public static List<Prestamo> CargarPrestamos() => Cargar<Prestamo>("prestamos.json");
        public static List<Reserva> CargarReservas() => Cargar<Reserva>("reservas.json");

        public static void GuardarLibros(List<Libro> l) => Guardar("libros.json", l);
        public static void GuardarUsuarios(List<Usuario> l) => Guardar("usuarios.json", l);
        public static void GuardarBibliotecarios(List<Bibliotecario> l) => Guardar("bibliotecarios.json", l);
        public static void GuardarPrestamos(List<Prestamo> l) => Guardar("prestamos.json", l);
        public static void GuardarReservas(List<Reserva> l) => Guardar("reservas.json", l);
    }
}
