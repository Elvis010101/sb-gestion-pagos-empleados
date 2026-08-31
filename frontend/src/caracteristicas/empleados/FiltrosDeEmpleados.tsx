import { useState, type FormEvent } from 'react';

import { Boton } from '../../comunes/componentes/Boton';
import { CampoDeSeleccion } from '../../comunes/componentes/CampoDeSeleccion';
import { CampoDeTexto } from '../../comunes/componentes/CampoDeTexto';

import { CRITERIOS_DE_FILTRO_VACIOS, type CriteriosDeFiltro } from './criteriosDeFiltro';
import estilos from './FiltrosDeEmpleados.module.css';
import { ETIQUETAS_ESTADO_EMPLEADO, EstadoEmpleado } from './tipos';

const OPCIONES_DE_ESTADO = [
  {
    valor: String(EstadoEmpleado.Activo),
    etiqueta: ETIQUETAS_ESTADO_EMPLEADO[EstadoEmpleado.Activo],
  },
  {
    valor: String(EstadoEmpleado.Inactivo),
    etiqueta: ETIQUETAS_ESTADO_EMPLEADO[EstadoEmpleado.Inactivo],
  },
];

interface PropiedadesFiltrosDeEmpleados {
  /** Se avisa hacia arriba solo al confirmar. Ver la nota sobre el borrador, más abajo. */
  alAplicar: (criterios: CriteriosDeFiltro) => void;

  estaCargando: boolean;
}

/**
 * Formulario de filtros de la consulta (RF-03).
 *
 * Guarda un BORRADOR local mientras el usuario escribe y solo avisa al padre cuando este
 * confirma. La alternativa —levantar cada pulsación de tecla al padre— dispararía una
 * consulta al servidor por letra escrita. Es la regla general: el estado vive lo más abajo
 * posible, y sube únicamente cuando otro componente lo necesita. Aquí, el padre solo necesita
 * los criterios ya elegidos.
 */
export function FiltrosDeEmpleados({ alAplicar, estaCargando }: PropiedadesFiltrosDeEmpleados) {
  const [borrador, establecerBorrador] = useState<CriteriosDeFiltro>(CRITERIOS_DE_FILTRO_VACIOS);

  function manejarEnvio(evento: FormEvent<HTMLFormElement>): void {
    // Sin esto el navegador recarga la página entera al enviar el formulario y se pierde
    // todo el estado de la aplicación.
    evento.preventDefault();
    alAplicar(borrador);
  }

  function limpiar(): void {
    establecerBorrador(CRITERIOS_DE_FILTRO_VACIOS);
    alAplicar(CRITERIOS_DE_FILTRO_VACIOS);
  }

  return (
    // Es un `<form>` de verdad y no un montón de campos sueltos: así la tecla Enter dispara
    // la búsqueda, que es lo que cualquiera espera al escribir en un buscador.
    <form className={estilos.filtros} onSubmit={manejarEnvio} role="search">
      <CampoDeTexto
        className={estilos.campo}
        etiqueta="Nombre o apellido"
        placeholder="Coincidencia parcial"
        value={borrador.nombre}
        onChange={(evento) =>
          establecerBorrador((actual) => ({ ...actual, nombre: evento.target.value }))
        }
      />

      <CampoDeTexto
        className={estilos.campo}
        etiqueta="Departamento"
        value={borrador.departamento}
        onChange={(evento) =>
          establecerBorrador((actual) => ({ ...actual, departamento: evento.target.value }))
        }
      />

      <CampoDeSeleccion
        className={estilos.campo}
        etiqueta="Estado"
        opciones={OPCIONES_DE_ESTADO}
        etiquetaDeOpcionVacia="Todos"
        value={borrador.estado}
        onChange={(evento) =>
          establecerBorrador((actual) => ({ ...actual, estado: evento.target.value }))
        }
      />

      <div className={estilos.acciones}>
        <Boton type="submit" estaProcesando={estaCargando}>
          Buscar
        </Boton>
        <Boton variante="secundario" type="button" onClick={limpiar} disabled={estaCargando}>
          Limpiar
        </Boton>
      </div>
    </form>
  );
}
