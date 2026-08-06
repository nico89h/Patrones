using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using MundialProde.Models;

namespace MundialProde.Servicios
{
    public class TorneoService
    {
        private readonly CopaMundo _copa;
        private readonly Ranking _ranking;

        public CopaMundo ObtenerCopa() => _copa;

        public TorneoService(CopaMundo copa, Ranking ranking)
        {
            _copa = copa;
            _ranking = ranking;
        }

        public void Inicializar()
        {
            Partido.AlFinalizarPartido = RevisarAvanceDeFase;

            CargarSeleccionesDesdeJson("Data/selecciones.json");

            // Raiz -> "Fase de Grupos" -> "Grupo A".."Grupo D" -> Partidos
            var faseDeGrupos = new Etapa("Fase de Grupos");
            _copa.AgregarAlArbol(faseDeGrupos);
            GenerarFaseDeGrupos(faseDeGrupos);

            // Raiz -> "Fase Eliminatoria" (arranca vacía; RevisarAvanceDeFase()
            // la va completando sola, a medida que la fase de grupos y cada
            // ronda van terminando
            var faseEliminatoria = new Etapa("Fase Eliminatoria");
            _copa.AgregarAlArbol(faseEliminatoria);

            int cantidadGrupos = faseDeGrupos.ObtenerHijos().Count;
            Console.WriteLine($"\n'{_copa.ObtenerNombre()}' creada con {_copa.ObtenerSelecciones().Count} selecciones en {cantidadGrupos} grupos.");
        }

        private void CargarSeleccionesDesdeJson(string rutaRelativa)
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
                _copa.AgregarSeleccion(new Seleccion(dto.Nombre, dto.RankingFifa, dto.Grupo));
        }

        private void GenerarFaseDeGrupos(Etapa contenedor)
        {
            var grupos = _copa.ObtenerSelecciones().GroupBy(s => s.ObtenerGrupo()).OrderBy(g => g.Key);
            foreach (var grupo in grupos)
            {
                var etapaGrupo = new Etapa($"Grupo {grupo.Key}");
                var equipos = grupo.ToList();

                for (int i = 0; i < equipos.Count; i++)
                    for (int j = i + 1; j < equipos.Count; j++)
                        CrearYAgregarPartido(etapaGrupo, equipos[i], equipos[j], etapaGrupo.ObtenerNombre());

                contenedor.Agregar(etapaGrupo);
            }
        }

        private void CrearYAgregarPartido(Etapa etapa, Seleccion local, Seleccion visitante, string nombreEtapa)
        {
            var partido = new Partido(local, visitante, nombreEtapa);
            partido.Suscribir(_ranking);
            etapa.Agregar(partido);
        }


        private void SuscribirTodosLosPartidos(Etapa etapa)
        {
            foreach (var partido in etapa.ObtenerPartidos())
                partido.Suscribir(_ranking);
        }

        private void RevisarAvanceDeFase()
        {
            var faseDeGrupos = _copa.BuscarComponente("Fase de Grupos") as Etapa;
            var faseEliminatoria = _copa.BuscarComponente("Fase Eliminatoria");
            if (faseDeGrupos == null || faseEliminatoria == null) return;
            if (!faseDeGrupos.EstaFinalizado()) return;

            var cuartos = _copa.BuscarComponente("Cuartos de Final") as Etapa;
            if (cuartos == null)
            {
                cuartos = ArmarCuartos(faseDeGrupos);
                faseEliminatoria.Agregar(cuartos);
                SuscribirTodosLosPartidos(cuartos);
                Console.WriteLine("\n>> Fase de grupos completa: se generaron automáticamente los Cuartos de Final. <<");
                return;
            }
            if (!cuartos.EstaFinalizado()) return;

            var semis = _copa.BuscarComponente("Semifinal") as Etapa;
            if (semis == null)
            {
                semis = ArmarSiguienteRonda(cuartos, "Semifinal");
                faseEliminatoria.Agregar(semis);
                SuscribirTodosLosPartidos(semis);
                Console.WriteLine("\n>> Cuartos de Final completos: se generó automáticamente la Semifinal. <<");
                return;
            }
            if (!semis.EstaFinalizado()) return;

            var final = _copa.BuscarComponente("Final") as Etapa;
            if (final == null)
            {
                final = ArmarSiguienteRonda(semis, "Final");
                faseEliminatoria.Agregar(final);
                SuscribirTodosLosPartidos(final);
                Console.WriteLine("\n>> Semifinal completa: se generó automáticamente la Final. <<");
            }
        }

        private Etapa ArmarCuartos(Etapa faseDeGrupos)
        {
            var clasificados = new List<Seleccion>();
            var gruposOrdenados = faseDeGrupos.ObtenerHijos().OfType<Etapa>().OrderBy(e => e.ObtenerNombre());

            foreach (var grupo in gruposOrdenados)
            {
                var equiposDelGrupo = grupo.ObtenerPartidos()
                    .SelectMany(p => new[] { p.ObtenerLocal(), p.ObtenerVisitante() })
                    .Distinct()
                    .OrderBy(s => s)
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

        private Etapa ArmarSiguienteRonda(Etapa etapaAnterior, string nombreNuevaEtapa)
        {
            var partidosAnteriores = etapaAnterior.ObtenerPartidos();
            var nuevaEtapa = new Etapa(nombreNuevaEtapa);

            for (int i = 0; i < partidosAnteriores.Count; i += 2)
            {
                var ganador1 = partidosAnteriores[i].ObtenerGanadorDefinitivo();
                var ganador2 = partidosAnteriores[i + 1].ObtenerGanadorDefinitivo();
                ArmarPartidoEliminatoria(nuevaEtapa, ganador1, ganador2, nombreNuevaEtapa);
            }

            return nuevaEtapa;
        }

        private void ArmarPartidoEliminatoria(Etapa etapa, Seleccion local, Seleccion visitante, string nombreEtapa)
        {
            var partido = new Partido(local, visitante, nombreEtapa);
            etapa.Agregar(partido);
        }

        public List<Etapa> ObtenerEtapasSimulables()
        {
            var resultado = new List<Etapa>();

            void Recorrer(ComponenteTorneo nodo)
            {
                if (nodo is Etapa etapa)
                {
                    if (etapa.ObtenerPartidos().Count > 0)
                        resultado.Add(etapa);
                    foreach (var hijo in etapa.ObtenerHijos())
                        Recorrer(hijo);
                }
            }

            foreach (var hijo in _copa.ObtenerRamasPrincipales())
                Recorrer(hijo);

            return resultado;
        }
    }
}
