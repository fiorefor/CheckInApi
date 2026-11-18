# CheckInApi

API REST para el check-in de una aerolínea, como parte del proyecto #1 de teoría de SL2. Permite administrar pasajeros, el pasaje de cada pasajero y sus equipajes. Los datos se guardan en un archivo de texto.

## Requisitos

- .NET SDK 10

## Ejecutar

Desde esta carpeta:

```bash
dotnet run --launch-profile http
```

La API queda disponible en `http://localhost:5129`. La raíz (`/`) redirige a `/api`, que devuelve un menú con todos los módulos y sus operaciones. En el entorno de desarrollo también se publica el documento OpenAPI en `http://localhost:5129/openapi/v1.json`.

## Endpoints de pasajeros

La ruta base es `/api/pasajeros`. Cada pasajero se identifica por un `id` numérico que asigna el servidor al crearlo.

| Método | Ruta | Descripción |
| --- | --- | --- |
| `GET` | `/api/pasajeros` | Obtiene todos los pasajeros. |
| `GET` | `/api/pasajeros/{id}` | Busca un pasajero por ID. Devuelve `404` si no existe. |
| `GET` | `/api/pasajeros/documento/{documento}` | Busca un pasajero por DNI. Devuelve `404` si no existe. |
| `POST` | `/api/pasajeros` | Agrega un pasajero. Devuelve `201` si se crea y `400` si el DNI ya está registrado o los datos no son válidos. |
| `PUT` | `/api/pasajeros/{id}` | Actualiza un pasajero. Conserva su pasaje y sus equipajes. Devuelve `404` si no existe y `400` si los datos no son válidos. |
| `DELETE` | `/api/pasajeros/{id}` | Elimina un pasajero. Devuelve `404` si no existe. |

El cuerpo de `POST` y `PUT` es:

```json
{
  "nombre": "Ada",
  "apellido": "Lovelace",
  "documento": "12345678",
  "email": "ada@ejemplo.com",
  "telefono": "11 4444-5555",
  "pasajeroFrecuente": "true"
}
```

Validaciones:

- `nombre` y `apellido` no pueden estar vacíos.
- `documento` debe tener 7 u 8 dígitos. Se admiten puntos, espacios y guiones (`12.345.678`), que se quitan al guardar. No puede repetirse entre pasajeros.
- `email` es obligatorio y debe tener el formato `usuario@dominio.com`.
- `telefono` es opcional. Si se envía, solo puede contener números, espacios, guiones, paréntesis y `+`, con 6 a 15 dígitos.

## Endpoints de pasajes

Cada pasajero puede tener un pasaje, que se administra con el `idPasajero`.

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

`tipoVuelo` admite `N` (nacional) o `I` (internacional); `tipoTarifa` admite `P`, `B`, `PE` o `F`.

El servidor asigna automáticamente el asiento: toma el primer asiento libre del vuelo (`1A`, `1B`, … hasta `30F`, 180 asientos por vuelo). Si el vuelo está completo, devuelve `400`.

Los precios se calculan automáticamente en pesos argentinos (ARS):

| Concepto | Importe |
| --- | ---: |
| Precio base nacional (`N`) | $200.000 |
| Precio base internacional (`I`) | $500.000 |
| Adicional Economy (`F`) | $250.000 |
| Adicional Premium Economy (`PE`) | $300.000 |
| Adicional Business (`B`) | $350.000 |
| Adicional Primera/Premium (`P`) | $400.000 |

`precioFinal` es la suma del precio base del vuelo más el adicional de la tarifa. Por ejemplo, vuelo nacional Economy: `precioBase` $200.000 y `precioFinal` $450.000. El cliente no debe enviar los importes, el asiento ni la fecha de emisión en el JSON; el servidor los determina.

## Endpoints de equipaje

Cada pasajero puede registrar varios equipajes, identificados por su código de etiqueta. Para registrar un equipaje, el pasajero debe tener un pasaje asignado.

| Método | Ruta | Descripción |
| --- | --- | --- |
| `GET` | `/api/equipajes` | Lista todos los equipajes. |
| `GET` | `/api/equipajes/{idPasajero}` | Lista los equipajes de un pasajero. Devuelve `404` si el pasajero no existe. |
| `GET` | `/api/equipajes/{idPasajero}/{codigoEtiqueta}` | Obtiene un equipaje. Devuelve `404` si no existe. |
| `POST` | `/api/equipajes/{idPasajero}` | Registra un equipaje. Devuelve `201` si se crea y `400` si el pasajero no existe, no tiene pasaje, el código ya está usado o los datos no son válidos. |
| `PUT` | `/api/equipajes/{idPasajero}/{codigoEtiqueta}` | Actualiza el tipo y el peso. Devuelve `404` si no existe el pasajero, el pasaje o el equipaje, y `400` si el peso no es válido. |
| `DELETE` | `/api/equipajes/{idPasajero}/{codigoEtiqueta}` | Elimina un equipaje. Devuelve `404` si no existe. |

El cuerpo de `POST` es:

```json
{
  "codigoEtiqueta": "EQ-001",
  "tipoEquipaje": "Bodega",
  "pesoKg": 20
}
```

En `PUT` el código de etiqueta va en la ruta, así que solo se usan `tipoEquipaje` y `pesoKg`. El código de etiqueta es obligatorio, no distingue mayúsculas y no puede repetirse para el mismo pasajero. El peso debe ser mayor a 0 kg.

`tarifaExceso` la calcula el servidor según el tipo de vuelo del pasaje:

| Tipo de vuelo | Peso permitido |
| --- | ---: |
| Nacional (`N`) | 15 kg |
| Internacional (`I`) | 23 kg |

Cada kilo por encima del límite cuesta $2.500. Por ejemplo, 20 kg en un vuelo nacional: 5 kg de exceso, `tarifaExceso` $12.500.

## Errores

Los endpoints y los errores no controlados responden con este formato JSON:

```json
{ "mensaje": "Descripción del error." }
```

Los datos inválidos y los JSON mal formados devuelven `400`. Los problemas de lectura o escritura del archivo de datos devuelven `500`.

## Probar la API desde VS Code

El archivo [CheckInApi.http](./CheckInApi.http) contiene ejemplos para probar los endpoints. Para ejecutarlos desde VS Code, instala la extensión [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client), inicia la API con el comando indicado en **Ejecutar** y selecciona **Send Request** sobre la petición que quieras probar. No es necesario usar `curl`.

## Probar la API con Yaak

La carpeta [yaak/](./yaak) contiene un workspace de [Yaak](https://yaak.app) con todas las requests de pasajeros, pasajes y equipajes, listo para importar.

1. Iniciá la API con el comando de **Ejecutar**.
2. En Yaak: menú de workspaces → **New Workspace** → **Open Folder** → elegí la carpeta `yaak`.
3. Seleccioná el entorno **Local** y ya podés enviar las requests.

La colección usa la variable `base_url` (`http://localhost:5129`) y `pasajero_id`, que se configuran en el entorno. Más detalles en [yaak/README.md](./yaak/README.md).

## Persistencia

Los datos se guardan en [Data/pasajeros.txt](./Data/pasajeros.txt), dentro de la carpeta del proyecto. Si el archivo o la carpeta no existen, se crean al guardar el primer pasajero.

El archivo usa una línea por pasajero y `|` como separador. Cada línea incluye los datos del pasajero, su pasaje (si tiene) y sus equipajes:

```text
Id|Nombre|Apellido|DNI|PasajeroFrecuente|Email|Teléfono|IdPasaje|TipoVuelo|TipoTarifa|NroVuelo|PrecioBase|PrecioFinal|FechaEmision|Asiento|Equipajes
```

- Si el pasajero no tiene pasaje, los campos del pasaje quedan vacíos.
- Los equipajes se guardan como `codigo,tipo,peso,tarifa` y se separan con `;`.
- Al modificar datos, el archivo se escribe primero en un temporal y después reemplaza al original, para no perder datos si falla la escritura.
- Si una línea está dañada, la API devuelve un error en lugar de ignorarla, para no perder ese pasajero en la próxima escritura.

La carpeta [DatosDeEjemplo/](./DatosDeEjemplo) tiene un conjunto de 12 pasajeros de prueba y explica cómo restaurarlo.

## Arquitectura

- **Controllers:** reciben las requests y traducen los resultados a códigos HTTP.
- **Services:** `PasajeroService`, `PasajeService` y `EquipajeService` contienen las validaciones y reglas de negocio.
- **Repositorios:** `IPasajeroRepository` abstrae el almacenamiento. Hoy se usa `PasajeroRepositoryArchivo`; se puede reemplazar por una implementación con base de datos sin tocar los servicios.
- **Manejo de errores:** un manejador central en `Program.cs` convierte las excepciones en respuestas con el formato de **Errores**.
