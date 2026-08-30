namespace SB.GestionPagos.Dominio.Excepciones;

/// <summary>
/// Se lanza cuando un valor numérico del negocio queda fuera del rango admitido
/// (horas negativas, tarifa de comisión mayor que 100 %, salario negativo).
/// </summary>
public sealed class ExcepcionValorFueraDeRango : ExcepcionDominio
{
    /// <param name="nombrePropiedad">Propiedad que incumplió la regla.</param>
    /// <param name="valorRecibido">Valor que se intentó asignar.</param>
    /// <param name="restriccionEsperada">
    /// Descripción en lenguaje humano de la regla incumplida, por ejemplo "no puede ser negativo".
    /// Se recibe ya redactada para que esta excepción sirva a cualquier tipo de cota sin
    /// necesitar un constructor distinto por cada forma de rango.
    /// </param>
    public ExcepcionValorFueraDeRango(string nombrePropiedad, decimal valorRecibido, string restriccionEsperada)
        : base(FormattableString.Invariant(
            $"El campo '{nombrePropiedad}' recibió el valor {valorRecibido}, pero {restriccionEsperada}."))
    {
        NombrePropiedad = nombrePropiedad;
        ValorRecibido = valorRecibido;
    }

    public string NombrePropiedad { get; }

    public decimal ValorRecibido { get; }
}
