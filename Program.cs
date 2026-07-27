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
                    case "4": MenuSimulacion(); break;
                    case "5": GenerarFaseEliminatoria(); break;
                    case "6": ranking.MostrarRanking(); break;
                    case "0": salir = true; break;
                    default: Console.WriteLine("Opción inválida."); break;
                }
            }

            Console.WriteLine("\n¡Hasta la próxima!");
        }

        // ----------------------------------------------------------------
        // Inicialización
        // ----------------------------------------------------------------

        private static void IniciarCopaMundo()
        {
            Console.Write("\nNombre de la Copa del Mundo a crear (enter = 'Copa Mundial 2026'): ");
            string nombreCopa = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(nombreCopa)) nombreCopa = "Copa Mundial 2026";

            copa = new CopaMundo(nombreCopa);
            usuarios = new List<Usuario>();
            ranking = new Ranking(usuarios, copa);

            CargarSeleccionesDesdeJson("Data/selecciones.json");
            GenerarFaseDeGrupos();

            int cantidadGrupos = copa.Etapas.Count(e => e.Nombre.StartsWith("Grupo"));
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

        private static void GenerarFaseDeGrupos()
        {
            var grupos = copa.Selecciones.GroupBy(s => s.Grupo).OrderBy(g => g.Key);
            foreach (var grupo in grupos)
            {
                var etapaGrupo = new Etapa($"Grupo {grupo.Key}");
                var equipos = grupo.ToList();

                for (int i = 0; i < equipos.Count; i++)
                    for (int j = i + 1; j < equipos.Count; j++)
                        CrearYAgregarPartido(etapaGrupo, equipos[i], equipos[j], etapaGrupo.Nombre);

                copa.Etapas.Add(etapaGrupo);
            }
        }

        private static void CrearYAgregarPartido(Etapa etapa, Seleccion local, Seleccion visitante, string nombreEtapa)
        {
            var partido = new Partido(local, visitante, nombreEtapa);
            partido.Suscribir(ranking); // el Ranking se suscribe como observador de cada partido
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
            Console.WriteLine("4. Simular partidos");
            Console.WriteLine("5. Generar siguiente fase eliminatoria");
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
            Console.Write("Opción: ");
            string op = Console.ReadLine();
            Console.WriteLine();

            switch (op)
            {
                case "1":
                    Console.Write("Nombre del usuario: ");
                    string nombre = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(nombre)) { Console.WriteLine("Nombre inválido."); return; }
                    usuarios.Add(new Usuario(nombre));
                    Console.WriteLine($"Usuario '{nombre}' creado.");
                    break;

                case "2":
                    Console.Write("¿Cuántos bots generar?: ");
                    if (int.TryParse(Console.ReadLine(), out int cant) && cant > 0)
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
                    if (usuarios.Count == 0) { Console.WriteLine("Todavía no hay usuarios."); return; }
                    foreach (var u in usuarios)
                        Console.WriteLine($"- {u.Nombre,-22} {(u.EsBot ? "(bot)" : "     ")} | {u.Puntaje} pts | {u.Predicciones.Count} predicciones");
                    break;

                default:
                    Console.WriteLine("Opción inválida.");
                    break;
            }
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
            if (usuarios.Count == 0) { Console.WriteLine("Primero creá al menos un usuario."); return; }

            Console.WriteLine("Usuarios disponibles:");
            for (int i = 0; i < usuarios.Count; i++)
                Console.WriteLine($"{i + 1}. {usuarios[i].Nombre}");
            Console.Write("Elegí un usuario: ");
            if (!int.TryParse(Console.ReadLine(), out int idx) || idx < 1 || idx > usuarios.Count)
            {
                Console.WriteLine("Usuario inválido.");
                return;
            }
            var usuario = usuarios[idx - 1];

            Console.WriteLine("\n1. Predecir resultado de un partido");
            Console.WriteLine("2. Predecir campeón del torneo");
            Console.Write("Opción: ");
            string op = Console.ReadLine();
            Console.WriteLine();

            if (op == "1") PredecirResultado(usuario);
            else if (op == "2") PredecirCampeon(usuario);
            else Console.WriteLine("Opción inválida.");
        }

        private static void PredecirResultado(Usuario usuario)
        {
            var pendientes = copa.TodosLosPartidos().Where(p => !p.EstaFinalizado()).ToList();
            if (pendientes.Count == 0) { Console.WriteLine("No hay partidos pendientes para predecir."); return; }

            for (int i = 0; i < pendientes.Count; i++)
                Console.WriteLine($"{i + 1}. [{pendientes[i].Etapa}] {pendientes[i].Local.Nombre} vs {pendientes[i].Visitante.Nombre}");
            Console.Write("Elegí el partido: ");
            if (!int.TryParse(Console.ReadLine(), out int pIdx) || pIdx < 1 || pIdx > pendientes.Count)
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
            Console.Write("Elegí el campeón: ");
            if (!int.TryParse(Console.ReadLine(), out int sIdx) || sIdx < 1 || sIdx > copa.Selecciones.Count)
            {
                Console.WriteLine("Selección inválida.");
                return;
            }

            factoryCampeon.CrearPrediccion(usuario, new object[] { copa, copa.Selecciones[sIdx - 1] });
            Console.WriteLine("Predicción de campeón cargada (estado: Pendiente).");
        }

        // ----------------------------------------------------------------
        // Simulación (Strategy)
        // ----------------------------------------------------------------

        private static void MenuSimulacion()
        {
            var estrategia = ElegirEstrategia();
            if (estrategia == null) { Console.WriteLine("Opción inválida."); return; }

            Console.WriteLine("\n¿Qué querés simular?");
            Console.WriteLine("1. Toda la fase de grupos");
            Console.WriteLine("2. Una etapa específica");
            Console.WriteLine("3. Un partido puntual");
            Console.WriteLine("4. Todo lo que esté pendiente");
            Console.Write("Opción: ");
            string op = Console.ReadLine();
            Console.WriteLine();

            switch (op)
            {
                case "1":
                    foreach (var etapa in copa.Etapas.Where(e => e.Nombre.StartsWith("Grupo")))
                        etapa.Simular(estrategia);
                    Console.WriteLine("Fase de grupos simulada.");
                    break;

                case "2":
                    Console.WriteLine("Etapas disponibles:");
                    foreach (var e in copa.Etapas) Console.WriteLine($"- {e.Nombre}");
                    Console.Write("Nombre exacto de la etapa: ");
                    string nombreEtapa = Console.ReadLine();
                    var etapaElegida = copa.ObtenerEtapa(nombreEtapa);
                    if (etapaElegida == null) { Console.WriteLine("Etapa no encontrada."); return; }
                    etapaElegida.Simular(estrategia);
                    Console.WriteLine("Etapa simulada.");
                    break;

                case "3":
                    var partidos = copa.TodosLosPartidos().Where(p => !p.EstaFinalizado()).ToList();
                    if (partidos.Count == 0) { Console.WriteLine("No hay partidos pendientes."); return; }
                    for (int i = 0; i < partidos.Count; i++)
                        Console.WriteLine($"{i + 1}. [{partidos[i].Etapa}] {partidos[i].Local.Nombre} vs {partidos[i].Visitante.Nombre}");
                    Console.Write("Elegí un partido: ");
                    if (int.TryParse(Console.ReadLine(), out int pidx) && pidx >= 1 && pidx <= partidos.Count)
                    {
                        partidos[pidx - 1].Simular(estrategia);
                        Console.WriteLine("Partido simulado.");
                    }
                    else Console.WriteLine("Partido inválido.");
                    break;

                case "4":
                    foreach (var etapa in copa.Etapas)
                        etapa.Simular(estrategia);
                    Console.WriteLine("Se simuló todo lo pendiente.");
                    break;

                default:
                    Console.WriteLine("Opción inválida.");
                    break;
            }
        }

        private static IEstrategiaSimulacion ElegirEstrategia()
        {
            Console.WriteLine("\nElegí la estrategia de simulación:");
            Console.WriteLine("1. Ranking FIFA");
            Console.WriteLine("2. Historial de enfrentamientos");
            Console.WriteLine("3. Estadísticas de rendimiento reciente");
            Console.Write("Opción: ");
            string op = Console.ReadLine();

            switch (op)
            {
                case "1": return new EstrategiaRankingFifa();
                case "2": return new EstrategiaHistorica();
                case "3": return new EstrategiaEstadistica();
                default: return null;
            }
        }

        // ----------------------------------------------------------------
        // Fase eliminatoria (arma el bracket dinámicamente usando el Composite)
        // ----------------------------------------------------------------

        private static void GenerarFaseEliminatoria()
        {
            bool grupos = copa.Etapas.Where(e => e.Nombre.StartsWith("Grupo")).All(e => e.EstaFinalizado());
            if (!grupos) { Console.WriteLine("Todavía no finalizó la fase de grupos."); return; }

            var cuartos = copa.ObtenerEtapa("Cuartos de Final");
            if (cuartos == null)
            {
                copa.Etapas.Add(ArmarCuartos());
                Console.WriteLine("Se generaron los Cuartos de Final.");
                return;
            }

            if (!cuartos.EstaFinalizado()) { Console.WriteLine("Todavía no finalizaron los Cuartos de Final."); return; }

            var semis = copa.ObtenerEtapa("Semifinal");
            if (semis == null)
            {
                copa.Etapas.Add(ArmarSiguienteRonda(cuartos, "Semifinal"));
                Console.WriteLine("Se generó la Semifinal.");
                return;
            }

            if (!semis.EstaFinalizado()) { Console.WriteLine("Todavía no finalizó la Semifinal."); return; }

            var final = copa.ObtenerEtapa("Final");
            if (final == null)
            {
                copa.Etapas.Add(ArmarSiguienteRonda(semis, "Final"));
                Console.WriteLine("Se generó la Final.");
                return;
            }

            if (copa.Campeon != null)
                Console.WriteLine($"El torneo ya terminó. Campeón: {copa.Campeon.Nombre} 🏆");
            else
                Console.WriteLine("La Final ya está generada, falta simularla.");
        }

        private static Etapa ArmarCuartos()
        {
            var clasificados = new List<Seleccion>();
            var gruposOrdenados = copa.Etapas.Where(e => e.Nombre.StartsWith("Grupo")).OrderBy(e => e.Nombre);

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
            CrearYAgregarPartido(cuartos, clasificados[0], clasificados[3], "Cuartos de Final"); // 1A vs 2B
            CrearYAgregarPartido(cuartos, clasificados[2], clasificados[1], "Cuartos de Final"); // 1B vs 2A
            CrearYAgregarPartido(cuartos, clasificados[4], clasificados[7], "Cuartos de Final"); // 1C vs 2D
            CrearYAgregarPartido(cuartos, clasificados[6], clasificados[5], "Cuartos de Final"); // 1D vs 2C
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
                CrearYAgregarPartido(nuevaEtapa, ganador1, ganador2, nombreNuevaEtapa);
            }

            return nuevaEtapa;
        }
    }
}
