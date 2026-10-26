using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Libreria_Los_Muletas.Interfaces;

namespace Libreria_Los_Muletas.Modelos
{
    /// <summary>
    /// Libro del catálogo. Implementa IPrestable e IBuscable.
    /// Aplica encapsulamiento, sobrecarga de constructores e IComparable (para búsqueda binaria).
    /// </summary>
    public class Libro : IPrestable, IBuscable<Libro>, IComparable<Libro>
    {
        private string _titulo;
        private string _autor;
        private string _isbn;
        private int _ejemplaresTotales;
        private int _ejemplaresDisponibles;

        public int Id { get; set; }

        public string Titulo
        {
            get { return _titulo; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El título no puede estar vacío.");
                _titulo = value.Trim();
            }
        }

        public string Autor
        {
            get { return _autor; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El autor no puede estar vacío.");
                _autor = value.Trim();
            }
        }

        public string Isbn
        {
            get { return _isbn; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El ISBN no puede estar vacío.");
                _isbn = value.Trim();
            }
        }

        public int AnioPublicacion { get; set; }
        public string Genero { get; set; }

        public int EjemplaresTotales
        {
            get { return _ejemplaresTotales; }
            set
            {
                if (value < 1)
                    throw new ArgumentException("Debe haber al menos 1 ejemplar total.");
                _ejemplaresTotales = value;
                if (_ejemplaresDisponibles > _ejemplaresTotales)
                    _ejemplaresDisponibles = _ejemplaresTotales;
            }
        }

        public int EjemplaresDisponibles
        {
            get { return _ejemplaresDisponibles; }
            set
            {
                if (value < 0 || value > _ejemplaresTotales)
                    throw new ArgumentException("Ejemplares disponibles fuera de rango (0..Total).");
                _ejemplaresDisponibles = value;
            }
        }

        // ---------- Constructores sobrecargados ----------

        /// <summary>
        /// Constructor vacío para deserialización.
        /// </summary>
        public Libro()
        {
            _ejemplaresTotales = 1;
            _ejemplaresDisponibles = 1;
        }

        /// <summary>
        /// Sobrecarga #1: solo datos básicos, asume 1 ejemplar.
        /// </summary>
        public Libro(string titulo, string autor, string isbn)
            : this(titulo, autor, isbn, 1) { }

        /// <summary>
        /// Sobrecarga #2: con cantidad de ejemplares.
        /// </summary>
        public Libro(string titulo, string autor, string isbn, int ejemplares)
        {
            Titulo = titulo;
            Autor = autor;
            Isbn = isbn;
            EjemplaresTotales = ejemplares;
            _ejemplaresDisponibles = ejemplares;
        }

        // ---------- IPrestable ----------

        public bool HayDisponibilidad()
        {
            return _ejemplaresDisponibles > 0;
        }

        public void PrestarEjemplar()
        {
            if (!HayDisponibilidad())
                throw new InvalidOperationException(
                    string.Format("No hay ejemplares disponibles de '{0}'.", _titulo));
            _ejemplaresDisponibles--;
        }

        public void DevolverEjemplar()
        {
            if (_ejemplaresDisponibles >= _ejemplaresTotales)
                throw new InvalidOperationException("Todos los ejemplares ya están en stock.");
            _ejemplaresDisponibles++;
        }

        // ---------- IBuscable ----------

        public bool CoincideCon(string criterio)
        {
            if (string.IsNullOrWhiteSpace(criterio)) return true;
            string c = criterio.Trim().ToLowerInvariant();
            return (_titulo ?? "").ToLowerInvariant().Contains(c)
                || (_autor ?? "").ToLowerInvariant().Contains(c)
                || (_isbn ?? "").ToLowerInvariant().Contains(c);
        }

        // ---------- IComparable (para búsqueda binaria por ISBN) ----------

        public int CompareTo(Libro other)
        {
            if (other == null) return 1;
            return string.Compare(_isbn, other._isbn,
                                  StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return string.Format("[{0}] {1} — {2} (disp: {3}/{4})",
                _isbn, _titulo, _autor, _ejemplaresDisponibles, _ejemplaresTotales);
        }
    }
}
