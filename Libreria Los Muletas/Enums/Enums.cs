using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libreria_Los_Muletas.Enums
{
    /// <summary>
    /// Estado de un préstamo.
    /// Nota: "Vencido" NO se almacena, se calcula dinámicamente
    /// comparando FechaVencimiento con la fecha actual.
    /// </summary>
    public enum EstadoPrestamo
    {
        Activo = 0,
        Devuelto = 1
    }

    /// <summary>
    /// Estado de una reserva en la cola FIFO.
    /// </summary>
    public enum EstadoReserva
    {
        Pendiente = 0,   // esperando que se libere un ejemplar
        Disponible = 1,  // el libro se liberó, el socio tiene 24 h para retirarlo
        Completada = 2,  // se convirtió en préstamo
        Cancelada = 3    // el socio o el bibliotecario la canceló
    }
}