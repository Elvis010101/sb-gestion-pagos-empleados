using SB.GestionPagos.Dominio.ObjetosDeValor;
using SB.GestionPagos.Dominio.Validaciones;

namespace SB.GestionPagos.Dominio.Entidades;

/// <summary>
/// Empleado que cobra únicamente un porcentaje de lo que vende.
/// </summary>
public sealed class EmpleadoPorComision : Empleado
{
    /// <summary>Una comisión de 0 % es válida; una negativa, no.</summary>
    public const decimal TARIFA_COMISION_MINIMA = 0m;

    /// <summary>La tarifa se expresa como fracción, de modo que 1 equivale al 100 % de las ventas.</summary>
    public const decimal TARIFA_COMISION_MAXIMA = 1m;

    public EmpleadoPorComision(
        string primerNombre,
        string apellidoPaterno,
        string numeroSeguroSocial,
        string departamento,
        decimal ventasBrutas,
        decimal tarifaComision)
        : base(primerNombre, apellidoPaterno, numeroSeguroSocial, departamento)
    {
        // El alta delega en la edición: una sola redacción de las reglas del contrato.
        // La clase es sealed, así que no hay despacho virtual hacia una subclase.
        ActualizarDatosDeContrato(ventasBrutas, tarifaComision);
    }

    public decimal VentasBrutas { get; private set; }

    /// <summary>
    /// Fracción de las ventas que le corresponde al empleado: 0.10 significa 10 %.
    /// </summary>
    public decimal TarifaComision { get; private set; }

    public override string TipoContrato => "Empleado por Comisión";

    /// <summary>
    /// Fórmula 3 de la p. 5 del PDF de la prueba: pago semanal = ventasBrutas × tarifaComision.
    /// </summary>
    public override ResultadoPago CalcularDesglosePagoSemanal()
        => new(new LineaCalculo("Comisión sobre ventas brutas", VentasBrutas * TarifaComision));

    /// <summary>
    /// Aplica los datos del contrato. O se aplican todos, o no se aplica ninguno.
    /// </summary>
    public void ActualizarDatosDeContrato(decimal ventasBrutas, decimal tarifaComision)
    {
        // Validar todo antes de asignar nada: una tarifa fuera de rango no debe dejar al
        // empleado con las ventas nuevas y la comisión vieja.
        decimal ventasBrutasValidadas =
            ValidacionDominio.NoNegativo(ventasBrutas, nameof(VentasBrutas));
        decimal tarifaComisionValidada = ValidacionDominio.EnRangoInclusivo(
            tarifaComision,
            TARIFA_COMISION_MINIMA,
            TARIFA_COMISION_MAXIMA,
            nameof(TarifaComision));

        VentasBrutas = ventasBrutasValidadas;
        TarifaComision = tarifaComisionValidada;
    }
}
