# Exposición del proyecto

Guion de la demo, en orden. Antes de empezar, iniciar la API:

```powershell
cd "C:\Users\Thiago\Documents\UNaM\2do Año\Seminario de Lenguajes II\CheckInApi"
dotnet run --launch-profile http
```

La API queda en `http://localhost:5129`.

---

## Datos de ejemplo

El repositorio incluye `DatosDeEjemplo/pasajeros_demo.txt`, un conjunto con 12
pasajeros que cubren pasajeros con y sin pasaje, vuelos nacionales e
internacionales, las cuatro tarifas, equipajes con y sin exceso y varios
equipajes por pasajero.

Para restaurarlo antes de la demo (con la API detenida):

```powershell
Copy-Item DatosDeEjemplo\pasajeros_demo.txt Data\pasajeros.txt -Force
```

El detalle de cada caso está en `DatosDeEjemplo/README.md`.

---

## 1. `GET /api`: mostrar módulos y rutas

Abrir en el cliente:

```http
GET http://localhost:5129/api
```

**Esperado:** `200` con el JSON del menú: `api`, `descripcion` y `modulos` de **Pasajeros**, **Pasajes** y **Equipajes** con sus operaciones.

**Qué explicar:** el menú se arma en `InicioController` y refleja únicamente los módulos que tienen endpoints.

---

## 2. `GET /`: mostrar la redirección

```http
GET http://localhost:5129/
```

**Esperado:** `302` con `Location: /api`.

**Qué explicar:** la redirección se configura en `Program.cs` con `app.MapGet("/", () => Results.Redirect("/api"))`.

---

## 3. `POST /api/Pasajero` con datos válidos

```http
POST http://localhost:5129/api/Pasajero
Content-Type: application/json

{
  "nombre": "Prueba",
  "apellido": "Integrante",
  "documento": "99999999",
  "email": "prueba@correo.com",
  "telefono": "3764000000"
}
```

**Esperado:** `200` con el texto `Pasajero agregado exitosamente con ID ...`.

**Qué explicar:** el controlador delega en `PasajeroService` y traduce la tupla `(true, mensaje)` a `200`.

---

## 4. `POST` repetido con el mismo documento

Enviar otra vez el mismo cuerpo.

**Esperado:** `400` con el texto que contiene `Ya existe un pasajero registrado con el DNI 99999999.`

**Qué explicar:** el servicio verifica duplicados antes de guardar y devuelve `(false, mensaje)`; el controlador responde `400`.

---

## 5. `GET /api/Pasajero/documento/{documento}`: recuperar el pasajero

```http
GET http://localhost:5129/api/Pasajero/documento/99999999
```

**Esperado:** `200` con el JSON del pasajero, incluido su `id`, su `pasaje` (si tiene) y sus `equipajes`.

**Qué explicar:** la ruta `documento/{documento}` no compite con `{id:int}`. El servicio normaliza el documento (quita puntos y espacios) antes de buscar.

Guardar el `id` devuelto para los pasos siguientes.

---

## 6. `POST /api/Pasajes/{id}`: asignar pasaje y consultarlo

```http
POST http://localhost:5129/api/Pasajes/{id}
Content-Type: application/json

{
  "tipoVuelo": "N",
  "tipoTarifa": "P",
  "nroVuelo": "AR1234"
}
```

**Esperado:** `201` con el pasaje creado, incluyendo `precioBase`, `precioFinal`, `fechaEmision` y `asiento`. El servidor asigna el primer asiento libre del vuelo (por ejemplo, `1A`).

Luego:

```http
GET http://localhost:5129/api/Pasajes/{id}
```

**Esperado:** `200` con el pasaje.

**Qué explicar:** el controlador de pasajes devuelve `201` al crear y `404` si el pasajero no existe. Los precios los calcula el dominio: nacional `$200.000` + primera `$400.000` = `$600.000`.

---

## 7. `POST /api/Equipaje/{id}`: registrar equipajes

```http
POST http://localhost:5129/api/Equipaje/{id}
Content-Type: application/json

{
  "codigoEtiqueta": "EQ-001",
  "tipoEquipaje": "Bodega",
  "pesoKg": 20
}
```

**Esperado:** `201` con el equipaje y `"tarifaExceso": 12500.0`. El vuelo es nacional (límite `15 kg`), así que el exceso es de `5 kg` a `$2.500` por kilo.

Registrar un segundo equipaje:

```http
POST http://localhost:5129/api/Equipaje/{id}
Content-Type: application/json

{
  "codigoEtiqueta": "EQ-002",
  "tipoEquipaje": "Mano",
  "pesoKg": 10
}
```

**Esperado:** `201` con `"tarifaExceso": 0.0`.

Enviar `"pesoKg": 0`:

**Esperado:** `400` con `{ "mensaje": "El peso del equipaje debe ser mayor a 0 kg." }`.

Repetir `EQ-001`:

**Esperado:** `400` con `{ "mensaje": "Ya existe un equipaje con el código de etiqueta 'EQ-001' para este pasajero." }`.

**Qué explicar:** la ruta aporta el pasajero y el cuerpo los datos del equipaje. `EquipajeRequest` no incluye `tarifaExceso` porque la calcula el servidor. Si el pasajero no existe o no tiene pasaje, `EquipajeService` lanza `EquipajeException` y el manejador central responde `400` con `{ "mensaje": ... }`. El código de etiqueta identifica al equipaje dentro del pasajero.

**Requisito:** el pasajero debe tener un pasaje. Si usás el `id` del paso 6, ya lo tiene. Si no, ejecutá primero `POST /api/Pasajes/{id}` (Pasajes → Asignar pasaje); de lo contrario el `POST` de equipaje responde `400` con `... no tiene un pasaje asignado ...`.

---

## 8. `GET` de equipajes: listar y buscar

```http
GET http://localhost:5129/api/Equipaje
GET http://localhost:5129/api/Equipaje/{id}
GET http://localhost:5129/api/Equipaje/{id}/EQ-001
```

**Esperado:** `200` con la lista completa, la lista del pasajero y el equipaje pedido, respectivamente.

Probar una ruta inexistente:

```http
GET http://localhost:5129/api/Equipaje/{id}/NO-EXISTE
```

**Esperado:** `404` con `{ "mensaje": "No se encontró un equipaje con el código 'NO-EXISTE' ..." }`.

**Qué explicar:** `ListarPorPasajero` devuelve `null` si el pasajero no existe (el controlador responde `404`) y una lista vacía si existe pero no tiene equipajes (`200` con `[]`). La búsqueda por código no distingue mayúsculas.

---

## 9. `PUT /api/Equipaje/{id}/{codigo}`: actualizar peso y recalcular

```http
PUT http://localhost:5129/api/Equipaje/{id}/EQ-001
Content-Type: application/json

{
  "codigoEtiqueta": "EQ-001",
  "tipoEquipaje": "Bodega",
  "pesoKg": 30
}
```

**Esperado:** `200` con `{ "mensaje": "Equipaje actualizado correctamente." }`.

Consultar con `GET /api/Equipaje/{id}/EQ-001` y mostrar `"tarifaExceso": 37500.0`: `15 kg` de exceso a `$2.500`.

**Qué explicar:** el código de etiqueta viaja en la ruta y no se modifica; el cuerpo solo cambia peso y tipo. La tarifa se vuelve a calcular en el servidor, nunca se recibe del cliente.

---

## 10. Persistencia: actualizar el pasajero, reiniciar la API y volver a consultar

Actualizar los datos personales del pasajero:

```http
PUT http://localhost:5129/api/Pasajero/{id}
Content-Type: application/json

{
  "nombre": "Prueba",
  "apellido": "Integrante",
  "documento": "99999999",
  "email": "prueba@correo.com",
  "telefono": "3764111111"
}
```

**Esperado:** `200`. Luego `GET /api/Equipaje/{id}` debe seguir devolviendo `EQ-001` y `EQ-002`.

Detener la API con `Ctrl+C` y volver a iniciarla con `dotnet run --launch-profile http`. Consultar otra vez `GET /api/Equipaje/{id}`.

**Esperado:** los dos equipajes siguen ahí.

**Qué explicar:** los equipajes se guardan en el campo 15 de la línea del pasajero en `Data/pasajeros.txt`. `PasajeroService.Actualizar` copia los equipajes existentes para que una edición de datos personales no los borre. Al reiniciar, el repositorio vuelve a leer el archivo y reconstruye la lista. Abrir el archivo y mostrar el campo con el formato `codigo,tipo,peso,tarifa` separado por `;`.

---

## 11. `PUT /api/Pasajes/{id}`: pasar a internacional y comparar el límite

```http
PUT http://localhost:5129/api/Pasajes/{id}
Content-Type: application/json

{
  "tipoVuelo": "I",
  "tipoTarifa": "PE",
  "nroVuelo": "AR5678"
}
```

**Esperado:** `200` con `{ "mensaje": "Pasaje actualizado correctamente." }`.

Volver a consultar con `GET /api/Pasajes/{id}` y mostrar que cambiaron `tipoVuelo`, `tipoTarifa`, `precioBase` y `precioFinal`, y que se conservaron `fechaEmision` y `asiento`.

Ahora repetir `PUT /api/Equipaje/{id}/EQ-001` con `"pesoKg": 30`.

**Esperado:** `200`; al consultar, `"tarifaExceso": 17500.0`. El límite internacional es `23 kg`, así que el exceso baja a `7 kg`.

**Qué explicar:** la misma valija paga distinto según el tipo de vuelo del pasaje. La regla (`15 kg` nacional, `23 kg` internacional, `$2.500` por kilo excedido) vive en `EquipajeService`.

---

## 12. `DELETE /api/Equipaje/{id}/{codigo}`: eliminar un equipaje

```http
DELETE http://localhost:5129/api/Equipaje/{id}/EQ-002
```

**Esperado:** `200` con `{ "mensaje": "Equipaje eliminado correctamente." }`.

Repetir el mismo `DELETE`.

**Esperado:** `404` con `{ "mensaje": "No se encontró un equipaje con el código 'EQ-002' ..." }`.

**Qué explicar:** la baja modifica la lista del pasajero y reescribe su línea. El equipaje no se marca como borrado: desaparece del archivo.

---

## 13. Enviar `"tipoVuelo": "X"` y mostrar el `400` de negocio

```http
PUT http://localhost:5129/api/Pasajes/{id}
Content-Type: application/json

{
  "tipoVuelo": "X",
  "tipoTarifa": "P",
  "nroVuelo": "AR1234"
}
```

**Esperado:** `400` con `{ "mensaje": "El campo Tipo de Vuelo tiene un error: ..." }`.

**Qué explicar:** la validación vive en el dominio (`Pasaje`); el servicio la convierte en una tupla con mensaje y el controlador en `400`.

---

## 14. `DELETE /api/Pasajero/{id}` y luego `GET /api/Pasajero/{id}`

```http
DELETE http://localhost:5129/api/Pasajero/{id}
```

**Esperado:** `200` con `Pasajero eliminado exitosamente.`

Después:

```http
GET http://localhost:5129/api/Pasajero/{id}
```

**Esperado:** `404` con `No se encontró un pasajero con ID {id}.`

Consultar `GET /api/Equipaje/{id}`.

**Esperado:** `404`, porque el pasajero y sus equipajes ya no existen.

**Qué explicar:** el `DELETE` controla la existencia antes de llamar al servicio, por eso devuelve `404` y no `400`. La baja es en cascada: los equipajes estaban dentro de la línea del pasajero.

---

## 15. Enviar un JSON mal formado y mostrar el `400` uniforme

```http
POST http://localhost:5129/api/Pasajero
Content-Type: application/json

{"nombre":
```

**Esperado:** `400` con `{ "mensaje": "Los datos enviados no son válidos. Verifique el formato del JSON y los campos obligatorios." }`.

**Qué explicar:** `InvalidModelStateResponseFactory` en `Program.cs` unifica los errores de enlace. El manejador central (`UseExceptionHandler`) hace lo mismo con las excepciones no controladas, sin duplicar `try/catch` en los controladores.

---

## Cierre

Recorrido completo: cliente HTTP → controlador → servicio → repositorio → archivo de texto, con respuestas uniformes y errores centralizados. Los tres módulos tienen CRUD y persistencia: pasajeros, pasajes y equipajes. Los equipajes se guardan dentro de la línea de su pasajero, lo que mantiene la integridad y hace que la baja del pasajero los elimine junto con él.
