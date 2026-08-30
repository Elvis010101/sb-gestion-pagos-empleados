using SB.GestionPagos.Dominio.ObjetosDeValor;
using SB.GestionPagos.Dominio.Validaciones;

namespace SB.GestionPagos.Dominio.Entidades;

/// <summary>
/// Empleado con un salario semanal fijo, independiente de las horas trabajadas.
/// </summary>
public sealed class EmpleadoAsalariado : Empleado
{
    public EmpleadoAsalariado(
        string primerNombre,
        string apellidoPaterno,
        string numeroSeguroSocial,
        string departamento,
        decimal salarioSemanal)
        : base(primerNombre, apellidoPaterno, numeroSeguroSocial, departamento)
    {
        SalarioSemanal = ValidacionDominio.NoNegativo(salarioSemanal, nameof(SalarioSemanal));
    }

    public decimal SalarioSemanal { get; private set; }

    public override string TipoContrato => "Empleado Asalariado";

    /// <summary>
    /// Fórmula 1 de la p. 5 del PDF de la prueba: pago semanal = salarioSemanal.
    /// </summary>
    public override ResultadoPago CalcularDesglosePagoSemanal()
        => new(new LineaCalculo("Salario semanal", SalarioSemanal));

    public void ActualizarDatosDeContrato(decimal salarioSemanal)
        => SalarioSemanal = ValidacionDominio.NoNegativo(salarioSemanal, nameof(SalarioSemanal));
}
