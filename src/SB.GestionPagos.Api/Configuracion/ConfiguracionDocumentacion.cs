using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SB.GestionPagos.Api.Configuracion;

/// <summary>
/// Swagger / OpenAPI con soporte de token Bearer (RNF-09).
/// </summary>
internal static class ConfiguracionDocumentacion
{
    private const string VERSION_DOCUMENTO = "v1";

    private const string TITULO_DOCUMENTO = "SB.GestionPagos";

    private const string DESCRIPCION_DOCUMENTO =
        "Sistema de gestión de pagos de empleados de la Superintendencia de Bancos. " +
        "Todos los endpoints exigen un token, salvo el inicio de sesión.";

    /// <summary>
    /// Identificador del esquema de seguridad dentro del documento OpenAPI.
    /// </summary>
    /// <remarks>
    /// Tiene que ser el mismo en la definición y en la referencia que la exige. Si no
    /// coinciden, Swagger dibuja el botón "Authorize" y aun así no manda la cabecera: un
    /// fallo silencioso que solo se nota probando.
    /// </remarks>
    private const string IDENTIFICADOR_ESQUEMA_SEGURIDAD = "Bearer";

    private const string ESQUEMA_HTTP_BEARER = "bearer";

    private const string FORMATO_TOKEN = "JWT";

    private const string DESCRIPCION_ESQUEMA_SEGURIDAD =
        "Autenticación JWT. Obtenga el token en POST /api/autenticacion/inicio-sesion y " +
        "péguelo aquí SOLO el token, sin escribir la palabra Bearer delante.";

    internal static IServiceCollection AgregarDocumentacion(this IServiceCollection servicios)
    {
        servicios.AddEndpointsApiExplorer();

        servicios.AddSwaggerGen(opciones =>
        {
            opciones.SwaggerDoc(VERSION_DOCUMENTO, new OpenApiInfo
            {
                Version = VERSION_DOCUMENTO,
                Title = TITULO_DOCUMENTO,
                Description = DESCRIPCION_DOCUMENTO
            });

            AgregarSoporteDeTokenBearer(opciones);
            AgregarComentariosDelCodigo(opciones);
        });

        return servicios;
    }

    /// <summary>
    /// Declara el esquema de seguridad y lo aplica a todas las operaciones.
    /// </summary>
    /// <remarks>
    /// El tipo es <c>Http</c> con esquema <c>bearer</c>, y no <c>ApiKey</c> en la cabecera.
    /// Con <c>ApiKey</c> —que es como se ve en muchos ejemplos— el usuario tiene que escribir
    /// a mano "Bearer " delante del token, y olvidarlo produce un 401 que parece un problema
    /// de credenciales. Declarándolo como HTTP Bearer, Swagger arma la cabecera completa.
    /// </remarks>
    private static void AgregarSoporteDeTokenBearer(SwaggerGenOptions opciones)
    {
        opciones.AddSecurityDefinition(IDENTIFICADOR_ESQUEMA_SEGURIDAD, new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = ESQUEMA_HTTP_BEARER,
            BearerFormat = FORMATO_TOKEN,
            In = ParameterLocation.Header,
            Description = DESCRIPCION_ESQUEMA_SEGURIDAD
        });

        // Requisito global: se aplica a todos los endpoints. Los marcados con
        // [AllowAnonymous] siguen funcionando sin token; lo único que ocurre es que Swagger
        // manda la cabecera y el servidor la ignora.
        opciones.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = IDENTIFICADOR_ESQUEMA_SEGURIDAD
                    }
                },
                Array.Empty<string>()
            }
        });
    }

    /// <summary>
    /// Vuelca los comentarios XML del proyecto en la documentación publicada.
    /// </summary>
    /// <remarks>
    /// Es lo que hace que el <c>&lt;summary&gt;</c> de cada acción aparezca en la página de
    /// Swagger en lugar de una lista de rutas sin explicar. El archivo lo genera
    /// <c>GenerateDocumentationFile</c>, ya activado en Directory.Build.props.
    /// </remarks>
    private static void AgregarComentariosDelCodigo(SwaggerGenOptions opciones)
    {
        string nombreArchivoXml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        string rutaArchivoXml = Path.Combine(AppContext.BaseDirectory, nombreArchivoXml);

        if (File.Exists(rutaArchivoXml))
        {
            opciones.IncludeXmlComments(rutaArchivoXml);
        }
    }
}
