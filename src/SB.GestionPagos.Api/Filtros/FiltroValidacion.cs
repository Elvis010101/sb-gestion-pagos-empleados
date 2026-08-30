using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SB.GestionPagos.Api.Filtros;

/// <summary>
/// Ejecuta el validador de FluentValidation que corresponda a cada argumento de la acción,
/// antes de que la acción llegue a ejecutarse.
/// </summary>
/// <remarks>
/// Los validadores se escribieron en la capa Aplicación en un bloque anterior, pero nadie los
/// invocaba: registrarlos en el contenedor no hace que se ejecuten. Este filtro es la costura
/// que los conecta con el canal HTTP.
///
/// Se escribe a mano y no con el paquete <c>FluentValidation.AspNetCore</c> por dos razones:
/// ese paquete no está en la lista de dependencias autorizadas, y está marcado como obsoleto
/// por sus propios autores desde la versión 11.
///
/// Es un filtro global y no un <c>if</c> dentro de cada acción: así ninguna acción futura
/// puede olvidarse de validar su entrada, que es el olvido que convierte una entrada mal
/// formada en una excepción a mitad del caso de uso.
/// </remarks>
public sealed class FiltroValidacion : IAsyncActionFilter
{
    private readonly IServiceProvider _proveedorDeServicios;

    public FiltroValidacion(IServiceProvider proveedorDeServicios)
    {
        _proveedorDeServicios = proveedorDeServicios;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext contexto, ActionExecutionDelegate continuar)
    {
        foreach (object? argumento in contexto.ActionArguments.Values)
        {
            if (argumento is null)
            {
                continue;
            }

            // El validador se busca por el tipo REAL del argumento: IValidator<CrearEmpleadoDto>,
            // IValidator<SolicitudInicioSesionDto>… Si no hay ninguno registrado para ese tipo
            // —un `int` de ruta, por ejemplo—, no hay nada que validar.
            Type tipoDelValidador = typeof(IValidator<>).MakeGenericType(argumento.GetType());

            if (_proveedorDeServicios.GetService(tipoDelValidador) is not IValidator validador)
            {
                continue;
            }

            ValidationResult resultado = await validador.ValidateAsync(
                new ValidationContext<object>(argumento),
                contexto.HttpContext.RequestAborted);

            if (resultado.IsValid)
            {
                continue;
            }

            contexto.Result = new BadRequestObjectResult(
                new ValidationProblemDetails(ConstruirEstadoDelModelo(resultado)));

            // Corta el flujo: la acción no llega a ejecutarse y el caso de uso nunca ve un
            // dato inválido.
            return;
        }

        await continuar();
    }

    /// <summary>
    /// Traduce los fallos de FluentValidation al formato estándar de errores de ASP.NET Core.
    /// </summary>
    /// <remarks>
    /// El resultado se serializa como <c>ProblemDetails</c> (RFC 7807), que es el mismo
    /// contrato de error que ya emite el framework: el frontend escribe UN solo manejador de
    /// errores y no dos.
    /// </remarks>
    private static ModelStateDictionary ConstruirEstadoDelModelo(ValidationResult resultado)
    {
        ModelStateDictionary estadoDelModelo = new();

        foreach (ValidationFailure fallo in resultado.Errors)
        {
            estadoDelModelo.AddModelError(fallo.PropertyName, fallo.ErrorMessage);
        }

        return estadoDelModelo;
    }
}
