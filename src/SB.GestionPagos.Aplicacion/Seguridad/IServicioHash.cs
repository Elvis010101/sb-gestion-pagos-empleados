namespace SB.GestionPagos.Aplicacion.Seguridad;

/// <summary>
/// Convierte contraseñas en hashes y verifica una contraseña contra un hash guardado.
/// </summary>
/// <remarks>
/// La Aplicación no sabe que detrás hay BCrypt. Esa ignorancia es el punto: si mañana el
/// algoritmo cambia, se reemplaza la implementación en Infraestructura y ni esta interfaz ni
/// el servicio de autenticación se enteran.
///
/// No hay un método "obtener la contraseña": un hash es de una sola dirección, y el contrato
/// lo refleja.
/// </remarks>
public interface IServicioHash
{
    string GenerarHash(string contrasenaEnClaro);

    /// <summary>
    /// Indica si la contraseña recibida corresponde al hash almacenado.
    /// </summary>
    /// <remarks>
    /// La comparación la hace la implementación, no quien llama: el algoritmo necesita
    /// extraer del propio hash la sal y el costo con que fue creado, y además debe comparar
    /// en tiempo constante para no filtrar información por la duración de la respuesta.
    /// </remarks>
    bool Verificar(string contrasenaEnClaro, string hashAlmacenado);
}
