namespace SB.GestionPagos.Dominio.Excepciones;

/// <summary>
/// Se lanza cuando la operación es válida en sí misma, pero no en el estado actual del objeto
/// (por ejemplo, reasignar el identificador de una entidad que ya lo tiene).
/// </summary>
public sealed class ExcepcionOperacionNoPermitida : ExcepcionDominio
{
    public ExcepcionOperacionNoPermitida(string mensaje)
        : base(mensaje)
    {
    }
}
