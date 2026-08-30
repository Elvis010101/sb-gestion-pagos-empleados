using SB.GestionPagos.Api.Configuracion;
using SB.GestionPagos.Api.Filtros;
using SB.GestionPagos.Aplicacion.Configuracion;
using SB.GestionPagos.Infraestructura.Configuracion;
using SB.GestionPagos.Servicios.Configuracion;

// Host de la API. Serilog, CORS y el middleware global de excepciones se agregan en el
// bloque de la capa Api. Aquí no va, ni irá, lógica de negocio.

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// El filtro se registra como filtro global: toda acción valida su entrada sin que su autor
// tenga que acordarse.
builder.Services.AddControllers(opciones => opciones.Filters.Add<FiltroValidacion>());

builder.Services.AgregarDocumentacion();

// Cada capa se registra a sí misma y el host solo las nombra. El orden entre estas cuatro
// llamadas no importa: el contenedor resuelve las dependencias al construirlas, no al
// declararlas.
builder.Services.AgregarAplicacion();
builder.Services.AgregarServicios();
builder.Services.AgregarInfraestructura(builder.Configuration);
builder.Services.AgregarSeguridad(builder.Configuration);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// El ORDEN de aquí abajo no es cosmético; cada pieza depende de lo que dejó la anterior.
//
//   UseRouting          decide QUÉ endpoint atiende la petición.
//   UseRateLimiter      necesita ese endpoint ya resuelto para saber qué política le toca.
//   UseAuthentication   lee la cabecera Authorization y construye el ClaimsPrincipal.
//   UseAuthorization    decide si ese principal puede entrar. Antes de UseAuthentication
//                       vería siempre un usuario anónimo y devolvería 401 con token válido.
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
