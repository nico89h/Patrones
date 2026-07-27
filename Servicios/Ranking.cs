using System;
using System.Collections.Generic;
using System.Linq;
using MundialProde.Usuarios;
using MundialProde.Models;
using MundialProde.Observer;
using MundialProde.Predicciones;

namespace MundialProde.Servicios
{
    // ===== PATRON OBSERVER (observador concreto) =====
    // El Ranking se suscribe a cada Partido. Cuando un partido finaliza:
    //  1) evalúa (y transiciona el ESTADO de) las predicciones de resultado de ese partido
    //  2) si el partido finalizado es la Final, define el campeón y evalúa las
    //     predicciones de campeón
    //  3) suma los puntos correspondientes al perfil de cada usuario
    //
    // OJO: el Ranking NO se reordena acá. El orden es una vista derivada de los
    // puntajes, que se calcula al vuelo recién cuando alguien pide verlo
    // (ObtenerRankingOrdenado / MostrarRanking). Así se evita reordenar una
    // estructura completa cada vez que termina un partido.
    public class Ranking : IObservadorPartido
    {
        private readonly List<Usuario> _usuarios;
        private readonly CopaMundo _copa;

        public Ranking(List<Usuario> usuarios, CopaMundo copa)
        {
            _usuarios = usuarios;
            _copa = copa;
        }

        public void ActualizarPuntaje(Partido partido)
        {
            foreach (var usuario in _usuarios)
            {
                var prediccionesDelPartido = usuario.Predicciones
                    .OfType<PrediccionResultado>()
                    .Where(p => p.Partido == partido && p.PuedeEditar());

                foreach (var prediccion in prediccionesDelPartido)
                {
                    prediccion.Evaluar(); // dispara la transición de estado (State)
                    if (prediccion.ObtenerPuntos() > 0)
                        usuario.SumarPuntos(prediccion.ObtenerPuntos());
                }
            }

            if (partido.Etapa == "Final" && partido.GanadorDefinitivo != null)
            {
                _copa.Campeon = partido.GanadorDefinitivo;
                EvaluarPrediccionesCampeon();
            }
        }

        private void EvaluarPrediccionesCampeon()
        {
            foreach (var usuario in _usuarios)
            {
                var prediccionesCampeon = usuario.Predicciones
                    .OfType<PrediccionCampeon>()
                    .Where(p => p.PuedeEditar());

                foreach (var prediccion in prediccionesCampeon)
                {
                    prediccion.Evaluar();
                    if (prediccion.ObtenerPuntos() > 0)
                        usuario.SumarPuntos(prediccion.ObtenerPuntos());
                }
            }
        }

        public List<Usuario> ObtenerRankingOrdenado()
            => _usuarios.OrderByDescending(u => u.Puntaje).ToList();

        public void MostrarRanking()
        {
            var ordenado = ObtenerRankingOrdenado();
            Console.WriteLine("\n===== RANKING GENERAL =====");
            if (ordenado.Count == 0)
            {
                Console.WriteLine("(todavía no hay usuarios)");
                return;
            }

            int pos = 1;
            foreach (var u in ordenado)
            {
                string tag = u.EsBot ? "(bot)" : "";
                Console.WriteLine($"{pos,3}. {u.Nombre,-22} {tag,-6} {u.Puntaje} pts");
                pos++;
            }
        }
    }
}
