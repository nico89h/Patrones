# Simulador de Mundial + Prode (C# / .NET 8)

Consola interactiva que simula una Copa del Mundo (16 selecciones, 4 grupos,
cuartos-semis-final) integrada con un juego de pronósticos ("Prode").

## Cómo correrlo

Necesitás el SDK de .NET 8 instalado.

```bash
cd MundialProde
dotnet run
```

Al iniciar te pide el nombre de la Copa, importa automáticamente
`Data/selecciones.json` (16 selecciones en 4 grupos) y arma el árbol del torneo.

## Menú

1. Ver estructura del torneo (bracket)
2. Gestionar usuarios (crear / generar bots / listar). Al listar, se puede
   tipear el número de un usuario ahí mismo para ver su historial de
   predicciones (acertadas, falladas y pendientes)
3. Cargar predicciones (resultado de partido o campeón)
4. Elegir estrategia de simulación — se elige **una vez** y queda fija hasta
   que la cambiés de nuevo; no se vuelve a preguntar cada vez que simulás
5. Simular partidos:
   - **Una etapa específica**: lista, numeradas, solo las etapas que
     realmente tienen partidos cargados (nunca vas a ver "Fase Eliminatoria"
     en la lista si todavía no se generó ningún cruce)
   - **Un partido puntual**
   - **Todo el torneo** (lo que esté pendiente, de punta a punta)
6. Ver ranking. Igual que en "Gestionar usuarios", se puede tipear el número
   (la posición en la tabla) para ver el historial de ese usuario

Todos los submenús y selecciones (usuario, partido, etapa, estrategia,
opciones de simulación) tienen una opción **"0. Volver" / "0. Cancelar"**
explícita; también se puede cancelar dejando el campo vacío (enter).

No hay ninguna opción para "generar la fase eliminatoria a mano": se genera
sola. Ver más abajo.

## Generación automática de la fase eliminatoria

Cuando termina el último partido de la fase de grupos, se generan solos los
Cuartos de Final. Cuando termina el último partido de Cuartos, se genera sola
la Semifinal. Cuando termina la Semifinal, se genera sola la Final. Todo esto
pasa en el momento, sin que el usuario tenga que pedir nada por menú.

Esto **no** está resuelto con un observer ni con una clase aparte: es una
función privada de `Program` (`RevisarAvanceDeFase`, junto con sus funciones
privadas auxiliares `ArmarCuartos` / `ArmarSiguienteRonda`). `Partido` tiene
un delegado estático (`Partido.AlFinalizarPartido`) que `Program` engancha una
sola vez al arrancar; cada vez que un partido termina de simularse, se invoca
ese delegado directamente — sin interfaces, sin lista de suscriptores. El
`Ranking`, en cambio, sí sigue siendo un observer real (`IObservadorPartido`),
ya que necesita enterarse de cada partido para actualizar puntajes.

Como consecuencia, si elegís **"Todo el torneo"** o simulás la etapa
**"Fase Eliminatoria"** una vez que ya tiene Cuartos generados, todo el
recorrido (Cuartos → Semifinal → Final) se simula **en cascada, en una sola
llamada**: `Etapa.Simular()` recorre sus partidos por índice (no con
`foreach`), así que si `RevisarAvanceDeFase()` le agrega una ronda nueva a la
misma lista mientras la está recorriendo, el propio bucle la detecta y la
sigue simulando también, sin romper por "colección modificada durante la
enumeración" (un bug real que tenía la versión anterior).

## El Composite, recursivo

`CopaMundo` tiene un único `ComponenteTorneo` raíz (`copa.Raiz`, una
`Etapa`), y todo el torneo es un árbol que cuelga de ahí:

```
Raiz ("Copa Mundial 2026")
 ├── "Fase de Grupos"
 │     ├── "Grupo A" -> Partido, Partido, ...
 │     ├── "Grupo B" -> ...
 │     ├── "Grupo C" -> ...
 │     └── "Grupo D" -> ...
 └── "Fase Eliminatoria"          (la va completando el GestorFases solo)
       ├── "Cuartos de Final" -> Partido x4
       ├── "Semifinal"        -> Partido x2
       └── "Final"            -> Partido x1
```

`Mostrar()`, `Simular()`, `ObtenerPartidos()` y `EstaFinalizado()` funcionan
igual sin importar el nivel del árbol: sobre toda la Copa
(`copa.Raiz.Simular(...)`), sobre "Fase de Grupos", sobre "Grupo A" o sobre
un `Partido` suelto. `Buscar(nombre)` es un método recursivo del propio
Composite (lo implementan tanto `Partido` como `Etapa`) que ubica cualquier
nodo del árbol por nombre sin que quien llama sepa a qué profundidad está.

## Mapeo de los 5 patrones

| Patrón | Dónde está | Rol |
|---|---|---|
| **Strategy** | `Strategy/IEstrategiaSimulacion` + `EstrategiaRankingFifa`, `EstrategiaHistorica`, `EstrategiaEstadistica` | Algoritmos de simulación intercambiables. Se elige una vez desde el menú (`estrategiaActual`) y se reutiliza en todas las simulaciones |
| **Observer** | `Observer/IPartidoNotificador` (subject), `Observer/IObservadorPartido` (observador). `Partido` implementa el subject. Hay **dos** observadores concretos: `Servicios/Ranking` (actualiza puntajes) y `Servicios/GestorFases` (genera la siguiente fase automáticamente) | Al finalizar un partido, ambos se enteran sin que el simulador sepa nada de rankings ni de brackets |
| **Factory Method** | `Factory/PrediccionFactory` (abstracta) + `PrediccionResultadoFactory`, `PrediccionCampeonFactory` | Delegan a subclases la instanciación del tipo concreto de `Prediccion` |
| **Composite** | `Models/ComponenteTorneo` (abstracto) + `Models/Partido` (hoja) + `Models/Etapa` (compuesto), con `CopaMundo.Raiz` como única raíz recursiva | Trata de manera uniforme un partido individual y una fase completa (o el torneo entero), en cualquier nivel de profundidad |
| **State** | `Predicciones/Estados/IEstadoPrediccion` + `EstadoPendiente`, `EstadoAcertado`, `EstadoFallado`; contexto: `Predicciones/Prediccion` | El comportamiento de una predicción (si se puede editar, cuántos puntos otorga) cambia según su estado actual |

## Puntaje de las predicciones de resultado

- **3 puntos**: acertó el marcador exacto.
- **1 punto**: acertó quién gana (o el empate), pero no el marcador exacto.
- **0 puntos**: no acertó ni el ganador ni el resultado.

Esto vive en `PrediccionResultado.PuntosBase`, que el estado `EstadoAcertado`
consulta al calcular los puntos — el patrón State no cambia, solo varía
cuántos puntos otorga el estado "Acertado" según el nivel de acierto.

## Nota sobre el Ranking (Observer)

El `Ranking` **no reordena una lista guardada** cada vez que termina un
partido. Solo actualiza los puntajes de los usuarios afectados
(`ActualizarPuntaje`). El orden es una vista derivada que se calcula recién
cuando se pide mostrar el ranking (`ObtenerRankingOrdenado`, con un simple
`OrderByDescending`).

## Simplificaciones a propósito

- No hay predicción de goleador (se descartó, como se pidió).
- El "historial de enfrentamientos" y las "estadísticas de rendimiento" son
  variaciones del mismo modelo de Poisson con distintos factores (no hay una
  base de datos histórica real detrás): están pensadas para que el Strategy
  sea evidente e intercambiable, no para ser estadísticamente precisas.
- Definición por penales en fase eliminatoria: si un partido de knockout
  termina empatado, se define al azar (simulación simplificada).
