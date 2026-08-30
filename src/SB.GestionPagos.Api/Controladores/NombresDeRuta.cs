namespace SB.GestionPagos.Api.Controladores;

/// <summary>
/// Nombres de las rutas a las que hace falta apuntar desde otro sitio del código.
/// </summary>
/// <remarks>
/// Una creación debe responder 201 con la cabecera <c>Location</c> apuntando al recurso
/// recién creado. Esa URL se puede construir de dos formas: nombrando la acción y el
/// controlador de destino (<c>CreatedAtAction</c>) o nombrando la RUTA
/// (<c>CreatedAtRoute</c>). Se usa la segunda: con la primera, renombrar un método de C#
/// —algo que un IDE hace sin avisar— rompería la cabecera en tiempo de ejecución y solo se
/// notaría probando.
/// </remarks>
internal static class NombresDeRuta
{
    internal const string OBTENER_EMPLEADO = "ObtenerEmpleado";

    internal const string OBTENER_ENTIDAD_GUBERNAMENTAL = "ObtenerEntidadGubernamental";
}
