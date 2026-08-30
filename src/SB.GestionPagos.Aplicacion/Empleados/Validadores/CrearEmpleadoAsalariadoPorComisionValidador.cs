using FluentValidation;
using SB.GestionPagos.Aplicacion.Empleados.Dtos;
using SB.GestionPagos.Aplicacion.Validaciones;
using SB.GestionPagos.Dominio.Entidades;

namespace SB.GestionPagos.Aplicacion.Empleados.Validadores;

/// <summary>Valida la forma del DTO de alta de un Empleado Asalariado por Comisión.</summary>
public sealed class CrearEmpleadoAsalariadoPorComisionValidador
    : AbstractValidator<CrearEmpleadoAsalariadoPorComisionDto>
{
    public CrearEmpleadoAsalariadoPorComisionValidador()
    {
        RuleFor(solicitud => solicitud.PrimerNombre).TextoObligatorio(LongitudMaxima.PRIMER_NOMBRE);
        RuleFor(solicitud => solicitud.ApellidoPaterno).TextoObligatorio(LongitudMaxima.APELLIDO_PATERNO);
        RuleFor(solicitud => solicitud.NumeroSeguroSocial).TextoObligatorio(LongitudMaxima.NUMERO_SEGURO_SOCIAL);
        RuleFor(solicitud => solicitud.Departamento).TextoObligatorio(LongitudMaxima.DEPARTAMENTO);
        RuleFor(solicitud => solicitud.VentasBrutas).MontoNoNegativo();
        RuleFor(solicitud => solicitud.TarifaComision).EnRangoInclusivo(
            EmpleadoAsalariadoPorComision.TARIFA_COMISION_MINIMA,
            EmpleadoAsalariadoPorComision.TARIFA_COMISION_MAXIMA);
        RuleFor(solicitud => solicitud.SalarioBase).MontoNoNegativo();
    }
}
