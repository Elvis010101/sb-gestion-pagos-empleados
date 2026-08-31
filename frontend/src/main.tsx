import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';

import { App } from './App';
import './comunes/estilos/variables.css';
import './comunes/estilos/global.css';

const elementoRaiz = document.getElementById('root');

// El elemento puede no existir si alguien edita el index.html: fallar aquí con un mensaje
// claro es mucho mejor que el "Cannot read properties of null" que saldría dos líneas abajo.
if (elementoRaiz === null) {
  throw new Error('No se encontró el elemento raíz #root en el documento.');
}

createRoot(elementoRaiz).render(
  <StrictMode>
    <App />
  </StrictMode>,
);
