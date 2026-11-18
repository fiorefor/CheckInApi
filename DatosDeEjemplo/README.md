# Datos de ejemplo

`pasajeros_demo.txt` es el conjunto de casos de uso que se usa como **backup** de
`Data/pasajeros.txt`. Ambos archivos tienen exactamente el mismo contenido.

Para restaurarlo antes de una prueba o exposición:

```powershell
Copy-Item DatosDeEjemplo\pasajeros_demo.txt Data\pasajeros.txt -Force
```

Detené la API antes de copiar: si está corriendo, puede reescribir el archivo con
su estado en memoria.

## Casos incluidos

El conjunto tiene **12 pasajeros**, **9 con pasaje**, **3 sin pasaje** y **12 equipajes**.

| ID | Pasajero | Caso | Vuelo / tarifa | Asiento | Equipajes |
| ---: | --- | --- | --- | --- | --- |
| 1 | Ada Lovelace | Nacional con exceso | `N` / `P` · AR1234 | 1A | `EQ-001` Bodega 20 kg → `$12.500` |
| 2 | Alan Turing | Internacional con dos equipajes | `I` / `B` · AR5678 | 1A | `EQ-002` 30 kg → `$17.500`; `EQ-003` 10 kg → `$0` |
| 3 | Grace Hopper | Sin pasaje | — | — | — |
| 4 | Linus Torvalds | Nacional sin equipajes | `N` / `F` · AR1234 | 1B | — |
| 5 | Margaret Hamilton | Internacional en el límite exacto | `I` / `P` · AR5678 | 1B | `EQ-004` 23 kg → `$0` |
| 6 | Dennis Ritchie | Sin pasaje | — | — | — |
| 7 | Barbara Liskov | Nacional con exceso alto | `N` / `PE` · AR1234 | 1C | `EQ-005` 50 kg → `$87.500` |
| 8 | Tim Berners-Lee | Internacional con dos equipajes | `I` / `F` · AR5678 | 1C | `EQ-006` 20 kg → `$0`; `EQ-007` 25 kg → `$5.000` |
| 9 | Katherine Johnson | Nacional en el límite exacto | `N` / `B` · AR1234 | 1D | `EQ-008` 15 kg → `$0` |
| 10 | Donald Knuth | Sin pasaje | — | — | — |
| 11 | Hedy Lamarr | Internacional con tres equipajes | `I` / `PE` · AR5678 | 1D | `EQ-009` 5 kg, `EQ-010` 10 kg, `EQ-011` 23 kg → `$0` |
| 12 | Ken Thompson | Tipo de equipaje con espacio escapado | `N` / `F` · AR1234 | 1E | `EQ-012` "Bodega especial" 23 kg → `$20.000` |

## Reglas que cubre

- Las cuatro tarifas: `P`, `B`, `PE` y `F`.
- Vuelos nacionales (límite `15 kg`) e internacionales (límite `23 kg`).
- Equipajes sin exceso, con exceso y justo en el límite.
- Pasajeros con un equipaje, con varios y sin equipajes.
- Pasajeros con pasaje y pasajeros sin pasaje.
- Asientos distintos dentro del mismo vuelo.
- Un tipo de equipaje con espacio, guardado como `Bodega%20especial` en el campo 15.

## Formato del campo 15

Cada equipaje se guarda como `codigo,tipo,peso,tarifa` y los equipajes se separan
con `;`. El código y el tipo se escapan con `Uri.EscapeDataString`.
