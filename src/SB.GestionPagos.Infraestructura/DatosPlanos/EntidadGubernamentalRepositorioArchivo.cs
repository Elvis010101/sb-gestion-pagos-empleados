using System.Globalization;
using SB.GestionPagos.Dominio.Entidades;
using SB.GestionPagos.Dominio.Excepciones;
using SB.GestionPagos.Dominio.Repositorios;

namespace SB.GestionPagos.Infraestructura.DatosPlanos;

/// <summary>
/// Implementación de <see cref="IEntidadGubernamentalRepositorio"/> sobre un archivo de texto
/// plano, tal como exige el documento de especificaciones técnicas para este módulo.
/// </summary>
/// <remarks>
/// Tres decisiones gobiernan la clase entera:
///
/// 1. UN ÚNICO ESCRITOR A LA VEZ. Todas las operaciones pasan por un <see cref="SemaphoreSlim"/>
///    de un solo permiso. No es un <c>lock</c> porque un <c>lock</c> no se puede sostener a
///    través de un <c>await</c>, y aquí hay E/S de disco de por medio. Para que el semáforo
///    signifique algo, el repositorio se registra como Singleton: si fuera Scoped, cada
///    petición traería su propio semáforo y no habría exclusión mutua entre peticiones.
///
/// 2. EL DISCO MANDA, LA MEMORIA SIGUE. El catálogo se cachea en memoria, pero la referencia
///    solo se reemplaza DESPUÉS de que la escritura en disco terminó bien. Si el disco falla,
///    la memoria se queda en el último estado que sí está persistido. Nunca hay una entidad
///    que exista para la aplicación y no para el archivo.
///
/// 3. LA ESCRITURA ES ATÓMICA. Se escribe un archivo temporal completo y recién entonces se
///    renombra sobre el definitivo. Un renombre es una operación atómica del sistema de
///    archivos: si el proceso muere a mitad de camino, en disco queda el archivo viejo entero
///    o el nuevo entero, jamás uno a medias.
///
/// Se descartó tomar el semáforo solo en las escrituras y dejar las lecturas sin bloqueo
/// —viable, porque las escrituras reemplazan la lista completa en vez de mutarla, así que un
/// lector siempre vería una instantánea coherente—. Habría exigido marcar el campo como
/// <c>volatile</c> para garantizar la visibilidad entre núcleos, y sobre 181 registros
/// servidos desde memoria el bloqueo no se nota. Se prefirió la versión obviamente correcta.
///
/// Es <c>internal</c>, como el repositorio de SQL Server: fuera de Infraestructura nadie debe
/// poder nombrarla.
/// </remarks>
internal sealed class EntidadGubernamentalRepositorioArchivo : IEntidadGubernamentalRepositorio, IDisposable
{
    /// <summary>Identificador de la primera entidad de un catálogo vacío.</summary>
    private const int PRIMER_IDENTIFICADOR = 1;

    /// <summary>
    /// Extensión del archivo intermedio sobre el que se escribe antes de renombrar.
    /// Vive en el mismo directorio que el definitivo a propósito: un renombre solo es
    /// atómico dentro del mismo volumen.
    /// </summary>
    private const string SUFIJO_ARCHIVO_TEMPORAL = ".tmp";

    private const int TAMANO_BUFFER_ESCRITURA = 4096;

    private readonly SemaphoreSlim _cerrojo = new(initialCount: 1, maxCount: 1);

    private readonly string _rutaArchivo;

    /// <summary>Catálogo en memoria. <c>null</c> mientras no se haya leído el archivo.</summary>
    private List<RegistroEntidadGubernamental>? _catalogo;

    /// <summary>
    /// Líneas de comentario iniciales del archivo, que se reescriben tal cual.
    /// </summary>
    /// <remarks>
    /// Se conservan en vez de regenerarse desde una constante para que leer y volver a
    /// escribir el archivo sin cambios produzca un archivo idéntico byte a byte. Ese
    /// invariante es lo que hace que un alta genere un diff de una sola línea en git.
    /// </remarks>
    private IReadOnlyList<string> _cabecera = Array.Empty<string>();

    /// <summary>
    /// El constructor es público aunque la clase sea <c>internal</c>: el contenedor de
    /// dependencias solo mira los constructores públicos al activar un tipo. La clase sigue
    /// siendo invisible fuera del ensamblado, que es lo que interesa.
    /// </summary>
    public EntidadGubernamentalRepositorioArchivo(OpcionesArchivoEntidadesGubernamentales opciones)
    {
        _rutaArchivo = opciones.RutaArchivo;
    }

    public async Task<IReadOnlyList<EntidadGubernamental>> ObtenerTodasAsync(CancellationToken cancelacion)
    {
        await _cerrojo.WaitAsync(cancelacion);
        try
        {
            await CargarSiHaceFaltaAsync(cancelacion);
            return Materializar(_catalogo!);
        }
        finally
        {
            _cerrojo.Release();
        }
    }

    public async Task<EntidadGubernamental?> ObtenerPorIdAsync(int identificador, CancellationToken cancelacion)
    {
        await _cerrojo.WaitAsync(cancelacion);
        try
        {
            await CargarSiHaceFaltaAsync(cancelacion);

            RegistroEntidadGubernamental? registro =
                _catalogo!.Find(candidato => candidato.Id == identificador);

            // Se devuelve una entidad recién construida y no una instancia compartida. El
            // caché es único para toda la aplicación —el repositorio es Singleton—, así que
            // entregar la misma instancia dejaría que un servicio, al llamar a Actualizar(),
            // modificara lo que otras peticiones están leyendo en ese mismo instante, y
            // encima antes de que el cambio esté en disco. EF Core no necesita esta precaución
            // porque cada petición trae su propio DbContext con su propio rastreador.
            return registro is null ? null : AEntidad(registro);
        }
        finally
        {
            _cerrojo.Release();
        }
    }

    public async Task<IReadOnlyList<EntidadGubernamental>> BuscarAsync(
        FiltroBusquedaEntidadGubernamental filtro,
        CancellationToken cancelacion)
    {
        await _cerrojo.WaitAsync(cancelacion);
        try
        {
            await CargarSiHaceFaltaAsync(cancelacion);

            // El filtrado es un recorrido lineal en memoria. Sobre 181 registros es
            // instantáneo; sobre los empleados sería inadmisible, y por eso ese repositorio
            // compone un IQueryable que SQL Server resuelve con índices.
            List<RegistroEntidadGubernamental> coincidencias =
                _catalogo!.FindAll(registro => Coincide(registro, filtro));

            return Materializar(coincidencias);
        }
        finally
        {
            _cerrojo.Release();
        }
    }

    public async Task AgregarAsync(EntidadGubernamental entidadGubernamental, CancellationToken cancelacion)
    {
        await _cerrojo.WaitAsync(cancelacion);
        try
        {
            await CargarSiHaceFaltaAsync(cancelacion);

            int nuevoIdentificador = SiguienteIdentificador(_catalogo!);

            List<RegistroEntidadGubernamental> catalogoPropuesto = new(_catalogo!)
            {
                ARegistro(entidadGubernamental, nuevoIdentificador),
            };

            await ConfirmarAsync(catalogoPropuesto, cancelacion);

            // El identificador se sella en la entidad del llamador al final, y no antes: si
            // la escritura hubiera fallado, el servicio se queda con una entidad sin Id
            // —coherente con que el alta no ocurrió— en vez de una que dice tenerlo y no
            // existe en ninguna parte. AsignarIdentificador solo admite una llamada.
            entidadGubernamental.AsignarIdentificador(nuevoIdentificador);
        }
        finally
        {
            _cerrojo.Release();
        }
    }

    public async Task ActualizarAsync(EntidadGubernamental entidadGubernamental, CancellationToken cancelacion)
    {
        await _cerrojo.WaitAsync(cancelacion);
        try
        {
            await CargarSiHaceFaltaAsync(cancelacion);

            int posicion = _catalogo!.FindIndex(registro => registro.Id == entidadGubernamental.Id);
            if (posicion < 0)
            {
                throw new InvalidOperationException(DesapareceEntre("actualizar", entidadGubernamental.Id));
            }

            List<RegistroEntidadGubernamental> catalogoPropuesto = new(_catalogo!);
            catalogoPropuesto[posicion] = ARegistro(entidadGubernamental, entidadGubernamental.Id);

            await ConfirmarAsync(catalogoPropuesto, cancelacion);
        }
        finally
        {
            _cerrojo.Release();
        }
    }

    public async Task EliminarAsync(EntidadGubernamental entidadGubernamental, CancellationToken cancelacion)
    {
        await _cerrojo.WaitAsync(cancelacion);
        try
        {
            await CargarSiHaceFaltaAsync(cancelacion);

            List<RegistroEntidadGubernamental> catalogoPropuesto =
                _catalogo!.FindAll(registro => registro.Id != entidadGubernamental.Id);

            if (catalogoPropuesto.Count == _catalogo!.Count)
            {
                throw new InvalidOperationException(DesapareceEntre("eliminar", entidadGubernamental.Id));
            }

            await ConfirmarAsync(catalogoPropuesto, cancelacion);
        }
        finally
        {
            _cerrojo.Release();
        }
    }

    public void Dispose() => _cerrojo.Dispose();

    /// <summary>
    /// Lee el archivo la primera vez que se necesita. Se invoca siempre con el cerrojo tomado.
    /// </summary>
    /// <remarks>
    /// La carga es perezosa y no ocurre al construir el objeto porque un constructor no puede
    /// hacer E/S asíncrona sin bloquear un hilo del grupo de subprocesos.
    /// </remarks>
    private async Task CargarSiHaceFaltaAsync(CancellationToken cancelacion)
    {
        if (_catalogo is not null)
        {
            return;
        }

        if (!File.Exists(_rutaArchivo))
        {
            // Falla ruidosa y con la ruta absoluta en el mensaje. La causa real casi siempre
            // es que el archivo no se copió a la salida del build, y ese diagnóstico es
            // imposible de adivinar desde un "objeto no encontrado" genérico.
            throw new FileNotFoundException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "No se encontró el archivo de entidades gubernamentales en '{0}'. "
                    + "Verifique que el .csproj lo copie a la salida del build o configure la clave '{1}'.",
                    _rutaArchivo,
                    OpcionesArchivoEntidadesGubernamentales.CLAVE_CONFIGURACION),
                _rutaArchivo);
        }

        string[] lineas = await File.ReadAllLinesAsync(
            _rutaArchivo,
            FormatoArchivoEntidadesGubernamentales.Codificacion,
            cancelacion);

        List<string> cabecera = new();
        List<RegistroEntidadGubernamental> catalogo = new(lineas.Length);
        HashSet<int> identificadoresYaVistos = new();
        bool yaEmpezaronLosDatos = false;

        for (int indice = 0; indice < lineas.Length; indice++)
        {
            string linea = lineas[indice];

            // Los números de línea del mensaje de error se cuentan como los cuenta un editor
            // de texto, desde 1, para que quien abra el archivo llegue directo a la línea mala.
            int numeroLinea = indice + 1;

            if (!FormatoArchivoEntidadesGubernamentales.EsLineaDeDatos(linea))
            {
                if (!yaEmpezaronLosDatos)
                {
                    cabecera.Add(linea);
                }

                continue;
            }

            yaEmpezaronLosDatos = true;

            RegistroEntidadGubernamental registro =
                FormatoArchivoEntidadesGubernamentales.DesdeLinea(linea, numeroLinea);

            // El identificador es la clave primaria del catálogo. Un archivo de texto no tiene
            // quien haga cumplir la unicidad, así que la hace cumplir la carga.
            if (!identificadoresYaVistos.Add(registro.Id))
            {
                throw new InvalidDataException(
                    FormatoArchivoEntidadesGubernamentales.DescribirLineaInvalida(
                        numeroLinea,
                        FormattableString.Invariant($"repite el identificador {registro.Id}")));
            }

            // Construir la entidad valida las invariantes del Dominio (campos obligatorios,
            // identificador positivo) y el resultado se descarta. Es deliberado: así un
            // archivo corrupto revienta al cargar, con el número de línea, y no en la primera
            // consulta que por casualidad toque ese registro.
            ValidarContraElDominio(registro, numeroLinea);

            catalogo.Add(registro);
        }

        _cabecera = cabecera;
        _catalogo = catalogo;
    }

    /// <summary>
    /// Persiste el catálogo propuesto y, solo si el disco lo aceptó, lo publica en memoria.
    /// </summary>
    private async Task ConfirmarAsync(
        List<RegistroEntidadGubernamental> catalogoPropuesto,
        CancellationToken cancelacion)
    {
        // La cancelación se comprueba ANTES de empezar a escribir y no durante. Abandonar una
        // escritura a la mitad no aporta nada: el trabajo ya está hecho y lo único que se
        // lograría es dejar un archivo temporal huérfano.
        cancelacion.ThrowIfCancellationRequested();

        await EscribirDeFormaAtomicaAsync(catalogoPropuesto);

        _catalogo = catalogoPropuesto;
    }

    private async Task EscribirDeFormaAtomicaAsync(List<RegistroEntidadGubernamental> catalogoPropuesto)
    {
        string rutaTemporal = _rutaArchivo + SUFIJO_ARCHIVO_TEMPORAL;

        await using (FileStream flujo = new(rutaTemporal, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await using (StreamWriter escritor = new(
                flujo,
                FormatoArchivoEntidadesGubernamentales.Codificacion,
                TAMANO_BUFFER_ESCRITURA,
                leaveOpen: true))
            {
                escritor.NewLine = FormatoArchivoEntidadesGubernamentales.FIN_DE_LINEA;

                foreach (string lineaDeCabecera in _cabecera)
                {
                    await escritor.WriteLineAsync(lineaDeCabecera);
                }

                // Se ordena por identificador para que el archivo tenga un orden estable y
                // reproducible. Sin esto, dos ejecuciones podrían escribir las mismas 181
                // entidades en distinto orden y git mostraría un archivo entero modificado.
                foreach (RegistroEntidadGubernamental registro in catalogoPropuesto.OrderBy(candidato => candidato.Id))
                {
                    await escritor.WriteLineAsync(FormatoArchivoEntidadesGubernamentales.ALinea(registro));
                }
            }

            // Vaciar el StreamWriter solo empuja los bytes al sistema operativo, que los deja
            // en su propio caché. Esto los baja al disco físico. Consistencia y durabilidad son
            // dos problemas distintos: el renombre atómico resuelve el primero (nunca se ve un
            // archivo a medias) y esta línea el segundo (un corte de energía no se lleva el
            // cambio que la Api ya reportó como exitoso).
            flujo.Flush(flushToDisk: true);
        }

        // El renombre es el instante en que el cambio se vuelve visible, y es indivisible.
        // Se prefirió a File.Replace, que conserva los permisos del destino pero exige que el
        // destino exista: aquí eso convertiría un archivo borrado por error en una excepción
        // en cada escritura en vez de una recuperación limpia.
        File.Move(rutaTemporal, _rutaArchivo, overwrite: true);
    }

    private static bool Coincide(RegistroEntidadGubernamental registro, FiltroBusquedaEntidadGubernamental filtro)
    {
        if (!string.IsNullOrWhiteSpace(filtro.Nombre)
            && !ComparadorTextoCatalogo.Contiene(registro.Nombre ?? string.Empty, filtro.Nombre.Trim()))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(filtro.Sector)
            && !ComparadorTextoCatalogo.SonIguales(registro.Sector ?? string.Empty, filtro.Sector.Trim()))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Ordena por nombre y convierte a entidades del Dominio.
    /// </summary>
    /// <remarks>
    /// El archivo se guarda por identificador y se devuelve por nombre. Son dos criterios con
    /// dos propósitos: el del archivo busca diffs mínimos, el de la respuesta busca una lista
    /// que una persona pueda recorrer.
    /// </remarks>
    private static IReadOnlyList<EntidadGubernamental> Materializar(
        IEnumerable<RegistroEntidadGubernamental> registros)
        => registros
            .OrderBy(registro => registro.Nombre, StringComparer.InvariantCulture)
            .Select(AEntidad)
            .ToList();

    private static EntidadGubernamental AEntidad(RegistroEntidadGubernamental registro)
    {
        EntidadGubernamental entidad = new(
            registro.Nombre!,
            registro.Categoria!,
            registro.PoderDelEstado!,
            registro.Sector!);

        entidad.AsignarIdentificador(registro.Id);

        return entidad;
    }

    private static RegistroEntidadGubernamental ARegistro(
        EntidadGubernamental entidadGubernamental,
        int identificador)
        => new(
            identificador,
            entidadGubernamental.Nombre,
            entidadGubernamental.Categoria,
            entidadGubernamental.PoderDelEstado,
            entidadGubernamental.Sector);

    private static void ValidarContraElDominio(RegistroEntidadGubernamental registro, int numeroLinea)
    {
        try
        {
            AEntidad(registro);
        }
        catch (ExcepcionDominio excepcion)
        {
            // Se traduce a InvalidDataException porque desde afuera esto no es "un dato de
            // negocio inválido que el usuario mandó" —que sería un 400— sino "el almacén está
            // corrupto", que es un 500. La excepción original viaja dentro para no perder el
            // nombre del campo que falló.
            throw new InvalidDataException(
                FormatoArchivoEntidadesGubernamentales.DescribirLineaInvalida(numeroLinea, "tiene datos inválidos"),
                excepcion);
        }
    }

    private static int SiguienteIdentificador(List<RegistroEntidadGubernamental> catalogo)
        // Se toma el mayor y se suma uno, en vez de usar la cantidad de registros. Los
        // identificadores de las entidades eliminadas NO se reciclan: si el 42 se dio de baja
        // y una entidad nueva heredara su número, cualquier enlace guardado apuntaría a un
        // registro distinto del que se guardó.
        => catalogo.Count == 0
            ? PRIMER_IDENTIFICADOR
            : catalogo.Max(registro => registro.Id) + 1;

    private static string DesapareceEntre(string operacion, int identificador)
        => string.Format(
            CultureInfo.InvariantCulture,
            "No se pudo {0} la entidad gubernamental {1}: ya no está en el catálogo. "
            + "Otra petición la eliminó entre la lectura y la escritura.",
            operacion,
            identificador);
}
