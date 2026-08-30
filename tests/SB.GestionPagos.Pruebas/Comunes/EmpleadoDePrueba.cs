using SB.GestionPagos.Dominio.Entidades;

namespace SB.GestionPagos.Pruebas.Comunes;

/// <summary>
/// Fábrica de empleados válidos para las pruebas.
/// </summary>
/// <remarks>
/// Patrón Object Mother. Los cuatro constructores del Dominio exigen los mismos cuatro datos
/// personales, que a casi ningún test le importan: lo que se está probando es el sueldo, las
/// horas o la comisión. Repetirlos en cada prueba llenaría los tests de ruido y escondería el
/// único dato que sí varía.
///
/// La consecuencia práctica es que cada prueba solo escribe los valores que forman parte de
/// lo que verifica. Cuando una falla, lo que se lee en la línea del test es exactamente la
/// regla de negocio en discusión.
/// </remarks>
internal static class EmpleadoDePrueba
{
    internal const string PRIMER_NOMBRE = "Ana";

    internal const string APELLIDO_PATERNO = "Rodríguez";

    internal const string NUMERO_SEGURO_SOCIAL = "402-1234567-8";

    internal const string DEPARTAMENTO = "Tecnología";

    internal static EmpleadoAsalariado Asalariado(
        decimal salarioSemanal,
        string primerNombre = PRIMER_NOMBRE,
        string apellidoPaterno = APELLIDO_PATERNO,
        string numeroSeguroSocial = NUMERO_SEGURO_SOCIAL,
        string departamento = DEPARTAMENTO)
        => new(primerNombre, apellidoPaterno, numeroSeguroSocial, departamento, salarioSemanal);

    internal static EmpleadoPorHoras PorHoras(
        decimal sueldoPorHora,
        decimal horasTrabajadas,
        string primerNombre = PRIMER_NOMBRE,
        string apellidoPaterno = APELLIDO_PATERNO,
        string numeroSeguroSocial = NUMERO_SEGURO_SOCIAL,
        string departamento = DEPARTAMENTO)
        => new(
            primerNombre,
            apellidoPaterno,
            numeroSeguroSocial,
            departamento,
            sueldoPorHora,
            horasTrabajadas);

    internal static EmpleadoPorComision PorComision(
        decimal ventasBrutas,
        decimal tarifaComision,
        string primerNombre = PRIMER_NOMBRE,
        string apellidoPaterno = APELLIDO_PATERNO,
        string numeroSeguroSocial = NUMERO_SEGURO_SOCIAL,
        string departamento = DEPARTAMENTO)
        => new(
            primerNombre,
            apellidoPaterno,
            numeroSeguroSocial,
            departamento,
            ventasBrutas,
            tarifaComision);

    internal static EmpleadoAsalariadoPorComision AsalariadoPorComision(
        decimal ventasBrutas,
        decimal tarifaComision,
        decimal salarioBase,
        string primerNombre = PRIMER_NOMBRE,
        string apellidoPaterno = APELLIDO_PATERNO,
        string numeroSeguroSocial = NUMERO_SEGURO_SOCIAL,
        string departamento = DEPARTAMENTO)
        => new(
            primerNombre,
            apellidoPaterno,
            numeroSeguroSocial,
            departamento,
            ventasBrutas,
            tarifaComision,
            salarioBase);
}
