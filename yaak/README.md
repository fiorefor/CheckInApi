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

Queda creado el workspace **CheckInApi** con las carpetas *Pasajeros* y *Pasajes* y el
entorno **Local**.

## Entorno `Local`

| Variable | Valor por defecto | Uso |
| --- | --- | --- |
| `base_url` | `http://localhost:5129` | Host de la API (perfil `http` de `launchSettings.json`) |
| `pasajero_id` | `1` | ID del pasajero en las rutas |
| `documento` | `12345678` | DNI usado en los cuerpos de alta/actualización |

Las URLs usan templates de Yaak: `${[ base_url ]}/api/Pasajero/${[ pasajero_id ]}`.

## Requests incluidos

### Pasajeros (`/api/Pasajero`)

| Request | Método y ruta |
| --- | --- |
| Listar pasajeros | `GET /api/Pasajero` |
| Obtener pasajero por ID | `GET /api/Pasajero/{id}` |
| Crear pasajero | `POST /api/Pasajero` |
| Actualizar pasajero | `PUT /api/Pasajero/{id}` |
| Eliminar pasajero | `DELETE /api/Pasajero/{id}` |

### Pasajes (`/api/Pasajes`)

| Request | Método y ruta |
| --- | --- |
| Listar pasajes | `GET /api/Pasajes` |
| Obtener pasaje del pasajero | `GET /api/Pasajes/{idPasajero}` |
| Asignar pasaje | `POST /api/Pasajes/{idPasajero}` |
| Actualizar pasaje | `PUT /api/Pasajes/{idPasajero}` |
| Eliminar pasaje | `DELETE /api/Pasajes/{idPasajero}` |

`tipoVuelo` acepta `N` o `I`; `tipoTarifa` acepta `P`, `B`, `PE` o `F`.

## Orden sugerido para la demo

1. **Crear pasajero** (el ID asignado aparece en la respuesta).
2. Copiar ese ID en la variable `pasajero_id` del entorno **Local**.
3. **Asignar pasaje** y luego **Obtener pasaje del pasajero**.
4. Probar **Actualizar pasajero** / **Actualizar pasaje** y verificar que el pasaje se conserva.
5. Terminar con **Eliminar pasaje** y **Eliminar pasajero**.

> Nota: si la API corre con el perfil `https`, la URL es `https://localhost:7164`.
> En ese caso cambiá `base_url` en el entorno.
