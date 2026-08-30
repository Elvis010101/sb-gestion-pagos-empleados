using FluentValidation;
using SB.GestionPagos.Aplicacion.Empleados.Dtos;
using SB.GestionPagos.Aplicacion.Validaciones;
using SB.GestionPagos.Dominio.Entidades;

namespace SB.GestionPagos.Aplicacion.Empleados.Validadores;

/// <summary>Valida la forma del DTO de edición de un Empleado por Comisión.</summary>
public sealed class ActualizarEmpleadoPorComisionValidador : AbstractValidator<ActualizarEmpleadoPorComisionDto>
{
    public ActualizarEmpleadoPorComisionValidador()
    {
        RuleFor(solicitud => solicitud.PrimerNombre).TextoObligatorio(LongitudMaxima.PRIMER_NOMBRE);
        RuleFor(solicitud => solicitud.ApellidoPaterno).TextoObligatorio(LongitudMaxima.APELLIDO_PATERNO);
        RuleFor(solicitud => solicitud.NumeroSeguroSocial).TextoObligatorio(LongitudMaxima.NUMERO_SEGURO_SOCIAL);
        RuleFor(solicitud => solicitud.Departamento).TextoObligatorio(LongitudMaxima.DEPARTAMENTO);
        RuleFor(solicitud => solicitud.Estado).ValorDeEnumeracionDefinido();
        RuleFor(solicitud => solicitud.VentasBrutas).MontoNoNegativo();
        RuleFor(solicitud => solicitud.TarifaComision).EnRangoInclusivo(
            EmpleadoPorComision.TARIFA_COMISION_MINIMA,
            EmpleadoPorComision.TARIFA_COMISION_MAXIMA);
    }
}
