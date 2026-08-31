import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';

import { PaginaIniciarSesion } from './caracteristicas/autenticacion/PaginaIniciarSesion';
import { ProveedorSesion } from './caracteristicas/autenticacion/ProveedorSesion';
import { PaginaConsultaEmpleados } from './caracteristicas/empleados/PaginaConsultaEmpleados';
import { PaginaCrearEmpleado } from './caracteristicas/empleados/PaginaCrearEmpleado';
import { PaginaEditarEmpleado } from './caracteristicas/empleados/PaginaEditarEmpleado';
import { PaginaEntidadesGubernamentales } from './caracteristicas/entidadesGubernamentales/PaginaEntidadesGubernamentales';
import { PaginaInicio } from './caracteristicas/inicio/PaginaInicio';
import { PaginaReporteSemanal } from './caracteristicas/reportes/PaginaReporteSemanal';
import { DisenoPrincipal } from './diseno/DisenoPrincipal';
import { RutaProtegida } from './rutas/RutaProtegida';
import { Rutas } from './rutas/rutas';

/**
 * Árbol de rutas de la aplicación.
 *
 * El orden del anidamiento es intencionado: el proveedor de sesión envuelve al enrutador
 * porque la ruta protegida necesita saber si hay sesión ANTES de decidir qué dibuja. Dentro,
 * las pantallas privadas cuelgan de `RutaProtegida`, y estas a su vez del diseño con la barra
 * lateral: así "estar autenticado" y "verse dentro del armazón institucional" quedan
 * declarados una sola vez, y no repetidos en cada pantalla.
 *
 * La pantalla de acceso queda FUERA de ese anidamiento porque no debe mostrar la barra
 * lateral: quien no ha entrado no tiene menú que ver.
 *
 * Ninguna ruta se declara "solo para administradores". La restricción por rol se aplica
 * dentro de cada pantalla, y sobre todo la aplica el servidor: esconder una ruta en el
 * navegador no es una medida de seguridad, porque el código del cliente está en manos de
 * quien lo usa.
 */
export function App() {
  return (
    <ProveedorSesion>
      <BrowserRouter>
        <Routes>
          <Route path={Rutas.IniciarSesion} element={<PaginaIniciarSesion />} />

          <Route element={<RutaProtegida />}>
            <Route element={<DisenoPrincipal />}>
              <Route path={Rutas.Inicio} element={<PaginaInicio />} />
              <Route path={Rutas.ConsultaEmpleados} element={<PaginaConsultaEmpleados />} />
              <Route path={Rutas.CrearRegistro} element={<PaginaCrearEmpleado />} />
              <Route path={Rutas.EditarEmpleado} element={<PaginaEditarEmpleado />} />
              <Route
                path={Rutas.EntidadesGubernamentales}
                element={<PaginaEntidadesGubernamentales />}
              />
              <Route path={Rutas.ReporteSemanal} element={<PaginaReporteSemanal />} />
            </Route>
          </Route>

          {/* Cualquier dirección desconocida vuelve al inicio. Sin esta ruta, un enlace mal
              escrito deja la pantalla en blanco, que es exactamente lo que el enunciado
              prohíbe. */}
          <Route path="*" element={<Navigate to={Rutas.Inicio} replace />} />
        </Routes>
      </BrowserRouter>
    </ProveedorSesion>
  );
}
