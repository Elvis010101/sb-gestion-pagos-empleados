using SB.GestionPagos.Dominio.ObjetosDeValor;
using SB.GestionPagos.Dominio.Validaciones;

namespace SB.GestionPagos.Dominio.Entidades;

/// <summary>
/// Empleado que cobra comisión sobre sus ventas más un salario base con bonificación.
/// </summary>
public sealed class EmpleadoAsalariadoPorComision : Empleado
{
    public const decimal TARIFA_COMISION_MINIMA = 0m;

    public const decimal TARIFA_COMISION_MAXIMA = 1m;

    /// <summary>Bonificación del 10 % que la empresa paga sobre el salario base.</summary>
    public const decimal PORCENTAJE_BONIFICACION_SALARIO_BASE = 0.10m;

    public EmpleadoAsalariadoPorComision(
        string primerNombre,
        string apellidoPaterno,
        string numeroSeguroSocial,
        string departamento,
        decimal ventasBrutas,
        decimal tarifaComision,
        decimal salarioBase)
        : base(primerNombre, apellidoPaterno, numeroSeguroSocial, departamento)
    {
        // El alta delega en la edición: una sola redacción de las reglas del contrato.
        // La clase es sealed, así que no hay despacho virtual hacia una subclase.
        ActualizarDatosDeContrato(ventasBrutas, tarifaComision, salarioBase);
    }

    public decimal VentasBrutas { get; private set; }

    /// <summary>Fracción de las ventas que le corresponde al empleado: 0.10 significa 10 %.</summary>
    public decimal TarifaComision { get; private set; }

    public decimal SalarioBase { get; private set; }

    public override string TipoContrato => "Empleado Asalariado por Comisión";

    /// <summary>
    /// Fórmula 4 de la p. 5 del PDF de la prueba:
    /// pago semanal = (ventasBrutas × tarifaComision) + salarioBase + (salarioBase × 0.10).
    /// </summary>
    public override ResultadoPago CalcularDesglosePagoSemanal()
        => new(
            new LineaCalculo("Comisión sobre ventas brutas", VentasBrutas * TarifaComision),
            new LineaCalculo("Salario base", SalarioBase),
            new LineaCalculo(
                "Bonificación sobre el salario base",
                SalarioBase * PORCENTAJE_BONIFICACION_SALARIO_BASE));

    /// <summary>
    /// Aplica los datos del contrato. O se aplican todos, o no se aplica ninguno.
    /// </summary>
    /// <remarks>
    /// Es el tipo con más margen para dejar basura si se validara y asignara campo por campo:
    /// un salario base negativo llegaría después de haber pisado ya las ventas y la comisión.
    /// </remarks>
    public void ActualizarDatosDeContrato(decimal ventasBrutas, decimal tarifaComision, decimal salarioBase)
    {
        decimal ventasBrutasValidadas =
            ValidacionDominio.NoNegativo(ventasBrutas, nameof(VentasBrutas));
        decimal tarifaComisionValidada = ValidacionDominio.EnRangoInclusivo(
            tarifaComision,
            TARIFA_COMISION_MINIMA,
            TARIFA_COMISION_MAXIMA,
            nameof(TarifaComision));
        decimal salarioBaseValidado = ValidacionDominio.NoNegativo(salarioBase, nameof(SalarioBase));

        VentasBrutas = ventasBrutasValidadas;
        TarifaComision = tarifaComisionValidada;
        SalarioBase = salarioBaseValidado;
    }
}
