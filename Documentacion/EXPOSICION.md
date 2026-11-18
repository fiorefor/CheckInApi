# Exposición del proyecto

Guion de la demo, en orden. Antes de empezar, iniciar la API:

```powershell
cd "C:\Users\Thiago\Documents\UNaM\2do Año\Seminario de Lenguajes II\CheckInApi"
dotnet run --launch-profile http
```

La API queda en `http://localhost:5129`.

---

## 1. `GET /api`: mostrar módulos y rutas

Abrir en el cliente:

```http
GET http://localhost:5129/api
```

**Esperado:** `200` con el JSON del menú: `api`, `descripcion` y `modulos` de **Pasajeros** y **Pasajes** con sus operaciones.

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

**Esperado:** `200` con el JSON del pasajero, incluido su `id`.

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

**Esperado:** `201` con el pasaje creado, incluyendo `precioBase`, `precioFinal`, `fechaEmision` y `asiento`.

Luego:

```http
GET http://localhost:5129/api/Pasajes/{id}
```

**Esperado:** `200` con el pasaje.

**Qué explicar:** el controlador de pasajes devuelve `201` al crear y `404` si el pasajero no existe. Los precios los calcula el dominio: nacional `$200.000` + primera `$400.000` = `$600.000`.

---

## 7. `PUT /api/Pasajes/{id}`: actualizar tarifa o vuelo

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

---

## 8. Enviar `"tipoVuelo": "X"` y mostrar el `400` de negocio

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

## 9. `DELETE /api/Pasajero/{id}` y luego `GET /api/Pasajero/{id}`

```http
DELETE http://localhost:5129/api/Pasajero/{id}
```

**Esperado:** `200` con `Pasajero eliminado exitosamente.`

Después:

```http
GET http://localhost:5129/api/Pasajero/{id}
```

**Esperado:** `404` con `No se encontró un pasajero con ID {id}.`

**Qué explicar:** el `DELETE` controla la existencia antes de llamar al servicio, por eso devuelve `404` y no `400`.

---

## 10. Enviar un JSON mal formado y mostrar el `400` uniforme

```http
POST http://localhost:5129/api/Pasajero
Content-Type: application/json

{"nombre":
```

**Esperado:** `400` con `{ "mensaje": "Los datos enviados no son válidos. Verifique el formato del JSON y los campos obligatorios." }`.

**Qué explicar:** `InvalidModelStateResponseFactory` en `Program.cs` unifica los errores de enlace. El manejador central (`UseExceptionHandler`) hace lo mismo con las excepciones no controladas, sin duplicar `try/catch` en los controladores.

---

## Cierre

Recorrido completo: cliente HTTP → controlador → servicio → repositorio → archivo de texto, con respuestas uniformes y errores centralizados. Queda pendiente el módulo de equipajes (servicio, endpoints y persistencia).
