using System.Globalization;

namespace SB.GestionPagos.Servicios.Empleados;

/// <summary>
/// Mensajes de error del módulo de empleados.
/// </summary>
/// <remarks>
/// Centralizados para que el mismo fallo se describa igual venga de donde venga: cinco
/// servicios distintos pueden responder "no encontrado" y el usuario debe leer siempre lo
/// mismo. También deja un solo lugar que tocar si el sistema se traduce.
/// </remarks>
internal static class MensajesEmpleado
{
    internal static string NoEncontrado(int identificador)
        => string.Format(
            CultureInfo.InvariantCulture,
            "No existe un empleado con el identificador {0}.",
            identificador);

    internal static string TipoDeContratoDistinto(int identificador, string tipoContratoReal)
        => string.Format(
            CultureInfo.InvariantCulture,
            "El empleado {0} es un '{1}'. Debe editarse con la operación correspondiente a ese tipo de contrato.",
            identificador,
            tipoContratoReal);

    internal static string NumeroSeguroSocialDuplicado()
        => "Ya existe un empleado registrado con ese número de seguro social.";
}
