using SB.GestionPagos.Dominio.Enumeraciones;

namespace SB.GestionPagos.Dominio.Repositorios;

/// <summary>
/// Criterios de búsqueda de empleados del RF-03. Todo criterio nulo significa "no filtrar".
/// </summary>
/// <param name="Nombre">Coincidencia parcial contra el primer nombre o el apellido paterno.</param>
/// <param name="Departamento">Departamento exacto al que pertenece el empleado.</param>
/// <param name="Estado">Situación laboral del empleado.</param>
/// <remarks>
/// Se agrupan en un objeto en lugar de pasarlos sueltos para que agregar un criterio más
/// adelante no obligue a cambiar la firma de <see cref="IEmpleadoRepositorio.BuscarPaginaAsync"/>
/// ni a tocar las implementaciones que ya existen.
/// </remarks>
public sealed record FiltroBusquedaEmpleado(
    string? Nombre = null,
    string? Departamento = null,
    EstadoEmpleado? Estado = null);
