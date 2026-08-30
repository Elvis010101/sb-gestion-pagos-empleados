namespace SB.GestionPagos.Dominio.Excepciones;

/// <summary>
/// Se lanza cuando un dato obligatorio del negocio llega vacío, nulo o solo con espacios.
/// </summary>
public sealed class ExcepcionValorRequerido : ExcepcionDominio
{
    public ExcepcionValorRequerido(string nombrePropiedad)
        : base($"El campo '{nombrePropiedad}' es obligatorio y no puede quedar vacío.")
    {
        NombrePropiedad = nombrePropiedad;
    }

    /// <summary>
    /// Propiedad que incumplió la regla. Se expone como dato, y no solo dentro del mensaje,
    /// para que la capa Api pueda señalar el campo exacto en la respuesta de error.
    /// </summary>
    public string NombrePropiedad { get; }
}
