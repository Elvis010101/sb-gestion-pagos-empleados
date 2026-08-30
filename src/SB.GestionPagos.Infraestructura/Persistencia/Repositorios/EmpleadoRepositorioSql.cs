using Microsoft.EntityFrameworkCore;
using SB.GestionPagos.Dominio.Entidades;
using SB.GestionPagos.Dominio.Enumeraciones;
using SB.GestionPagos.Dominio.Repositorios;

namespace SB.GestionPagos.Infraestructura.Persistencia.Repositorios;

/// <summary>
/// Implementación de <see cref="IEmpleadoRepositorio"/> sobre SQL Server con EF Core.
/// </summary>
/// <remarks>
/// La regla que gobierna toda esta clase: el filtrado, el orden y el recorte de página se
/// componen sobre <c>IQueryable</c> y NO se ejecutan hasta el <c>await</c> final. Mientras
/// la consulta sigue siendo <c>IQueryable</c>, cada <c>Where</c> se acumula en un árbol de
/// expresiones que el proveedor traduce a un solo SQL. En cuanto se materializa —con
/// <c>ToList</c>, <c>AsEnumerable</c> o un <c>foreach</c>— todo lo que venga después se
/// resuelve en memoria del servidor de aplicación, y el RNF-04 deja de ser alcanzable.
///
/// Es <c>internal</c>: nadie fuera de Infraestructura debe poder nombrarla. Los servicios
/// reciben <c>IEmpleadoRepositorio</c>, y el registro en el contenedor es la única costura
/// donde el contrato se encuentra con esta clase.
/// </remarks>
internal sealed class EmpleadoRepositorioSql : IEmpleadoRepositorio
{
    /// <summary>
    /// Carácter con el que se neutralizan los comodines dentro del texto buscado.
    /// </summary>
    /// <remarks>
    /// En SQL, <c>%</c> y <c>_</c> son comodines de <c>LIKE</c>. Sin escaparlos, buscar el
    /// texto "50%" devolvería la tabla entera, y buscar "_" también. No es una vulnerabilidad
    /// de inyección —el valor sigue viajando como parámetro— pero sí un resultado incorrecto.
    /// </remarks>
    private const string CARACTER_ESCAPE_LIKE = "\\";

    private const string COMODIN_CUALQUIER_TEXTO = "%";

    private const string COMODIN_UN_CARACTER = "_";

    private const string INICIO_CLASE_DE_CARACTERES = "[";

    private readonly GestionPagosDbContext _contexto;

    public EmpleadoRepositorioSql(GestionPagosDbContext contexto)
    {
        _contexto = contexto;
    }

    /// <summary>
    /// Trae un empleado por su identificador, CON seguimiento de cambios.
    /// </summary>
    /// <remarks>
    /// Es la única consulta de esta clase que no lleva <c>AsNoTracking</c>, y es deliberado:
    /// este método alimenta tanto la lectura como la edición y la baja lógica. Los servicios
    /// traen la entidad por aquí, la modifican y llaman a <see cref="ActualizarAsync"/>; si
    /// llegara sin seguimiento, el rastreador de cambios no tendría el estado original contra
    /// el cual comparar y no habría nada que guardar.
    ///
    /// El costo de seguir UNA entidad es despreciable. El de seguir las mil de un reporte no
    /// lo es, y por eso las consultas de colección sí lo desactivan.
    /// </remarks>
    public Task<Empleado?> ObtenerPorIdAsync(int identificador, CancellationToken cancelacion)
        => _contexto.Empleados
            .FirstOrDefaultAsync(empleado => empleado.Id == identificador, cancelacion);

    public async Task<PaginaDeRegistros<Empleado>> BuscarPaginaAsync(
        FiltroBusquedaEmpleado filtro,
        Paginacion paginacion,
        CancellationToken cancelacion)
    {
        IQueryable<Empleado> consultaFiltrada = ConstruirConsultaFiltrada(filtro);

        // Dos viajes a la base y no uno, a propósito. El total corresponde al filtro SIN
        // paginar —es lo que el paginador de la interfaz necesita para saber cuántas páginas
        // hay— y no puede deducirse del tramo devuelto. Ambas consultas parten del mismo
        // IQueryable, así que aplican exactamente el mismo WHERE: no hay forma de que el
        // total y la página respondan a filtros distintos.
        int totalRegistros = await consultaFiltrada.CountAsync(cancelacion);

        List<Empleado> elementos = await OrdenarPorApellido(consultaFiltrada)
            .Skip(paginacion.RegistrosOmitidos)
            .Take(paginacion.TamanoPagina)
            .ToListAsync(cancelacion);

        return new PaginaDeRegistros<Empleado>(elementos, totalRegistros);
    }

    public async Task<IReadOnlyList<Empleado>> ListarAsync(
        FiltroBusquedaEmpleado filtro,
        CancellationToken cancelacion)
        => await OrdenarPorApellido(ConstruirConsultaFiltrada(filtro)).ToListAsync(cancelacion);

    public Task<bool> ExisteNumeroSeguroSocialAsync(
        string numeroSeguroSocial,
        int? identificadorExcluido,
        CancellationToken cancelacion)
    {
        // Se recorta igual que lo hace el Dominio al asignarlo. Sin esto, " 001 " no chocaría
        // con "001" en la comprobación previa, pero sí en el índice único: el usuario vería
        // un error del motor en vez del mensaje de conflicto.
        string numeroNormalizado = numeroSeguroSocial.Trim();

        IQueryable<Empleado> consulta = _contexto.Empleados
            .Where(empleado => empleado.NumeroSeguroSocial == numeroNormalizado);

        // La exclusión se decide en C# y no dentro de la expresión. Escribirla como
        // `identificadorExcluido == null || empleado.Id != identificadorExcluido` funcionaría,
        // pero metería un `@p IS NULL` en el SQL que impide al motor reutilizar un buen plan.
        if (identificadorExcluido is not null)
        {
            int identificador = identificadorExcluido.Value;
            consulta = consulta.Where(empleado => empleado.Id != identificador);
        }

        // AnyAsync no necesita AsNoTracking: devuelve un bool, no entidades. El SQL que
        // genera es un EXISTS, así que el motor se detiene en la primera fila que coincide
        // en vez de contarlas todas.
        return consulta.AnyAsync(cancelacion);
    }

    public async Task AgregarAsync(Empleado empleado, CancellationToken cancelacion)
    {
        // Add y no AddAsync: la versión asíncrona solo aporta algo con generadores de valor
        // que consultan la base (HiLo). Con IDENTITY, el identificador lo asigna SQL Server
        // al insertar, y EF lo escribe de vuelta en la entidad durante SaveChanges.
        _contexto.Empleados.Add(empleado);

        await _contexto.SaveChangesAsync(cancelacion);
    }

    public async Task ActualizarAsync(Empleado empleado, CancellationToken cancelacion)
    {
        // Si la entidad la trajo este mismo contexto, el rastreador ya sabe qué propiedades
        // cambiaron y generará un UPDATE solo con esas columnas. Si viene desarraigada —de
        // otro contexto, o de una prueba— hay que adjuntarla explícitamente; sin esta guarda,
        // SaveChanges no encontraría nada que hacer y el método fallaría en silencio, que es
        // la peor forma de fallar.
        if (_contexto.Entry(empleado).State == EntityState.Detached)
        {
            _contexto.Empleados.Update(empleado);
        }

        await _contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>
    /// Compone los criterios del RF-03 sobre la consulta, sin ejecutarla.
    /// </summary>
    /// <remarks>
    /// Cada <c>if</c> añade un <c>Where</c> al árbol de expresiones. Un criterio ausente no
    /// añade nada: por eso "no filtrar" no cuesta una cláusula inútil en el SQL final.
    /// </remarks>
    private IQueryable<Empleado> ConstruirConsultaFiltrada(FiltroBusquedaEmpleado filtro)
    {
        // AsNoTracking desde el principio: esta consulta alimenta lecturas, y seguir cada
        // entidad devuelta costaría una copia de sus valores originales por fila.
        IQueryable<Empleado> consulta = _contexto.Empleados.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Nombre))
        {
            string patron = ConstruirPatronDeCoincidenciaParcial(filtro.Nombre);

            // El RF-03 pide buscar por nombre sin decir por cuál de los dos, así que se
            // busca en ambos. La comparación no distingue mayúsculas porque la intercalación
            // por omisión de SQL Server no las distingue; hacerlo en C# con ToLower()
            // impediría al motor usar el índice.
            consulta = consulta.Where(empleado =>
                EF.Functions.Like(empleado.PrimerNombre, patron, CARACTER_ESCAPE_LIKE)
                || EF.Functions.Like(empleado.ApellidoPaterno, patron, CARACTER_ESCAPE_LIKE));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Departamento))
        {
            // La variable local existe para que el árbol de expresiones capture un string y
            // no la propiedad de un objeto: EF lo traduce a un parámetro limpio.
            string departamento = filtro.Departamento.Trim();

            consulta = consulta.Where(empleado => empleado.Departamento == departamento);
        }

        if (filtro.Estado is not null)
        {
            EstadoEmpleado estado = filtro.Estado.Value;

            consulta = consulta.Where(empleado => empleado.Estado == estado);
        }

        return consulta;
    }

    /// <summary>
    /// Orden estable del listado: apellido, nombre y, como desempate, el identificador.
    /// </summary>
    /// <remarks>
    /// El desempate por identificador no es cosmético. <c>OFFSET</c>/<c>FETCH</c> exige un
    /// <c>ORDER BY</c>, y si ese orden no es total —dos empleados llamados igual— el motor
    /// puede devolverlos en cualquier orden entre una consulta y otra: el mismo empleado
    /// aparecería dos veces en la página 2 y desaparecería de la 3.
    /// </remarks>
    private static IOrderedQueryable<Empleado> OrdenarPorApellido(IQueryable<Empleado> consulta)
        => consulta
            .OrderBy(empleado => empleado.ApellidoPaterno)
            .ThenBy(empleado => empleado.PrimerNombre)
            .ThenBy(empleado => empleado.Id);

    /// <summary>
    /// Convierte el texto buscado en un patrón <c>LIKE</c> con los comodines neutralizados.
    /// </summary>
    private static string ConstruirPatronDeCoincidenciaParcial(string textoBuscado)
    {
        string textoEscapado = textoBuscado
            .Trim()
            // El propio carácter de escape va primero: si se hiciera al final, volvería a
            // escapar las barras que acaban de introducir los reemplazos anteriores.
            .Replace(CARACTER_ESCAPE_LIKE, CARACTER_ESCAPE_LIKE + CARACTER_ESCAPE_LIKE)
            .Replace(COMODIN_CUALQUIER_TEXTO, CARACTER_ESCAPE_LIKE + COMODIN_CUALQUIER_TEXTO)
            .Replace(COMODIN_UN_CARACTER, CARACTER_ESCAPE_LIKE + COMODIN_UN_CARACTER)
            .Replace(INICIO_CLASE_DE_CARACTERES, CARACTER_ESCAPE_LIKE + INICIO_CLASE_DE_CARACTERES);

        return COMODIN_CUALQUIER_TEXTO + textoEscapado + COMODIN_CUALQUIER_TEXTO;
    }
}
