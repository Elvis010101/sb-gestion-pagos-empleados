namespace SB.GestionPagos.Api.Middleware;

/// <summary>
/// Métodos de extensión para insertar los middlewares propios en el canal.
/// </summary>
/// <remarks>
/// Existen para que <c>Program.cs</c> se lea como una lista de pasos en español
/// —<c>UsarManejoDeExcepciones()</c>, <c>UsarCorrelacion()</c>— en lugar de una fila de
/// <c>UseMiddleware&lt;...&gt;()</c> con nombres de clase. El canal es lo más importante que
/// se declara en el arranque y tiene que poder leerse de un vistazo.
/// </remarks>
internal static class ExtensionesDeCanalizacion
{
    internal static IApplicationBuilder UsarManejoDeExcepciones(this IApplicationBuilder aplicacion)
        => aplicacion.UseMiddleware<MiddlewareManejoExcepciones>();

    internal static IApplicationBuilder UsarCorrelacion(this IApplicationBuilder aplicacion)
        => aplicacion.UseMiddleware<MiddlewareCorrelacion>();
}
