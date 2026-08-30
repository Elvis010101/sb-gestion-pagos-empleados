namespace SB.GestionPagos.Api.Configuracion;

/// <summary>
/// Rutas que atiende el host y que no pertenecen a ningún controlador.
/// </summary>
/// <remarks>
/// La ruta de salud se nombra en dos sitios —donde se publica el endpoint y donde el
/// registro decide bajarle la severidad— y tienen que ser el mismo texto. Con la ruta
/// escrita a mano en ambos, cambiarla en uno solo dejaría el registro inundado de sondas
/// sin que nada fallara de forma visible.
/// </remarks>
internal static class RutasDelHost
{
    /// <summary>Comprobación de vida del proceso.</summary>
    internal const string SALUD = "/salud";
}
