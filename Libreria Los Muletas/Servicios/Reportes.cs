using System;
using System.Collections.Generic;
using System.Linq;
using Libreria_Los_Muletas.Enums;
using Libreria_Los_Muletas.Modelos;

namespace Libreria_Los_Muletas.Servicios
{
    // ---------- DTOs de reportes ----------

    public class LibroMasPrestado
    {
        public int IdLibro { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public string Isbn { get; set; }
        public int CantidadPrestamos { get; set; }
    }

    public class PrestamoVencido
    {
        public int IdPrestamo { get; set; }
        public string TituloLibro { get; set; }
        public string Socio { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public int DiasRetraso { get; set; }
    }

    public class SocioMoroso
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public int PrestamosVencidos { get; set; }
        public int MaxDiasRetraso { get; set; }
    }

    /// <summary>
    /// Generador de reportes. Todos los métodos reciben el gestor para
    /// no acoplarse a una instancia concreta y facilitar pruebas.
    /// </summary>
    public static class Reportes
    {
        /// <summary>
        /// Ranking de libros por cantidad de préstamos (histórico).
        /// </summary>
        public static List<LibroMasPrestado> LibrosMasPrestados(GestorBiblioteca g)
        {
            return g.Prestamos
                .GroupBy(p => p.IdLibro)
                .Select(gr => new { IdLibro = gr.Key, Cantidad = gr.Count() })
                .Join(g.Libros,
                      x => x.IdLibro,
                      l => l.Id,
                      (x, l) => new LibroMasPrestado
                      {
                          IdLibro = l.Id,
                          Titulo = l.Titulo,
                          Autor = l.Autor,
                          Isbn = l.Isbn,
                          CantidadPrestamos = x.Cantidad
                      })
                .OrderByDescending(x => x.CantidadPrestamos)
                .ThenBy(x => x.Titulo)
                .ToList();
        }

        /// <summary>
        /// Préstamos activos que ya pasaron la fecha de vencimiento.
        /// Usa el método recursivo CalcularDiasRetraso().
        /// </summary>
        public static List<PrestamoVencido> PrestamosVencidos(GestorBiblioteca g)
        {
            return g.Prestamos
                .Where(p => p.Estado == EstadoPrestamo.Activo && p.EstaVencido)
                .Select(p => new PrestamoVencido
                {
                    IdPrestamo = p.Id,
                    TituloLibro = g.Libros.FirstOrDefault(l => l.Id == p.IdLibro)?.Titulo
                                  ?? "(libro eliminado)",
                    Socio = g.Usuarios.FirstOrDefault(u => u.Id == p.IdUsuario)?.NombreCompleto
                            ?? "(socio eliminado)",
                    FechaVencimiento = p.FechaVencimiento,
                    DiasRetraso = p.CalcularDiasRetraso()
                })
                .OrderByDescending(x => x.DiasRetraso)
                .ToList();
        }

        /// <summary>
        /// Socios con al menos un préstamo vencido activo.
        /// </summary>
        public static List<SocioMoroso> SociosMorosos(GestorBiblioteca g)
        {
            return g.Usuarios
                .Where(u => g.EsMoroso(u))
                .Select(u => new SocioMoroso
                {
                    IdUsuario = u.Id,
                    Nombre = u.NombreCompleto,
                    Email = u.Email,
                    PrestamosVencidos = g.Prestamos.Count(p =>
                        p.IdUsuario == u.Id
                        && p.Estado == EstadoPrestamo.Activo
                        && p.EstaVencido),
                    MaxDiasRetraso = g.Prestamos
                        .Where(p => p.IdUsuario == u.Id
                                    && p.Estado == EstadoPrestamo.Activo
                                    && p.EstaVencido)
                        .Select(p => p.CalcularDiasRetraso())
                        .DefaultIfEmpty(0)
                        .Max()
                })
                .OrderByDescending(x => x.MaxDiasRetraso)
                .ToList();
        }
    }
}
