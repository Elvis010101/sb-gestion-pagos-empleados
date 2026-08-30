using SB.GestionPagos.Dominio.ObjetosDeValor;
using SB.GestionPagos.Dominio.Validaciones;

namespace SB.GestionPagos.Dominio.Entidades;

/// <summary>
/// Empleado pagado por hora, con recargo sobre las horas que exceden la jornada ordinaria.
/// </summary>
public sealed class EmpleadoPorHoras : Empleado
{
    /// <summary>Jornada semanal ordinaria. A partir de aquí, cada hora se paga como extra.</summary>
    public const decimal HORAS_SEMANALES_ESTANDAR = 40m;

    /// <summary>La hora extra se paga a una vez y media el valor de la hora ordinaria.</summary>
    public const decimal FACTOR_HORA_EXTRA = 1.5m;

    /// <summary>Nadie trabaja horas negativas.</summary>
    public const decimal HORAS_MINIMAS_SEMANALES = 0m;

    /// <summary>Cota física de una semana: 7 días × 24 horas.</summary>
    public const decimal HORAS_MAXIMAS_SEMANALES = 168m;

    public EmpleadoPorHoras(
        string primerNombre,
        string apellidoPaterno,
        string numeroSeguroSocial,
        string departamento,
        decimal sueldoPorHora,
        decimal horasTrabajadas)
        : base(primerNombre, apellidoPaterno, numeroSeguroSocial, departamento)
    {
        SueldoPorHora = ValidacionDominio.NoNegativo(sueldoPorHora, nameof(SueldoPorHora));
        HorasTrabajadas = ValidacionDominio.EnRangoInclusivo(
            horasTrabajadas,
            HORAS_MINIMAS_SEMANALES,
            HORAS_MAXIMAS_SEMANALES,
            nameof(HorasTrabajadas));
    }

    public decimal SueldoPorHora { get; private set; }

    /// <summary>
    /// Horas de la semana. Es <c>decimal</c> y no entero porque las jornadas parciales
    /// existen: media hora trabajada es media hora pagada.
    /// </summary>
    public decimal HorasTrabajadas { get; private set; }

    public override string TipoContrato => "Empleado por Horas";

    /// <summary>
    /// Fórmula 2 de la p. 5 del PDF de la prueba:
    /// si horasTrabajadas ≤ 40, pago = sueldoPorHora × horasTrabajadas;
    /// si horasTrabajadas &gt; 40, pago = (sueldoPorHora × 40) + (sueldoPorHora × 1.5 × (horasTrabajadas − 40)).
    /// </summary>
    public override ResultadoPago CalcularDesglosePagoSemanal()
    {
        // La comparación es "menor o igual": con exactamente 40 horas todavía no hay recargo.
        // Esa frontera es el error clásico de esta fórmula y tiene una prueba dedicada.
        if (HorasTrabajadas <= HORAS_SEMANALES_ESTANDAR)
        {
            return new ResultadoPago(
                new LineaCalculo("Horas ordinarias", SueldoPorHora * HorasTrabajadas));
        }

        decimal horasExtra = HorasTrabajadas - HORAS_SEMANALES_ESTANDAR;

        return new ResultadoPago(
            new LineaCalculo("Horas ordinarias", SueldoPorHora * HORAS_SEMANALES_ESTANDAR),
            new LineaCalculo("Horas extra", SueldoPorHora * FACTOR_HORA_EXTRA * horasExtra));
    }

    public void ActualizarDatosDeContrato(decimal sueldoPorHora, decimal horasTrabajadas)
    {
        SueldoPorHora = ValidacionDominio.NoNegativo(sueldoPorHora, nameof(SueldoPorHora));
        HorasTrabajadas = ValidacionDominio.EnRangoInclusivo(
            horasTrabajadas,
            HORAS_MINIMAS_SEMANALES,
            HORAS_MAXIMAS_SEMANALES,
            nameof(HorasTrabajadas));
    }
}
