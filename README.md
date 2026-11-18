# CheckInApi

API REST para el check-in de una aerolínea, desarrollada para el proyecto de Seminario de Lenguajes II. Administra pasajeros, pasajes y equipajes, y persiste todo en un único archivo de texto.

## Requisitos

- .NET SDK 10

## Ejecutar

Desde esta carpeta:

```bash
dotnet run --launch-profile http
```

La API queda disponible en `http://localhost:5129`. El menú con los módulos y las rutas se obtiene con `GET /api`. En desarrollo también se publica el documento OpenAPI en `http://localhost:5129/openapi/v1.json`.

## Arquitectura

El flujo de una petición es siempre el mismo:

```text
Cliente HTTP (.http / Yaak)
        ↓
Controller  → recibe la petición y traduce el resultado a un código HTTP
        ↓
Service     → aplica las reglas de negocio
        ↓
Repository  → lee y escribe el archivo
        ↓
Data/pasajeros.txt
```

Los controladores no acceden al archivo ni validan reglas de negocio. Las dependencias se registran en `Program.cs` y se inyectan por constructor.

## Endpoints

### Menú

| Método | Ruta | Descripción |
| --- | --- | --- |
| `GET` | `/api` | Devuelve los módulos, rutas y operaciones disponibles. |
| `GET` | `/` | Redirige a `/api`. |

### Pasajeros

| Método | Ruta | Descripción |
| --- | --- | --- |
| `GET` | `/api/pasajeros` | Lista todos los pasajeros con su pasaje y sus equipajes. |
| `GET` | `/api/pasajeros/{id}` | Busca un pasajero por ID. Devuelve `404` si no existe. |
| `GET` | `/api/pasajeros/documento/{documento}` | Busca un pasajero por documento. Devuelve `404` si no existe. |
| `POST` | `/api/pasajeros` | Crea un pasajero. Devuelve `201` si se crea y `400` si los datos no son válidos o el DNI ya existe. |
| `PUT` | `/api/pasajeros/{id}` | Actualiza un pasajero. Devuelve `200`, `400` o `404`. |
| `DELETE` | `/api/pasajeros/{id}` | Elimina un pasajero. Devuelve `200`, `400` o `404`. |

El cuerpo de `POST` y `PUT` es:

```json
{
  "nombre": "Ada",
  "apellido": "Lovelace",
  "documento": "12345678",
  "email": "ada@mail.com",
  "telefono": "+54 11 5555-1234",
  "pasajeroFrecuente": "1234567890"
}
```

Reglas: el DNI debe tener 7 u 8 dígitos (se admiten puntos), el email es obligatorio y el teléfono es opcional. El nombre no puede estar vacío. Al actualizar se conservan el pasaje y los equipajes del pasajero.

### Pasajes

Cada pasaje se administra dentro de su pasajero por `idPasajero`.

| Método | Ruta | Descripción |
| --- | --- | --- |
| `GET` | `/api/pasajes` | Lista los pasajes asignados. |
| `GET` | `/api/pasajes/{idPasajero}` | Obtiene el pasaje del pasajero. Devuelve `404` si no existe. |
| `POST` | `/api/pasajes/{idPasajero}` | Asigna un pasaje. Devuelve `201`, `400` o `404`. |
| `PUT` | `/api/pasajes/{idPasajero}` | Actualiza el pasaje. Devuelve `200`, `400` o `404`. |
| `DELETE` | `/api/pasajes/{idPasajero}` | Quita el pasaje. Devuelve `200`, `400` o `404`. |

El cuerpo de `POST` y `PUT` es:

```json
{
  "tipoVuelo": "N",
  "tipoTarifa": "P",
  "nroVuelo": "AR1234"
}
```

`tipoVuelo` admite `N` o `I`; `tipoTarifa` admite `P`, `B`, `PE` o `F`. Los precios se calculan automáticamente en pesos argentinos (ARS):

| Concepto | Importe |
| --- | ---: |
| Precio base nacional (`N`) | $200.000 |
| Precio base internacional (`I`) | $500.000 |
| Adicional Economy (`F`) | $250.000 |
| Adicional Premium Economy (`PE`) | $300.000 |
| Adicional Business (`B`) | $350.000 |
| Adicional Primera/Premium (`P`) | $400.000 |

`precioFinal` es la suma del precio base del vuelo más el adicional de la tarifa. Al asignar un pasaje, el servidor ocupa el primer asiento libre del vuelo (por ejemplo `1A`) y lo conserva en las actualizaciones.

### Equipajes

| Método | Ruta | Descripción |
| --- | --- | --- |
| `GET` | `/api/equipajes` | Lista todos los equipajes. |
| `GET` | `/api/equipajes/{idPasajero}` | Lista los equipajes del pasajero. Devuelve `404` si el pasajero no existe. |
| `GET` | `/api/equipajes/{idPasajero}/{codigoEtiqueta}` | Obtiene un equipaje por código. Devuelve `404` si no existe. |
| `POST` | `/api/equipajes/{idPasajero}` | Registra un equipaje. Devuelve `201`, `400` o `404`. |
| `PUT` | `/api/equipajes/{idPasajero}/{codigoEtiqueta}` | Actualiza un equipaje. Devuelve `200`, `400` o `404`. |
| `DELETE` | `/api/equipajes/{idPasajero}/{codigoEtiqueta}` | Elimina un equipaje. Devuelve `200`, `400` o `404`. |

El cuerpo de `POST` y `PUT` es:

```json
{
  "codigoEtiqueta": "EQ-001",
  "tipoEquipaje": "Bodega",
  "pesoKg": 20
}
```

- El pasajero debe existir y tener un pasaje asignado antes de registrar equipajes.
- `codigoEtiqueta` es obligatorio, único por pasajero y no se modifica en el `PUT`.
- El peso debe ser mayor que cero.
- El límite es de `15 kg` en vuelos nacionales (`N`) y `23 kg` en internacionales (`I`). Cada kilo excedido cuesta `$2.500`; `tarifaExceso` siempre la calcula el servidor.

## Errores

Los errores se responden en JSON con un mensaje:

```json
{ "mensaje": "No se encontró un pasajero con ID 99." }
```

| Código | Cuándo |
| --- | --- |
| `400` | Datos inválidos, regla de negocio incumplida o JSON mal formado. |
| `404` | El recurso o la ruta solicitada no existe. |
| `500` | Error de lectura o escritura del archivo, o error inesperado. |

Todas las operaciones devuelven un cuerpo con el recurso o con un mensaje de éxito, error o no encontrado.

## Probar la API desde VS Code

El archivo [CheckInApi.http](./CheckInApi.http) contiene ejemplos para todos los endpoints. Para ejecutarlos, instalá la extensión [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client), iniciá la API con el comando de **Ejecutar** y seleccioná **Send Request** sobre la petición que quieras probar.

## Probar la API con Yaak

La carpeta [yaak/](./yaak) contiene un workspace de [Yaak](https://yaak.app) con las requests de pasajeros, pasajes y equipajes.

1. Iniciá la API con el comando de **Ejecutar**.
2. En Yaak: menú de workspaces → **New Workspace** → **Open Folder** → elegí la carpeta `yaak`.
3. Seleccioná el entorno **Local** y ya podés enviar las requests.

La colección usa la variable `base_url` (`http://localhost:5129`) y `pasajero_id`, que se configuran en el entorno. Más detalles en [yaak/README.md](./yaak/README.md).

## Persistencia

El repositorio guarda todo en `Data/pasajeros.txt`, dentro del directorio del proyecto (`ContentRootPath`), para poder inspeccionarlo durante el desarrollo. El archivo usa una línea por pasajero y `|` como separador:

```text
Id|Nombre|Apellido|DNI|Frecuente|Email|Teléfono|IdPasaje|TipoVuelo|TipoTarifa|NroVuelo|PrecioBase|PrecioFinal|FechaEmision|Asiento|Equipajes
```

- Los campos 7 a 14 corresponden al pasaje. Si el pasajero no tiene pasaje, quedan vacíos.
- El campo 15 es opcional y guarda los equipajes como `codigo,tipo,peso,tarifa`, separados por `;`.
- Las escrituras usan un `lock` compartido y un archivo temporal (`pasajeros.txt.tmp`) que reemplaza al original mediante `File.Move`, para no corromper el archivo si falla la escritura.

## Datos de ejemplo

[DatosDeEjemplo/pasajeros_demo.txt](./DatosDeEjemplo/pasajeros_demo.txt) es el backup del conjunto de casos de uso: 12 pasajeros, 9 con pasaje y 12 equipajes. Para restaurarlo, con la API detenida:

```powershell
Copy-Item DatosDeEjemplo\pasajeros_demo.txt Data\pasajeros.txt -Force
```

El detalle de cada caso está en [DatosDeEjemplo/README.md](./DatosDeEjemplo/README.md).
