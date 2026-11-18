# Workspace de Yaak para CheckInApi

Esta carpeta contiene un workspace de [Yaak](https://yaak.app) en formato **Directory Sync**:
un archivo YAML por recurso (`yaak.<id>.yaml`). Al abrirlo, Yaak importa el workspace y deja
la carpeta sincronizada, así que los cambios que hagas en la app se guardan acá y se pueden
versionar con Git.

## Cómo abrirlo (una sola vez)

1. Iniciá la API desde la raíz del proyecto:

   ```bash
   dotnet run --launch-profile http
   ```

2. En Yaak, abrí el menú de workspaces (arriba a la izquierda) → **New Workspace** → **Open Folder**.
3. Elegí esta carpeta (`CheckInApi/yaak`) y confirmá la importación.

Queda creado el workspace **CheckInApi** con las carpetas *Pasajeros*, *Pasajes* y
*Equipajes*, y el entorno **Local**.

## Entorno `Local`

| Variable | Valor por defecto | Uso |
| --- | --- | --- |
| `base_url` | `http://localhost:5129` | Host de la API (perfil `http` de `launchSettings.json`) |
| `pasajero_id` | `1` | ID del pasajero en las rutas |
| `documento` | `12345678` | DNI usado en los cuerpos de alta/actualización |
| `codigo_etiqueta` | `EQ-001` | Código del equipaje en las rutas de Equipajes |

Las URLs usan templates de Yaak: `${[ base_url ]}/api/Pasajero/${[ pasajero_id ]}`.

> `pasajero_id` es el **ID del pasajero**, no el `idPasaje`. En el conjunto de ejemplo,
> el pasajero `1` (Ada Lovelace) ya tiene pasaje, así que sirve para probar Equipajes
> sin crear nada antes.

## Requests incluidos

### Pasajeros (`/api/Pasajero`)

| Request | Método y ruta |
| --- | --- |
| Listar pasajeros | `GET /api/Pasajero` |
| Obtener pasajero por ID | `GET /api/Pasajero/{id}` |
| Crear pasajero | `POST /api/Pasajero` |
| Actualizar pasajero | `PUT /api/Pasajero/{id}` |
| Eliminar pasajero | `DELETE /api/Pasajero/{id}` |

Los `GET` devuelven el pasajero con su `pasaje` y sus `equipajes`.

### Pasajes (`/api/Pasajes`)

| Request | Método y ruta |
| --- | --- |
| Listar pasajes | `GET /api/Pasajes` |
| Obtener pasaje del pasajero | `GET /api/Pasajes/{idPasajero}` |
| Asignar pasaje | `POST /api/Pasajes/{idPasajero}` |
| Actualizar pasaje | `PUT /api/Pasajes/{idPasajero}` |
| Eliminar pasaje | `DELETE /api/Pasajes/{idPasajero}` |

`tipoVuelo` acepta `N` o `I`; `tipoTarifa` acepta `P`, `B`, `PE` o `F`.

Al asignar un pasaje, el servidor toma el primer asiento libre del vuelo y lo devuelve en
`asiento` (por ejemplo `1A`). Al actualizar, el asiento se conserva.

**Eliminar pasaje** responde `200` con `{ "mensaje": "Pasaje eliminado correctamente." }`,
`404` si el pasajero no existe o no tiene pasaje, y `400` si falla la escritura del archivo.

### Equipajes (`/api/Equipaje`)

| Request | Método y ruta |
| --- | --- |
| Listar equipajes | `GET /api/Equipaje` |
| Listar equipajes del pasajero | `GET /api/Equipaje/{idPasajero}` |
| Obtener equipaje por código | `GET /api/Equipaje/{idPasajero}/{codigoEtiqueta}` |
| Registrar equipaje | `POST /api/Equipaje/{idPasajero}` |
| Actualizar equipaje | `PUT /api/Equipaje/{idPasajero}/{codigoEtiqueta}` |
| Eliminar equipaje | `DELETE /api/Equipaje/{idPasajero}/{codigoEtiqueta}` |

- El pasajero debe tener un pasaje antes de registrar equipajes.
- `codigoEtiqueta` es obligatorio, único por pasajero y no se modifica en el `PUT`.
- Peso permitido: `15 kg` en vuelos nacionales (`N`) y `23 kg` en internacionales (`I`).
  Cada kilo excedido cuesta `$2.500`; `tarifaExceso` siempre la calcula el servidor.
- Los equipajes se guardan dentro de la línea del pasajero en `Data/pasajeros.txt`, así que
  sobreviven al reinicio de la API y se eliminan junto con el pasajero.
- Respuestas: `201` al registrar, `200` con el recurso o con `{ "mensaje": ... }`, `404`
  si el pasajero o el equipaje no existen, y `400` si los datos son inválidos.

## Datos de ejemplo

`DatosDeEjemplo/pasajeros_demo.txt` es el **backup** del conjunto de casos de uso. Tiene
12 pasajeros: 9 con pasaje, 3 sin pasaje y 12 equipajes, cubriendo vuelos nacionales e
internacionales, las cuatro tarifas y equipajes con y sin exceso.

Para restaurarlo antes de una demo (con la API detenida):

```powershell
Copy-Item DatosDeEjemplo\pasajeros_demo.txt Data\pasajeros.txt -Force
```

El detalle de cada caso está en `DatosDeEjemplo/README.md`.

## Orden sugerido para la demo

1. **Listar pasajeros**: se ven los pasajes y los equipajes ya cargados.
2. **Crear pasajero** (el ID asignado aparece en la respuesta).
3. Copiar ese ID en la variable `pasajero_id` del entorno **Local**.
4. **Asignar pasaje** y luego **Obtener pasaje del pasajero** (mirar el `asiento`).
5. **Registrar equipaje** y ver la `tarifaExceso` calculada.
6. Copiar el código en `codigo_etiqueta` y probar **Obtener**, **Actualizar** y
   **Eliminar equipaje**.
7. Probar **Actualizar pasajero** / **Actualizar pasaje** y verificar que el pasaje y los
   equipajes se conservan.

   > Si actualizás el pasajero recién creado, cambiá antes `documento` en el entorno por su
   > DNI. Si dejás `12345678` (el de Ada), la API responde `400` por documento duplicado.

8. Terminar con **Eliminar pasaje** y **Eliminar pasajero**.

> Nota: si la API corre con el perfil `https`, la URL es `https://localhost:7164`.
> En ese caso cambiá `base_url` en el entorno.
