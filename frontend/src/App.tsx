import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';

import { PaginaIniciarSesion } from './caracteristicas/autenticacion/PaginaIniciarSesion';
import { ProveedorSesion } from './caracteristicas/autenticacion/ProveedorSesion';
import { PaginaInicio } from './caracteristicas/inicio/PaginaInicio';
import { PaginaEnConstruccion } from './comunes/componentes/PaginaEnConstruccion';
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
              <Route
                path={Rutas.ConsultaEmpleados}
                element={<PaginaEnConstruccion titulo="Consulta de empleados" />}
              />
              <Route
                path={Rutas.CrearRegistro}
                element={<PaginaEnConstruccion titulo="Crear registro" />}
              />
              <Route
                path={Rutas.EditarEmpleado}
                element={<PaginaEnConstruccion titulo="Editar empleado" />}
              />
              <Route
                path={Rutas.EntidadesGubernamentales}
                element={<PaginaEnConstruccion titulo="Entidades gubernamentales" />}
              />
              <Route
                path={Rutas.ReporteSemanal}
                element={<PaginaEnConstruccion titulo="Reporte semanal" />}
              />
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
