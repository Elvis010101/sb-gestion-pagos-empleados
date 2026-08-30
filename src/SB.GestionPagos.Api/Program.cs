// Host de la API. En este bloque solo se levanta el esqueleto: Serilog, Swagger con
// Bearer, JWT, CORS, rate limiting y el middleware de excepciones se agregan en el
// bloque correspondiente. Aquí no va, ni irá, lógica de negocio.

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
