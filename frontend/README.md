# SB.GestionPagos — Frontend

Interfaz del Sistema de Gestión de Pagos de la Superintendencia de Bancos.
React 18 + TypeScript + Vite, sin librerías de componentes: la maqueta se replica con CSS
Modules propios.

## Requisitos

- Node.js 20.19 o superior (Vite 8 no arranca por debajo de esa versión).
- La API en ejecución. Por omisión se espera en `http://localhost:5122/api`.

## Puesta en marcha

```bash
npm install
cp .env.example .env.development   # opcional: solo si la API no está en el puerto por omisión
npm run dev
```

La aplicación queda en `http://localhost:5173`. **El puerto es fijo a propósito**
(`strictPort` en `vite.config.ts`): la política de CORS del backend autoriza ese origen y
solo ese, así que si Vite se moviera a otro puerto el navegador bloquearía todas las
respuestas.

### Usuarios de prueba

Los siembra la migración inicial de la base de datos:

| Usuario   | Contraseña     | Rol           | Puede             |
| --------- | -------------- | ------------- | ----------------- |
| `admin`   | `Admin123!`    | Administrador | Leer y escribir   |
| `usuario` | `Usuario123!`  | Usuario       | Solo leer         |

## Comandos

| Comando          | Qué hace                                                    |
| ---------------- | ----------------------------------------------------------- |
| `npm run dev`    | Servidor de desarrollo con recarga en caliente.              |
| `npm run build`  | Verifica tipos con `tsc` y genera la versión de producción.  |
| `npm run lint`   | ESLint. `@typescript-eslint/no-explicit-any` es **error**.   |
| `npm run format` | Aplica Prettier.                                             |

## Organización

El corte es **por características**, no por capas técnicas:

```
src/
├── activos/           Logo e ícono institucionales
├── caracteristicas/   Un módulo por dominio: sus tipos, su cliente de API y sus pantallas
│   ├── autenticacion/
│   ├── empleados/
│   ├── entidadesGubernamentales/
│   ├── inicio/
│   └── reportes/
├── comunes/           Transversal: cliente HTTP, componentes base, estilos, tipos compartidos
├── diseno/            El armazón de Maqueta.jpeg: barra lateral, encabezado, tarjeta
└── rutas/             Declaración única de rutas y guardia de sesión
```

La alternativa —`components/`, `services/`, `types/`— obliga a abrir cuatro carpetas para
tocar una sola pantalla y no deja ver qué se puede borrar cuando un módulo deja de hacer
falta. Con este corte, es una carpeta.

## Pantallas

| Ruta                          | Qué hace                                                                  | Escritura |
| ----------------------------- | ------------------------------------------------------------------------- | --------- |
| `/acceso`                     | Inicio de sesión. Única pantalla pública.                                  | —         |
| `/`                           | Inicio, con accesos directos según el rol.                                 | —         |
| `/consulta`                   | Empleados con filtros por nombre, departamento y estado, y paginación.     | Admin     |
| `/crear-registro`             | Alta. El formulario cambia sus campos según el tipo de contrato.           | Admin     |
| `/empleados/:id/editar`       | Edición. Detecta el tipo desde el empleado y recalcula el pago al guardar. | Admin     |
| `/entidades-gubernamentales`  | Catálogo con búsqueda por nombre y sector, y CRUD completo.                | Admin     |
| `/reporte-semanal`            | Nómina de la semana con el desglose del cálculo por empleado.              | —         |

Con el rol `Usuario`, las acciones de escritura no se dibujan. **Ocultarlas es cortesía, no
seguridad**: quien escriba la dirección a mano llega igual, y ahí el que dice que no es el
403 del servidor.

## Decisiones que conviene conocer

- **El token vive en `localStorage`, y solo el token.** Ni el nombre ni el rol se guardan: el
  almacenamiento local lo puede editar cualquiera, así que la identidad se le pide al servidor
  con `GET /autenticacion/sesion`, que la lee del token firmado. El backend autentica por
  encabezado `Authorization` y su CORS no habilita credenciales, de modo que una cookie
  `httpOnly` —más resistente a XSS— exigiría cambiar también el backend.
- **Los enums viajan como números.** El host no registra `JsonStringEnumConverter`, así que
  `estado` y `rol` llegan como `1` o `2`. Se modelan con objeto congelado más tipo unión y no
  con `enum` de TypeScript, que `erasableSyntaxOnly` prohíbe.
- **Un solo cliente HTTP con interceptores.** El token se inyecta en un punto y el 401 se
  maneja en un punto. Ningún componente conoce axios: todos ven `ErrorApi`.
- **El formulario de empleado no sabe cuántos tipos de contrato existen.** Cada tipo se
  declara una vez en `caracteristicas/empleados/configuracionDeTiposDeEmpleado.ts`: sus
  campos, sus límites y cómo llamar a su propio endpoint. El formulario recorre esa lista y
  el esquema de Zod se construye a partir de ella, así que **agregar un quinto tipo de
  empleado es agregar una entrada al registro** — el formulario, la tabla y el enrutador no
  se tocan. Es el espejo del polimorfismo del Dominio, donde cada entidad sabe calcular su
  propio pago y ningún servicio pregunta de qué tipo es nadie.
- **`strict` está activado en el `tsconfig`.** La plantilla de Vite no lo trae, y sin él la
  promesa de "ningún `any`" no significa nada, porque los nulos pasan sin control.
