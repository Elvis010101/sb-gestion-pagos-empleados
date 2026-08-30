using System.Diagnostics.CodeAnalysis;
using SB.GestionPagos.Dominio.Excepciones;
using SB.GestionPagos.Dominio.Validaciones;

namespace SB.GestionPagos.Dominio.Entidades;

/// <summary>
/// Entidad gubernamental de la República Dominicana, del mantenimiento que exige el
/// documento de especificaciones técnicas (p. 3).
/// </summary>
public sealed class EntidadGubernamental
{
    private const int IDENTIFICADOR_NO_ASIGNADO = 0;

    private const int IDENTIFICADOR_MINIMO = 1;

    public EntidadGubernamental(string nombre, string categoria, string poderDelEstado, string sector)
    {
        EstablecerDatos(nombre, categoria, poderDelEstado, sector);
    }

    public int Id { get; private set; }

    public string Nombre { get; private set; }

    public string Categoria { get; private set; }

    public string PoderDelEstado { get; private set; }

    public string Sector { get; private set; }

    /// <summary>
    /// Fija el identificador una única vez.
    /// </summary>
    /// <remarks>
    /// Este módulo se persiste en archivo de texto plano, así que no hay motor de base de
    /// datos que genere el identificador: lo asigna el repositorio. Se expone como operación
    /// explícita, y no como setter público, para que reasignarlo sea imposible por accidente.
    /// </remarks>
    public void AsignarIdentificador(int identificador)
    {
        if (Id != IDENTIFICADOR_NO_ASIGNADO)
        {
            throw new ExcepcionOperacionNoPermitida(
                $"La entidad gubernamental '{Nombre}' ya tiene asignado el identificador {Id}.");
        }

        if (identificador < IDENTIFICADOR_MINIMO)
        {
            throw new ExcepcionValorFueraDeRango(
                nameof(Id),
                identificador,
                FormattableString.Invariant($"debe ser {IDENTIFICADOR_MINIMO} o mayor"));
        }

        Id = identificador;
    }

    public void Actualizar(string nombre, string categoria, string poderDelEstado, string sector)
        => EstablecerDatos(nombre, categoria, poderDelEstado, sector);

    [MemberNotNull(nameof(Nombre), nameof(Categoria), nameof(PoderDelEstado), nameof(Sector))]
    private void EstablecerDatos(string nombre, string categoria, string poderDelEstado, string sector)
    {
        // Validar todo antes de asignar nada, por el mismo motivo que en Empleado. Aquí
        // pesa además que el repositorio de este módulo reescribe el archivo de texto plano
        // completo: una entidad medio actualizada en memoria acabaría como una línea
        // corrupta en disco, y el archivo no tiene transacciones que lo deshagan.
        string nombreValidado = ValidacionDominio.TextoRequerido(nombre, nameof(Nombre));
        string categoriaValidada = ValidacionDominio.TextoRequerido(categoria, nameof(Categoria));
        string poderDelEstadoValidado =
            ValidacionDominio.TextoRequerido(poderDelEstado, nameof(PoderDelEstado));
        string sectorValidado = ValidacionDominio.TextoRequerido(sector, nameof(Sector));

        Nombre = nombreValidado;
        Categoria = categoriaValidada;
        PoderDelEstado = poderDelEstadoValidado;
        Sector = sectorValidado;
    }
}
