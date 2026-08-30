using SB.GestionPagos.Api.Configuracion;
using SB.GestionPagos.Api.Errores;
using SB.GestionPagos.Api.Filtros;
using SB.GestionPagos.Api.Middleware;
using SB.GestionPagos.Aplicacion.Configuracion;
using SB.GestionPagos.Infraestructura.Configuracion;
using SB.GestionPagos.Servicios.Configuracion;
using Serilog;

// Host de la API. Aquí no va, ni irá, lógica de negocio: solo el registro de las capas y la
// declaración del canal por el que pasa cada petición.

// El registrador de arranque existe antes que la aplicación. Sin él, un fallo de arranque
// —falta la cadena de conexión, falta la clave de firma del token— mataría el proceso sin
// dejar rastro en ningún sitio.
Log.Logger = ConfiguracionRegistro.CrearRegistradorDeArranque();

try
{
    Log.Information("Iniciando el host de SB.GestionPagos.");

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    // Sustituye el registro por omisión de .NET por Serilog en TODA la aplicación, incluidas
    // las capas que solo conocen ILogger<T>: siguen registrando contra la misma interfaz y
    // sus eventos acaban en la consola y en el archivo sin que ellas se enteren.
    builder.Host.UseSerilog(ConfiguracionRegistro.Configurar);

    // El filtro se registra como filtro global: toda acción valida su entrada sin que su autor
    // tenga que acordarse.
    builder.Services.AddControllers(opciones => opciones.Filters.Add<FiltroValidacion>());

    builder.Services.AgregarContratoDeErrores();
    builder.Services.AddHealthChecks();

    builder.Services.AgregarDocumentacion();
    builder.Services.AgregarPoliticaDeCors(builder.Configuration);

    // Cada capa se registra a sí misma y el host solo las nombra. El orden entre estas cuatro
    // llamadas no importa: el contenedor resuelve las dependencias al construirlas, no al
    // declararlas.
    builder.Services.AgregarAplicacion();
    builder.Services.AgregarServicios();
    builder.Services.AgregarInfraestructura(builder.Configuration);
    builder.Services.AgregarSeguridad(builder.Configuration);

    WebApplication app = builder.Build();

    // ------------------------------------------------------------------------------------
    // EL CANAL. El orden de aquí abajo no es cosmético: cada pieza envuelve a las siguientes
    // y depende de lo que dejó la anterior. Leído de arriba abajo es el viaje de entrada de
    // la petición; de abajo arriba, el de salida de la respuesta.
    // ------------------------------------------------------------------------------------

    // PRIMERO, siempre. Un middleware solo puede atrapar lo que ocurre por debajo de él.
    // Puesto al final no cubriría nada, y las excepciones de enrutamiento, autenticación o
    // límite de frecuencia saldrían sin registro y sin contrato de error.
    app.UsarManejoDeExcepciones();

    // Antes del registro de peticiones: asigna el identificador que después aparece en cada
    // línea del archivo, incluidas las que escriben las capas de más adentro.
    app.UsarCorrelacion();

    app.UsarRegistroDePeticiones();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    //   UseRouting          decide QUÉ endpoint atiende la petición.
    //   UseCors             responde el preflight y corta ahí. Antes del límite de
    //                       frecuencia a propósito: una comprobación previa del navegador no
    //                       debe gastar el cupo del usuario.
    //   UseRateLimiter      necesita el endpoint ya resuelto para saber qué política le toca.
    //   UseAuthentication   lee la cabecera Authorization y construye el ClaimsPrincipal.
    //   UseAuthorization    decide si ese principal puede entrar. Antes de UseAuthentication
    //                       vería siempre un usuario anónimo y devolvería 401 con token válido.
    app.UseRouting();
    app.UseCors(ConfiguracionCors.POLITICA_FRONTEND);
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // AllowAnonymous es imprescindible: la política de reserva del host exige un usuario
    // autenticado en todo endpoint que no diga lo contrario, y una sonda de salud que
    // necesitara credenciales no serviría para lo único que existe.
    app.MapHealthChecks(RutasDelHost.SALUD).AllowAnonymous();

    app.Run();

    return 0;
}

// HostAbortedException NO es un fallo: es lo que lanzan las herramientas `dotnet ef` cuando
// construyen el host para leer el DbContext y lo abortan a propósito. Tratarla como error
// haría que cada migración terminara con un registro fatal y un código de salida 1.
catch (Exception excepcion) when (excepcion is not HostAbortedException)
{
    Log.Fatal(excepcion, "El host terminó de forma inesperada.");
    return 1;
}
finally
{
    // Los sinks de Serilog escriben en segundo plano. Sin este vaciado, las últimas líneas
    // —justamente las del fallo que tumbó el proceso— se perderían al morir el proceso.
    Log.CloseAndFlush();
}
