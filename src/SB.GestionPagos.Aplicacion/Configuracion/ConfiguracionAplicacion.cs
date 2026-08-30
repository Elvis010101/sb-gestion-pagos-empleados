using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace SB.GestionPagos.Aplicacion.Configuracion;

/// <summary>
/// Registro en el contenedor de dependencias de lo que aporta la capa Aplicación.
/// </summary>
/// <remarks>
/// La capa se registra a sí misma. La alternativa —que el proyecto Api recorriera el
/// ensamblado de Aplicación buscando validadores— obligaría al host a conocer la estructura
/// interna de otra capa; aquí basta con que llame a un método.
/// </remarks>
public static class ConfiguracionAplicacion
{
    /// <summary>
    /// Registra todos los validadores de FluentValidation declarados en esta capa.
    /// </summary>
    /// <remarks>
    /// Se registran como <c>Singleton</c> porque un validador no guarda estado entre
    /// llamadas: construir uno nuevo por petición sería trabajo desperdiciado en cada
    /// solicitud.
    /// </remarks>
    public static IServiceCollection AgregarAplicacion(this IServiceCollection servicios)
    {
        servicios.AddValidatorsFromAssembly(
            Assembly.GetExecutingAssembly(),
            ServiceLifetime.Singleton);

        return servicios;
    }
}
