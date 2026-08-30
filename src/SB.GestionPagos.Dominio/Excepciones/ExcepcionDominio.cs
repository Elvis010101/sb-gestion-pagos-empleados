namespace SB.GestionPagos.Dominio.Excepciones;

/// <summary>
/// Raíz de todas las excepciones que representan la violación de una regla de negocio.
/// </summary>
/// <remarks>
/// Existe para que las capas externas distingan con un solo <c>catch</c> entre
/// "el dato que llegó viola una regla del negocio" (se traduce a HTTP 400) y
/// "algo se rompió" (se traduce a HTTP 500), sin tener que enumerar cada excepción
/// concreta ni volver a listarlas cada vez que se agregue una nueva.
/// </remarks>
public abstract class ExcepcionDominio : Exception
{
    protected ExcepcionDominio(string mensaje)
        : base(mensaje)
    {
    }
}
