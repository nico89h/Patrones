using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using MundialProde.Models;
using MundialProde.Usuarios;
using MundialProde.Strategy;
using MundialProde.Predicciones;
using MundialProde.Factory;
using MundialProde.Servicios;

namespace MundialProde
{
    public class Program
    {
        private static CopaMundo copa;
        private static List<Usuario> usuarios;
        private static Ranking ranking;
        private static readonly Random rnd = new Random();
        private static readonly PrediccionResultadoFactory factoryResultado = new PrediccionResultadoFactory();
        private static readonly PrediccionCampeonFactory factoryCampeon = new PrediccionCampeonFactory();

        // La estrategia se elige una vez desde el menú y se reutiliza en cada simulación,
        // en vez de volver a preguntar cada vez que se quiere simular algo.
        private static IEstrategiaSimulacion estrategiaActual = new EstrategiaRankingFifa();

        public static void Main(string[] args)
        {
            try { Console.OutputEncoding = System.Text.Encoding.UTF8; } catch { /* algunas terminales no lo permiten */ }

            Console.WriteLine("==================================================");
            Console.WriteLine("   SIMULADOR DE MUNDIAL + PRODE DE PREDICCIONES   ");
            Console.WriteLine("==================================================");

            IniciarCopaMundo();

            bool salir = false;
            while (!salir)
            {
                MostrarMenu();
                string opcion = Console.ReadLine();
                Console.WriteLine();
                switch (opcion)
                {
                    case "1": copa.Mostrar(); break;
                    case "2": MenuUsuarios(); break;
                    case "3": MenuPredicciones(); break;
                    case "4": MenuElegirEstrategia(); break;
                    case "5": MenuSimulacion(); break;
                    case "6": VerRanking(); break;
                    case "0": salir = true; break;
                    default: Console.WriteLine("Opción inválida."); break;
                }
            }

            Console.WriteLine("\n¡Hasta la próxima!");
        }

        // ----------------------------------------------------------------
        // Inicialización — arma el árbol del Composite (una única raíz),
        // conecta el Ranking (Observer) a cada partido, y engancha
        // Partido.AlFinalizarPartido a RevisarAvanceDeFase (más abajo): una
        // función privada de esta misma clase, sin patrón ni clase aparte.
        // ----------------------------------------------------------------

        private static void IniciarCopaMundo()
        {
            Console.Write("\nNombre de la Copa del Mundo a crear (enter = 'Copa Mundial 2026'): ");
            string nombreCopa = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(nombreCopa)) nombreCopa = "Copa Mundial 2026";

            copa = new CopaMundo(nombreCopa);
            usuarios = new List<Usuario>();
            ranking = new Ranking(usuarios, copa);
            Partido.AlFinalizarPartido = RevisarAvanceDeFase;

            CargarSeleccionesDesdeJson("Data/selecciones.json");

            // Raiz -> "Fase de Grupos" -> "Grupo A".."Grupo D" -> Partidos
            var faseDeGrupos = new Etapa("Fase de Grupos");
            copa.Raiz.Agregar(faseDeGrupos);
            GenerarFaseDeGrupos(faseDeGrupos);

            // Raiz -> "Fase Eliminatoria" (arranca vacía; RevisarAvanceDeFase()
            // la va completando sola, a medida que la fase de grupos y cada ronda
            // van terminando — ver la llamada directa dentro de Partido.Simular)
            var faseEliminatoria = new Etapa("Fase Eliminatoria");
            copa.Raiz.Agregar(faseEliminatoria);

            int cantidadGrupos = faseDeGrupos.Hijos.Count;
            Console.WriteLine($"\n'{copa.Nombre}' creada con {copa.Selecciones.Count} selecciones en {cantidadGrupos} grupos.");
        }

        private static void CargarSeleccionesDesdeJson(string rutaRelativa)
        {
            string ruta = Path.Combine(AppContext.BaseDirectory, rutaRelativa);
            if (!File.Exists(ruta))
                ruta = rutaRelativa; // fallback por si se ejecuta con `dotnet run` desde la raíz del proyecto

            if (!File.Exists(ruta))
            {
                Console.WriteLine($"No se encontró el archivo de selecciones en '{ruta}'.");
                return;
            }

            string json = File.ReadAllText(ruta);
            var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var dtos = JsonSerializer.Deserialize<List<SeleccionDTO>>(json, opciones);

            foreach (var dto in dtos)
                copa.Selecciones.Add(new Seleccion(dto.Nombre, dto.RankingFifa, dto.Grupo));
        }

        private static void GenerarFaseDeGrupos(Etapa contenedor)
        {
            var grupos = copa.Selecciones.GroupBy(s => s.Grupo).OrderBy(g => g.Key);
            foreach (var grupo in grupos)
            {
                var etapaGrupo = new Etapa($"Grupo {grupo.Key}");
                var equipos = grupo.ToList();

                for (int i = 0; i < equipos.Count; i++)
                    for (int j = i + 1; j < equipos.Count; j++)
                        CrearYAgregarPartido(etapaGrupo, equipos[i], equipos[j], etapaGrupo.Nombre);

                contenedor.Agregar(etapaGrupo);
            }
        }

        private static void CrearYAgregarPartido(Etapa etapa, Seleccion local, Seleccion visitante, string nombreEtapa)
        {
            var partido = new Partido(local, visitante, nombreEtapa);
            partido.Suscribir(ranking);
            etapa.Agregar(partido);
        }

        // Suscribe el Ranking a todos los partidos de una etapa recién
        // generada automáticamente (ej: cuando se arma la Semifinal sola).
        private static void SuscribirTodosLosPartidos(Etapa etapa)
        {
            foreach (var partido in etapa.ObtenerPartidos())
                partido.Suscribir(ranking);
        }

        // ----------------------------------------------------------------
        // Avance de fase — pura lógica, sin clase ni patrón aparte. Se llama
        // directo desde Partido.Simular() (vía el delegado AlFinalizarPartido)
        // apenas termina cada partido. Si con ese resultado se completó la
        // etapa actual, arma automáticamente la siguiente:
        // Fase de Grupos completa -> Cuartos, Cuartos completos -> Semifinal,
        // Semifinal completa -> Final. El usuario nunca lo pide a mano.
        // ----------------------------------------------------------------

        private static void RevisarAvanceDeFase()
        {
            var faseDeGrupos = copa.ObtenerEtapa("Fase de Grupos");
            var faseEliminatoria = copa.ObtenerEtapa("Fase Eliminatoria");
            if (faseDeGrupos == null || faseEliminatoria == null) return;
            if (!faseDeGrupos.EstaFinalizado()) return;

            var cuartos = copa.ObtenerEtapa("Cuartos de Final");
            if (cuartos == null)
            {
                cuartos = ArmarCuartos(faseDeGrupos);
                faseEliminatoria.Agregar(cuartos);
                SuscribirTodosLosPartidos(cuartos);
                Console.WriteLine("\n>> Fase de grupos completa: se generaron automáticamente los Cuartos de Final. <<");
                return;
            }
            if (!cuartos.EstaFinalizado()) return;

            var semis = copa.ObtenerEtapa("Semifinal");
            if (semis == null)
            {
                semis = ArmarSiguienteRonda(cuartos, "Semifinal");
                faseEliminatoria.Agregar(semis);
                SuscribirTodosLosPartidos(semis);
                Console.WriteLine("\n>> Cuartos de Final completos: se generó automáticamente la Semifinal. <<");
                return;
            }
            if (!semis.EstaFinalizado()) return;

            var final = copa.ObtenerEtapa("Final");
            if (final == null)
            {
                final = ArmarSiguienteRonda(semis, "Final");
                faseEliminatoria.Agregar(final);
                SuscribirTodosLosPartidos(final);
                Console.WriteLine("\n>> Semifinal completa: se generó automáticamente la Final. <<");
            }
        }

        private static Etapa ArmarCuartos(Etapa faseDeGrupos)
        {
            var clasificados = new List<Seleccion>();
            var gruposOrdenados = faseDeGrupos.Hijos.OfType<Etapa>().OrderBy(e => e.Nombre);

            foreach (var grupo in gruposOrdenados)
            {
                var equiposDelGrupo = grupo.ObtenerPartidos()
                    .SelectMany(p => new[] { p.Local, p.Visitante })
                    .Distinct()
                    .OrderByDescending(s => s.PuntosGrupo)
                    .ThenByDescending(s => s.DiferenciaGoles)
                    .ThenByDescending(s => s.GolesFavor)
                    .Take(2)
                    .ToList();

                clasificados.AddRange(equiposDelGrupo);
            }

            // clasificados queda: [1A, 2A, 1B, 2B, 1C, 2C, 1D, 2D]
            var cuartos = new Etapa("Cuartos de Final");
            ArmarPartidoEliminatoria(cuartos, clasificados[0], clasificados[3], "Cuartos de Final"); // 1A vs 2B
            ArmarPartidoEliminatoria(cuartos, clasificados[2], clasificados[1], "Cuartos de Final"); // 1B vs 2A
            ArmarPartidoEliminatoria(cuartos, clasificados[4], clasificados[7], "Cuartos de Final"); // 1C vs 2D
            ArmarPartidoEliminatoria(cuartos, clasificados[6], clasificados[5], "Cuartos de Final"); // 1D vs 2C
            return cuartos;
        }

        private static Etapa ArmarSiguienteRonda(Etapa etapaAnterior, string nombreNuevaEtapa)
        {
            var partidosAnteriores = etapaAnterior.ObtenerPartidos();
            var nuevaEtapa = new Etapa(nombreNuevaEtapa);

            for (int i = 0; i < partidosAnteriores.Count; i += 2)
            {
                var ganador1 = partidosAnteriores[i].GanadorDefinitivo;
                var ganador2 = partidosAnteriores[i + 1].GanadorDefinitivo;
                ArmarPartidoEliminatoria(nuevaEtapa, ganador1, ganador2, nombreNuevaEtapa);
            }

            return nuevaEtapa;
        }

        // Crea el partido y lo agrega a la etapa, sin suscribirlo todavía
        // (la suscripción al Ranking se hace en bloque, una vez armada toda
        // la etapa, con SuscribirTodosLosPartidos).
        private static void ArmarPartidoEliminatoria(Etapa etapa, Seleccion local, Seleccion visitante, string nombreEtapa)
        {
            var partido = new Partido(local, visitante, nombreEtapa);
            etapa.Agregar(partido);
        }

        // ----------------------------------------------------------------
        // Menú
        // ----------------------------------------------------------------

        private static void MostrarMenu()
        {
            Console.WriteLine("\n----------------- MENÚ -----------------");
            Console.WriteLine("1. Ver estructura del torneo (bracket)");
            Console.WriteLine("2. Gestionar usuarios");
            Console.WriteLine("3. Cargar predicciones");
            Console.WriteLine($"4. Elegir estrategia de simulación (actual: {estrategiaActual.Nombre})");
            Console.WriteLine("5. Simular partidos");
            Console.WriteLine("6. Ver ranking");
            Console.WriteLine("0. Salir");
            Console.Write("Elegí una opción: ");
        }

        // ----------------------------------------------------------------
        // Usuarios
        // ----------------------------------------------------------------

        private static void MenuUsuarios()
        {
            Console.WriteLine("1. Crear usuario");
            Console.WriteLine("2. Generar usuarios aleatorios (bots)");
            Console.WriteLine("3. Listar usuarios");
            Console.WriteLine("0. Volver");
            Console.Write("Opción: ");
            string op = Console.ReadLine();
            Console.WriteLine();

            switch (op)
            {
                case "1":
                    Console.Write("Nombre del usuario (enter = cancelar): ");
                    string nombre = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(nombre)) { Console.WriteLine("Cancelado."); return; }
                    usuarios.Add(new Usuario(nombre));
                    Console.WriteLine($"Usuario '{nombre}' creado.");
                    break;

                case "2":
                    Console.Write("¿Cuántos bots generar? (enter = cancelar): ");
                    string entradaBots = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(entradaBots)) { Console.WriteLine("Cancelado."); return; }
                    if (int.TryParse(entradaBots, out int cant) && cant > 0)
                    {
                        var bots = GeneradorUsuarios.GenerarVarios(cant);
                        usuarios.AddRange(bots);
                        foreach (var bot in bots)
                            GenerarPrediccionesAutomaticas(bot);
                        Console.WriteLine($"Se generaron {cant} bots, cada uno con sus predicciones cargadas.");
                    }
                    else Console.WriteLine("Cantidad inválida.");
                    break;

                case "3":
                    ListarUsuariosConHistorial();
                    break;

                case "0":
                    break;

                default:
                    Console.WriteLine("Opción inválida.");
                    break;
            }
        }

        // Lista los usuarios numerados y, ahí mismo, deja tipear el número
        // de uno para ver su historial de predicciones (enter = no ver ninguno).
        private static void ListarUsuariosConHistorial()
        {
            if (usuarios.Count == 0) { Console.WriteLine("Todavía no hay usuarios."); return; }

            for (int i = 0; i < usuarios.Count; i++)
            {
                var u = usuarios[i];
                Console.WriteLine($"{i + 1}. {u.Nombre,-22} {(u.EsBot ? "(bot)" : "     ")} | {u.Puntaje} pts | {u.Predicciones.Count} predicciones");
            }

            Console.Write("\nNúmero de usuario para ver su historial (enter para omitir): ");
            string entrada = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(entrada)) return;

            if (int.TryParse(entrada, out int idx) && idx >= 1 && idx <= usuarios.Count)
                MostrarHistorialPredicciones(usuarios[idx - 1]);
            else
                Console.WriteLine("Usuario inválido.");
        }

        // Muestra la lista de usuarios y devuelve el elegido. Devuelve null
        // si todavía no hay usuarios, si el usuario cancela (0 / enter) o si
        // ingresa una opción inválida. La usa MenuPredicciones para saber
        // para quién se está cargando el pronóstico.
        private static Usuario SeleccionarUsuario()
        {
            if (usuarios.Count == 0) { Console.WriteLine("Todavía no hay usuarios."); return null; }

            Console.WriteLine("Usuarios disponibles:");
            for (int i = 0; i < usuarios.Count; i++)
                Console.WriteLine($"{i + 1}. {usuarios[i].Nombre}");
            Console.WriteLine("0. Volver");
            Console.Write("Elegí un usuario: ");
            string entrada = Console.ReadLine();

            if (entrada == "0" || string.IsNullOrWhiteSpace(entrada)) return null;
            if (!int.TryParse(entrada, out int idx) || idx < 1 || idx > usuarios.Count)
            {
                Console.WriteLine("Usuario inválido.");
                return null;
            }
            return usuarios[idx - 1];
        }

        // Historial completo de predicciones de un usuario: qué predijo,
        // en qué estado quedó (Pendiente/Acertado/Fallado) y cuántos puntos
        // le dio cada una.
        private static void MostrarHistorialPredicciones(Usuario usuario)
        {
            Console.WriteLine($"\n===== Historial de predicciones de {usuario.Nombre} =====");
            if (usuario.Predicciones.Count == 0)
            {
                Console.WriteLine("(todavía no cargó ninguna predicción)");
                return;
            }

            foreach (var prediccion in usuario.Predicciones)
            {
                string tipo = prediccion is PrediccionCampeon ? "Campeón" : "Resultado";
                Console.WriteLine($"- [{tipo}] {prediccion.Descripcion(),-40} | Estado: {prediccion.Estado.Nombre,-9} | Puntos: {prediccion.ObtenerPuntos()}");
            }

            int acertadas = usuario.Predicciones.Count(p => p.Estado.Nombre == "Acertado");
            int falladas = usuario.Predicciones.Count(p => p.Estado.Nombre == "Fallado");
            int pendientes = usuario.Predicciones.Count(p => p.Estado.Nombre == "Pendiente");
            Console.WriteLine($"\nTotal: {acertadas} acertadas, {falladas} falladas, {pendientes} pendientes | {usuario.Puntaje} pts");
        }

        // ----------------------------------------------------------------
        // Ranking
        // ----------------------------------------------------------------

        // Muestra el ranking y, ahí mismo, deja tipear el número (la
        // posición en la tabla) de un usuario para ver su historial.
        private static void VerRanking()
        {
            var ordenado = ranking.ObtenerRankingOrdenado();
            ranking.MostrarRanking();
            if (ordenado.Count == 0) return;

            Console.Write("\nNúmero de usuario para ver su historial (enter para omitir): ");
            string entrada = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(entrada)) return;

            if (int.TryParse(entrada, out int idx) && idx >= 1 && idx <= ordenado.Count)
                MostrarHistorialPredicciones(ordenado[idx - 1]);
            else
                Console.WriteLine("Usuario inválido.");
        }

        // Genera predicciones "razonables" para un bot, usando el Factory Method.
        private static void GenerarPrediccionesAutomaticas(Usuario bot)
        {
            foreach (var partido in copa.TodosLosPartidos().Where(p => !p.EstaFinalizado()))
            {
                int gl = rnd.Next(0, 4);
                int gv = rnd.Next(0, 4);
                factoryResultado.CrearPrediccion(bot, new object[] { partido, gl, gv });
            }

            if (copa.Selecciones.Count > 0)
            {
                var seleccionAzar = copa.Selecciones[rnd.Next(copa.Selecciones.Count)];
                factoryCampeon.CrearPrediccion(bot, new object[] { copa, seleccionAzar });
            }
        }

        // ----------------------------------------------------------------
        // Predicciones (usuario elegido por consola)
        // ----------------------------------------------------------------

        private static void MenuPredicciones()
        {
            var usuario = SeleccionarUsuario();
            if (usuario == null) return;

            Console.WriteLine("\n1. Predecir resultado de un partido");
            Console.WriteLine("2. Predecir campeón del torneo");
            Console.WriteLine("0. Volver");
            Console.Write("Opción: ");
            string op = Console.ReadLine();
            Console.WriteLine();

            switch (op)
            {
                case "1": PredecirResultado(usuario); break;
                case "2": PredecirCampeon(usuario); break;
                case "0": break;
                default: Console.WriteLine("Opción inválida."); break;
            }
        }

        private static void PredecirResultado(Usuario usuario)
        {
            var pendientes = copa.TodosLosPartidos().Where(p => !p.EstaFinalizado()).ToList();
            if (pendientes.Count == 0) { Console.WriteLine("No hay partidos pendientes para predecir."); return; }

            for (int i = 0; i < pendientes.Count; i++)
                Console.WriteLine($"{i + 1}. [{pendientes[i].Etapa}] {pendientes[i].Local.Nombre} vs {pendientes[i].Visitante.Nombre}");
            Console.WriteLine("0. Cancelar");
            Console.Write("Elegí el partido (número): ");
            string entradaPartido = Console.ReadLine();
            if (entradaPartido == "0" || string.IsNullOrWhiteSpace(entradaPartido)) { Console.WriteLine("Cancelado."); return; }
            if (!int.TryParse(entradaPartido, out int pIdx) || pIdx < 1 || pIdx > pendientes.Count)
            {
                Console.WriteLine("Partido inválido.");
                return;
            }
            var partido = pendientes[pIdx - 1];

            // Si ya tenía una predicción editable (Pendiente) para este partido, se reemplaza.
            var existente = usuario.Predicciones
                .OfType<PrediccionResultado>()
                .FirstOrDefault(p => p.Partido == partido && p.PuedeEditar());
            if (existente != null)
            {
                usuario.Predicciones.Remove(existente);
                Console.WriteLine("(se reemplazó tu predicción anterior para este partido)");
            }

            Console.Write($"Goles de {partido.Local.Nombre}: ");
            int.TryParse(Console.ReadLine(), out int gl);
            Console.Write($"Goles de {partido.Visitante.Nombre}: ");
            int.TryParse(Console.ReadLine(), out int gv);

            factoryResultado.CrearPrediccion(usuario, new object[] { partido, gl, gv });
            Console.WriteLine("Predicción cargada (estado: Pendiente).");
        }

        private static void PredecirCampeon(Usuario usuario)
        {
            var existente = usuario.Predicciones.OfType<PrediccionCampeon>().FirstOrDefault(p => p.PuedeEditar());
            if (existente != null)
            {
                usuario.Predicciones.Remove(existente);
                Console.WriteLine("(se reemplazó tu predicción de campeón anterior)");
            }

            for (int i = 0; i < copa.Selecciones.Count; i++)
                Console.WriteLine($"{i + 1}. {copa.Selecciones[i].Nombre}");
            Console.WriteLine("0. Cancelar");
            Console.Write("Elegí el campeón (número): ");
            string entradaSeleccion = Console.ReadLine();
            if (entradaSeleccion == "0" || string.IsNullOrWhiteSpace(entradaSeleccion)) { Console.WriteLine("Cancelado."); return; }
            if (!int.TryParse(entradaSeleccion, out int sIdx) || sIdx < 1 || sIdx > copa.Selecciones.Count)
            {
                Console.WriteLine("Selección inválida.");
                return;
            }

            factoryCampeon.CrearPrediccion(usuario, new object[] { copa, copa.Selecciones[sIdx - 1] });
            Console.WriteLine("Predicción de campeón cargada (estado: Pendiente).");
        }

        // ----------------------------------------------------------------
        // Estrategia (Strategy) — se elige una vez y se reutiliza
        // ----------------------------------------------------------------

        private static void MenuElegirEstrategia()
        {
            var nueva = ElegirEstrategia();
            if (nueva != null)
            {
                estrategiaActual = nueva;
                Console.WriteLine($"Estrategia actual: {estrategiaActual.Nombre}");
            }
            // Si nueva es null, ElegirEstrategia ya mostró el mensaje
            // correspondiente (cancelado u opción inválida).
        }

        private static IEstrategiaSimulacion ElegirEstrategia()
        {
            Console.WriteLine("Elegí la estrategia de simulación:");
            Console.WriteLine("1. Ranking FIFA");
            Console.WriteLine("2. Historial de enfrentamientos");
            Console.WriteLine("3. Estadísticas de rendimiento reciente");
            Console.WriteLine("0. Cancelar");
            Console.Write("Opción: ");
            string op = Console.ReadLine();

            switch (op)
            {
                case "1": return new EstrategiaRankingFifa();
                case "2": return new EstrategiaHistorica();
                case "3": return new EstrategiaEstadistica();
                case "0":
                    Console.WriteLine($"Cancelado. Se mantiene la estrategia actual: {estrategiaActual.Nombre}");
                    return null;
                default:
                    Console.WriteLine($"Opción inválida. Se mantiene la estrategia actual: {estrategiaActual.Nombre}");
                    return null;
            }
        }

        // ----------------------------------------------------------------
        // Simulación — usa siempre estrategiaActual (no vuelve a preguntar).
        // La fase eliminatoria se genera sola (RevisarAvanceDeFase(), llamada
        // directa al final de cada Partido.Simular): no hay ninguna opción de
        // menú para "generarla a mano".
        // ----------------------------------------------------------------

        private static void MenuSimulacion()
        {
            Console.WriteLine($"Estrategia actual: {estrategiaActual.Nombre}");
            Console.WriteLine("\n¿Qué querés simular?");
            Console.WriteLine("1. Una etapa específica");
            Console.WriteLine("2. Un partido puntual");
            Console.WriteLine("3. Todo el torneo (lo que esté pendiente)");
            Console.WriteLine("0. Volver");
            Console.Write("Opción: ");
            string op = Console.ReadLine();
            Console.WriteLine();

            switch (op)
            {
                case "1":
                    var etapas = ObtenerEtapasSimulables();
                    if (etapas.Count == 0) { Console.WriteLine("No hay etapas con partidos pendientes para simular."); return; }
                    Console.WriteLine("Etapas disponibles:");
                    for (int i = 0; i < etapas.Count; i++)
                        Console.WriteLine($"{i + 1}. {etapas[i].Nombre}");
                    Console.WriteLine("0. Cancelar");
                    Console.Write("Elegí una etapa (número): ");
                    string entradaEtapa = Console.ReadLine();
                    if (entradaEtapa == "0" || string.IsNullOrWhiteSpace(entradaEtapa)) { Console.WriteLine("Cancelado."); return; }
                    if (int.TryParse(entradaEtapa, out int eIdx) && eIdx >= 1 && eIdx <= etapas.Count)
                    {
                        Console.WriteLine();
                        etapas[eIdx - 1].Simular(estrategiaActual);
                        Console.WriteLine("\nEtapa simulada.");
                    }
                    else Console.WriteLine("Etapa inválida.");
                    break;

                case "2":
                    var partidos = copa.TodosLosPartidos().Where(p => !p.EstaFinalizado()).ToList();
                    if (partidos.Count == 0) { Console.WriteLine("No hay partidos pendientes."); return; }
                    for (int i = 0; i < partidos.Count; i++)
                        Console.WriteLine($"{i + 1}. [{partidos[i].Etapa}] {partidos[i].Local.Nombre} vs {partidos[i].Visitante.Nombre}");
                    Console.WriteLine("0. Cancelar");
                    Console.Write("Elegí un partido (número): ");
                    string entradaPartido = Console.ReadLine();
                    if (entradaPartido == "0" || string.IsNullOrWhiteSpace(entradaPartido)) { Console.WriteLine("Cancelado."); return; }
                    if (int.TryParse(entradaPartido, out int pIdx) && pIdx >= 1 && pIdx <= partidos.Count)
                    {
                        Console.WriteLine();
                        partidos[pIdx - 1].Simular(estrategiaActual);
                    }
                    else Console.WriteLine("Partido inválido.");
                    break;

                case "3":
                    copa.Raiz.Simular(estrategiaActual);
                    Console.WriteLine("\nSe simuló todo lo pendiente del torneo.");
                    break;

                case "0":
                    break;

                default:
                    Console.WriteLine("Opción inválida.");
                    break;
            }
        }

        // Recorre el árbol del Composite y devuelve las Etapas que tienen al
        // menos un partido (se excluyen contenedores todavía vacíos, como
        // "Fase Eliminatoria" antes de que termine la fase de grupos).
        private static List<Etapa> ObtenerEtapasSimulables()
        {
            var resultado = new List<Etapa>();

            void Recorrer(ComponenteTorneo nodo)
            {
                if (nodo is Etapa etapa)
                {
                    if (etapa.ObtenerPartidos().Count > 0)
                        resultado.Add(etapa);
                    foreach (var hijo in etapa.Hijos)
                        Recorrer(hijo);
                }
            }

            if (copa.Raiz is Etapa raiz)
            {
                foreach (var hijo in raiz.Hijos)
                    Recorrer(hijo);
            }

            return resultado;
        }
    }
}
