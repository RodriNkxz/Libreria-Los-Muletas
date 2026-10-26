using System;
using System.Collections.Generic;
using System.Linq;
using Libreria_Los_Muletas.Enums;
using Libreria_Los_Muletas.Interfaces;
using Libreria_Los_Muletas.Modelos;

namespace Libreria_Los_Muletas.Servicios
{
    /// <summary>
    /// Fachada principal del sistema. Singleton: una sola instancia
    /// para todos los formularios.
    /// </summary>
    public class GestorBiblioteca
    {
        // ============ SINGLETON ============

        private static GestorBiblioteca _instancia;
        public static GestorBiblioteca Instancia
        {
            get
            {
                if (_instancia == null) _instancia = new GestorBiblioteca();
                return _instancia;
            }
        }

        private GestorBiblioteca()
        {
            Libros = new List<Libro>();
            Usuarios = new List<Usuario>();
            Bibliotecarios = new List<Bibliotecario>();
            Prestamos = new List<Prestamo>();
            Reservas = new List<Reserva>();
        }

        // ============ COLECCIONES ============

        public List<Libro> Libros { get; private set; }
        public List<Usuario> Usuarios { get; private set; }
        public List<Bibliotecario> Bibliotecarios { get; private set; }
        public List<Prestamo> Prestamos { get; private set; }
        public List<Reserva> Reservas { get; private set; }

        public Bibliotecario SesionActual { get; private set; }

        // ============ PERSISTENCIA ============

        public void CargarTodo()
        {
            Libros = RepositorioJson.CargarLibros();
            Usuarios = RepositorioJson.CargarUsuarios();
            Bibliotecarios = RepositorioJson.CargarBibliotecarios();
            Prestamos = RepositorioJson.CargarPrestamos();
            Reservas = RepositorioJson.CargarReservas();

            // Crear admin por defecto si no hay ninguno
            if (Bibliotecarios.Count == 0)
            {
                Bibliotecarios.Add(new Bibliotecario(
                    1, "Admin", "Sistema", "admin", "admin", true));
                GuardarTodo();
            }

            // Inicializar estados calculados (no persistidos)
            ActualizarEstadoUsuarios();
            ProcesarReservasExpiradas();
        }

        public void GuardarTodo()
        {
            RepositorioJson.GuardarLibros(Libros);
            RepositorioJson.GuardarUsuarios(Usuarios);
            RepositorioJson.GuardarBibliotecarios(Bibliotecarios);
            RepositorioJson.GuardarPrestamos(Prestamos);
            RepositorioJson.GuardarReservas(Reservas);
        }

        // ============ LOGIN ============

        public bool Login(string usuario, string password)
        {
            var b = Bibliotecarios.FirstOrDefault(x =>
                string.Equals(x.NombreUsuario, usuario, StringComparison.OrdinalIgnoreCase));
            if (b == null || !b.VerificarPassword(password)) return false;
            SesionActual = b;
            return true;
        }

        public void Logout() { SesionActual = null; }

        // ============ IDs AUTOINCREMENTALES ============

        public int ProximoIdLibro() => Libros.Count == 0 ? 1 : Libros.Max(l => l.Id) + 1;
        public int ProximoIdUsuario() => Usuarios.Count == 0 ? 1 : Usuarios.Max(u => u.Id) + 1;
        public int ProximoIdBibliotecario() => Bibliotecarios.Count == 0 ? 1 : Bibliotecarios.Max(b => b.Id) + 1;
        public int ProximoIdPrestamo() => Prestamos.Count == 0 ? 1 : Prestamos.Max(p => p.Id) + 1;
        public int ProximoIdReserva() => Reservas.Count == 0 ? 1 : Reservas.Max(r => r.Id) + 1;

        // ============ LIBROS: CRUD ============

        public void AgregarLibro(Libro libro)
        {
            if (libro == null) throw new ArgumentNullException(nameof(libro));
            if (Libros.Any(l => string.Equals(l.Isbn, libro.Isbn, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Ya existe un libro con ese ISBN.");

            if (libro.Id == 0) libro.Id = ProximoIdLibro();
            Libros.Add(libro);
            GuardarTodo();
        }

        public void ActualizarLibro(Libro libro)
        {
            if (libro == null) throw new ArgumentNullException(nameof(libro));
            var existente = Libros.FirstOrDefault(l => l.Id == libro.Id);
            if (existente == null) throw new InvalidOperationException("El libro no existe.");

            if (Libros.Any(l => l.Id != libro.Id &&
                string.Equals(l.Isbn, libro.Isbn, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Ya existe otro libro con ese ISBN.");

            int prestados = Prestamos.Count(p =>
                p.IdLibro == libro.Id && p.Estado == EstadoPrestamo.Activo);

            if (libro.EjemplaresTotales < prestados)
                throw new InvalidOperationException(string.Format(
                    "No se pueden reducir los ejemplares a {0}; hay {1} en préstamo.",
                    libro.EjemplaresTotales, prestados));

            existente.Titulo = libro.Titulo;
            existente.Autor = libro.Autor;
            existente.Isbn = libro.Isbn;
            existente.AnioPublicacion = libro.AnioPublicacion;
            existente.Genero = libro.Genero;
            existente.EjemplaresTotales = libro.EjemplaresTotales;
            existente.EjemplaresDisponibles = libro.EjemplaresTotales - prestados;
            GuardarTodo();
        }

        public void EliminarLibro(int idLibro)
        {
            var libro = Libros.FirstOrDefault(l => l.Id == idLibro);
            if (libro == null) throw new InvalidOperationException("El libro no existe.");

            if (Prestamos.Any(p => p.IdLibro == idLibro && p.Estado == EstadoPrestamo.Activo))
                throw new InvalidOperationException("No se puede eliminar un libro con préstamos activos.");

            foreach (var r in Reservas.Where(r => r.IdLibro == idLibro &&
                (r.Estado == EstadoReserva.Pendiente || r.Estado == EstadoReserva.Disponible)))
            {
                r.Cancelar();
            }

            Libros.Remove(libro);
            GuardarTodo();
        }

        // ============ USUARIOS: CRUD ============

        public void AgregarUsuario(Usuario usuario)
        {
            if (usuario == null) throw new ArgumentNullException(nameof(usuario));
            if (usuario.Id == 0) usuario.Id = ProximoIdUsuario();
            Usuarios.Add(usuario);
            GuardarTodo();
        }

        public void ActualizarUsuario(Usuario usuario)
        {
            if (usuario == null) throw new ArgumentNullException(nameof(usuario));
            var existente = Usuarios.FirstOrDefault(u => u.Id == usuario.Id);
            if (existente == null) throw new InvalidOperationException("El usuario no existe.");

            existente.Nombre = usuario.Nombre;
            existente.Apellido = usuario.Apellido;
            existente.Email = usuario.Email;
            existente.Activo = usuario.Activo;
            GuardarTodo();
        }

        public void EliminarUsuario(int idUsuario)
        {
            var usuario = Usuarios.FirstOrDefault(u => u.Id == idUsuario);
            if (usuario == null) throw new InvalidOperationException("El usuario no existe.");

            if (Prestamos.Any(p => p.IdUsuario == idUsuario && p.Estado == EstadoPrestamo.Activo))
                throw new InvalidOperationException("No se puede eliminar un usuario con préstamos activos.");

            foreach (var r in Reservas.Where(r => r.IdUsuario == idUsuario &&
                (r.Estado == EstadoReserva.Pendiente || r.Estado == EstadoReserva.Disponible)))
            {
                r.Cancelar();
            }

            Usuarios.Remove(usuario);
            GuardarTodo();
        }

        public void AgregarBibliotecario(Bibliotecario b)
        {
            if (b == null) throw new ArgumentNullException(nameof(b));
            if (Bibliotecarios.Any(x =>
                string.Equals(x.NombreUsuario, b.NombreUsuario, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("El nombre de usuario ya existe.");

            if (b.Id == 0) b.Id = ProximoIdBibliotecario();
            Bibliotecarios.Add(b);
            GuardarTodo();
        }

        // ============ BÚSQUEDA SOBRECARGADA (LIBROS) ============

        /// <summary>Sobrecarga #1: busca por título o autor (o ISBN parcial).</summary>
        public List<Libro> Buscar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return new List<Libro>(Libros);
            return Libros.Where(l => l.CoincideCon(texto)).ToList();
        }

        /// <summary>Sobrecarga #2: busca por ISBN exacto o parcial.</summary>
        public List<Libro> Buscar(string isbn, bool exacto)
        {
            if (string.IsNullOrWhiteSpace(isbn)) return new List<Libro>();
            return exacto
                ? Libros.Where(l => string.Equals(l.Isbn, isbn.Trim(), StringComparison.OrdinalIgnoreCase)).ToList()
                : Libros.Where(l => l.Isbn != null && l.Isbn.ToLowerInvariant().Contains(isbn.Trim().ToLowerInvariant())).ToList();
        }

        /// <summary>Sobrecarga #3: busca por rango de años de publicación.</summary>
        public List<Libro> Buscar(int anioMin, int anioMax)
        {
            if (anioMin > anioMax) { int t = anioMin; anioMin = anioMax; anioMax = t; }
            return Libros.Where(l => l.AnioPublicacion >= anioMin && l.AnioPublicacion <= anioMax).ToList();
        }

        // ============ BÚSQUEDA BINARIA RECURSIVA ============

        /// <summary>
        /// 🔁 RECURSIVIDAD: búsqueda binaria recursiva por ISBN exacto.
        /// Ordena una copia por ISBN y delega en el método recursivo.
        /// </summary>
        public Libro BuscarPorIsbnRecursivo(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn)) return null;
            var ordenados = Libros
                .OrderBy(l => l.Isbn, StringComparer.OrdinalIgnoreCase)
                .ToList();
            return BuscarPorIsbnRecursivo(ordenados, isbn.Trim(), 0, ordenados.Count - 1);
        }

        /// <summary>
        /// Método recursivo puro (público para poder demostrarlo).
        /// Caso base: ini > fin → null.
        /// Paso recursivo: según comparación, va a la mitad izquierda o derecha.
        /// </summary>
        public Libro BuscarPorIsbnRecursivo(List<Libro> libros, string isbn, int ini, int fin)
        {
            if (ini > fin) return null;                                  // caso base

            int mid = (ini + fin) / 2;
            int cmp = string.Compare(libros[mid].Isbn, isbn, StringComparison.OrdinalIgnoreCase);

            if (cmp == 0) return libros[mid];                            // caso base: encontrado

            return cmp > 0
                ? BuscarPorIsbnRecursivo(libros, isbn, ini, mid - 1)     // paso recursivo
                : BuscarPorIsbnRecursivo(libros, isbn, mid + 1, fin);    // paso recursivo
        }

        // ============ PRÉSTAMOS (SOBRECARGA) ============

        /// <summary>
        /// Sobrecarga #1: usa el plazo por defecto (7 días).
        /// </summary>
        public Prestamo RegistrarPrestamo(Usuario usuario, Libro libro)
        {
            return RegistrarPrestamo(usuario, libro, Prestamo.DIAS_PRESTAMO);
        }

        /// <summary>
        /// Sobrecarga #2: permite indicar días personalizados.
        /// Si no hay stock, crea una reserva y lanza excepción informativa.
        /// </summary>
        public Prestamo RegistrarPrestamo(Usuario usuario, Libro libro, int dias)
        {
            if (usuario == null) throw new ArgumentNullException(nameof(usuario));
            if (libro == null) throw new ArgumentNullException(nameof(libro));
            if (dias <= 0) throw new ArgumentException("Los días deben ser mayores a cero.");

            var u = Usuarios.FirstOrDefault(x => x.Id == usuario.Id);
            if (u == null) throw new InvalidOperationException("El socio no existe.");
            if (!u.Activo) throw new InvalidOperationException("El socio está inactivo.");

            var l = Libros.FirstOrDefault(x => x.Id == libro.Id);
            if (l == null) throw new InvalidOperationException("El libro no existe.");

            if (EsMoroso(u))
                throw new InvalidOperationException("El socio es moroso. Debe regularizar sus préstamos vencidos.");

            int activos = Prestamos.Count(p => p.IdUsuario == u.Id && p.Estado == EstadoPrestamo.Activo);
            if (activos >= 3)
                throw new InvalidOperationException("El socio ya tiene 3 préstamos activos.");

            // ¿Tiene el socio una reserva DISPONIBLE para este libro?
            var reservaPropia = Reservas.FirstOrDefault(r =>
                r.IdLibro == l.Id && r.IdUsuario == u.Id && r.Estado == EstadoReserva.Disponible);

            if (reservaPropia != null)
            {
                // Se completa la reserva y se crea el préstamo SIN tocar stock
                // (el stock ya se incrementó al devolver el libro que la liberó)
                reservaPropia.Completar();
                var nuevo = new Prestamo(ProximoIdPrestamo(), l.Id, u.Id, DateTime.Today, dias);
                Prestamos.Add(nuevo);
                GuardarTodo();
                return nuevo;
            }

            // ¿El libro está reservado para OTRO socio?
            bool reservadoParaOtro = Reservas.Any(r =>
                r.IdLibro == l.Id && r.Estado == EstadoReserva.Disponible && r.IdUsuario != u.Id);
            if (reservadoParaOtro)
                throw new InvalidOperationException("El libro está reservado para otro socio.");

            // ¿Hay reservas pendientes? Solo el primero de la cola puede llevarse el libro.
            bool hayPendientes = Reservas.Any(r =>
                r.IdLibro == l.Id && r.Estado == EstadoReserva.Pendiente);
            if (hayPendientes)
                throw new InvalidOperationException("Hay reservas pendientes para este libro. No se puede prestar a otro socio.");

            // Sin stock → crear reserva y avisar
            if (!l.HayDisponibilidad())
            {
                var reserva = new Reserva(ProximoIdReserva(), l.Id, u.Id);
                Reservas.Add(reserva);
                GuardarTodo();
                throw new InvalidOperationException(
                    "No hay ejemplares disponibles. Se generó una reserva para el socio.");
            }

            l.PrestarEjemplar();
            var prestamo = new Prestamo(ProximoIdPrestamo(), l.Id, u.Id, DateTime.Today, dias);
            Prestamos.Add(prestamo);
            GuardarTodo();
            return prestamo;
        }

        // ============ DEVOLUCIÓN Y RENOVACIÓN ============

        public void Devolver(int idPrestamo)
        {
            var p = Prestamos.FirstOrDefault(x => x.Id == idPrestamo);
            if (p == null) throw new InvalidOperationException("El préstamo no existe.");
            if (p.Estado == EstadoPrestamo.Devuelto)
                throw new InvalidOperationException("El préstamo ya fue devuelto.");

            p.RegistrarDevolucion();

            var libro = Libros.FirstOrDefault(l => l.Id == p.IdLibro);
            if (libro != null) libro.DevolverEjemplar();

            // Promover la reserva pendiente más antigua a "Disponible" (24 h)
            var primeraPendiente = Reservas
                .Where(r => r.IdLibro == p.IdLibro && r.Estado == EstadoReserva.Pendiente)
                .OrderBy(r => r.FechaReserva)
                .FirstOrDefault();

            if (primeraPendiente != null) primeraPendiente.MarcarDisponible();

            GuardarTodo();
        }

        public void Renovar(int idPrestamo)
        {
            var p = Prestamos.FirstOrDefault(x => x.Id == idPrestamo);
            if (p == null) throw new InvalidOperationException("El préstamo no existe.");
            p.Renovar();
            GuardarTodo();
        }

        public void CancelarReserva(int idReserva)
        {
            var r = Reservas.FirstOrDefault(x => x.Id == idReserva);
            if (r == null) throw new InvalidOperationException("La reserva no existe.");
            r.Cancelar();
            GuardarTodo();
        }

        // ============ ESTADO / CONSULTAS ============

        public bool EsMoroso(Usuario usuario)
        {
            return Prestamos.Any(p => p.IdUsuario == usuario.Id
                                      && p.Estado == EstadoPrestamo.Activo
                                      && p.EstaVencido);
        }

        public int PrestamosActivosDe(Usuario usuario)
        {
            return Prestamos.Count(p => p.IdUsuario == usuario.Id && p.Estado == EstadoPrestamo.Activo);
        }

        public void ActualizarEstadoUsuarios()
        {
            foreach (var u in Usuarios)
            {
                u.PrestamosActivos = PrestamosActivosDe(u);
                u.EsMoroso = EsMoroso(u);
            }
        }

        public void ProcesarReservasExpiradas()
        {
            var expiradas = Reservas.Where(r =>
                r.Estado == EstadoReserva.Disponible && r.Expiro).ToList();

            foreach (var r in expiradas)
            {
                r.Cancelar();
                var siguiente = Reservas
                    .Where(x => x.IdLibro == r.IdLibro && x.Estado == EstadoReserva.Pendiente)
                    .OrderBy(x => x.FechaReserva)
                    .FirstOrDefault();
                if (siguiente != null) siguiente.MarcarDisponible();
            }

            if (expiradas.Count > 0) GuardarTodo();
        }

        // ============ CONSULTAS AUXILIARES (para los forms) ============

        public List<Prestamo> PrestamosDeUsuario(int idUsuario) =>
            Prestamos.Where(p => p.IdUsuario == idUsuario).OrderByDescending(p => p.FechaPrestamo).ToList();

        public List<Prestamo> PrestamosDeLibro(int idLibro) =>
            Prestamos.Where(p => p.IdLibro == idLibro).OrderByDescending(p => p.FechaPrestamo).ToList();

        public List<Reserva> ReservasDeLibro(int idLibro) =>
            Reservas.Where(r => r.IdLibro == idLibro)
                    .OrderBy(r => r.FechaReserva).ToList();

        public List<Reserva> ReservasDeUsuario(int idUsuario) =>
            Reservas.Where(r => r.IdUsuario == idUsuario)
                    .OrderByDescending(r => r.FechaReserva).ToList();

        public List<Usuario> BuscarUsuarios(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return new List<Usuario>(Usuarios);
            string t = texto.Trim().ToLowerInvariant();
            return Usuarios.Where(u =>
                (u.Nombre ?? "").ToLowerInvariant().Contains(t) ||
                (u.Apellido ?? "").ToLowerInvariant().Contains(t) ||
                (u.Email ?? "").ToLowerInvariant().Contains(t) ||
                u.Id.ToString() == t).ToList();
        }
    }
}
