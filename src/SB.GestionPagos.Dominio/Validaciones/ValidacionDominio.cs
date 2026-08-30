using SB.GestionPagos.Dominio.Excepciones;

namespace SB.GestionPagos.Dominio.Validaciones;

/// <summary>
/// Guardas reutilizables para las invariantes del Dominio.
/// </summary>
/// <remarks>
/// Es <c>internal</c> a propósito: es andamiaje interno del Dominio, no parte del contrato
/// público que consumen Aplicación, Servicios o Infraestructura. Esas capas nunca deben
/// invocar estas guardas por su cuenta; ven la regla a través de la excepción que reciben.
/// </remarks>
internal static class ValidacionDominio
{
    private const decimal VALOR_MINIMO_NO_NEGATIVO = 0m;

    /// <summary>
    /// Exige que el texto tenga contenido real y lo devuelve sin espacios sobrantes,
    /// para que " Juan " y "Juan" no se almacenen como dos valores distintos.
    /// </summary>
    internal static string TextoRequerido(string? valor, string nombrePropiedad)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ExcepcionValorRequerido(nombrePropiedad);
        }

        return valor.Trim();
    }

    internal static decimal NoNegativo(decimal valor, string nombrePropiedad)
    {
        if (valor < VALOR_MINIMO_NO_NEGATIVO)
        {
            throw new ExcepcionValorFueraDeRango(nombrePropiedad, valor, "no puede ser negativo");
        }

        return valor;
    }

    internal static decimal EnRangoInclusivo(
        decimal valor,
        decimal valorMinimo,
        decimal valorMaximo,
        string nombrePropiedad)
    {
        if (valor < valorMinimo || valor > valorMaximo)
        {
            throw new ExcepcionValorFueraDeRango(
                nombrePropiedad,
                valor,
                FormattableString.Invariant($"debe estar entre {valorMinimo} y {valorMaximo}"));
        }

        return valor;
    }
}
