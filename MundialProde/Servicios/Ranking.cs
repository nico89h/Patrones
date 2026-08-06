using System;
using System.Collections.Generic;
using System.Linq;
using MundialProde.Usuarios;
using MundialProde.Models;
using MundialProde.Observer;
using MundialProde.Predicciones;

namespace MundialProde.Servicios
{
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
                var prediccionesDelPartido = usuario.ObtenerPredicciones()
                    .OfType<PrediccionResultado>()
                    .Where(p => p.CorrespondeAPartido(partido) && p.PuedeEditar());

                foreach (var prediccion in prediccionesDelPartido)
                {
                    prediccion.Evaluar(); // dispara la transición de estado (State)
                    if (prediccion.ObtenerPuntos() > 0)
                        usuario.SumarPuntos(prediccion.ObtenerPuntos());
                }
            }

            if (partido.EsLaFinal() && partido.ObtenerGanadorDefinitivo() != null)
            {
                _copa.DefinirCampeon(partido.ObtenerGanadorDefinitivo());
                EvaluarPrediccionesCampeon();
            }
        }

        private void EvaluarPrediccionesCampeon()
        {
            foreach (var usuario in _usuarios)
            {
                var prediccionesCampeon = usuario.ObtenerPredicciones()
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
            => _usuarios.OrderByDescending(u => u.ObtenerPuntaje()).ToList();

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
                string tag = u.EsBot() ? "(bot)" : "";
                Console.WriteLine($"{pos,3}. {u.ObtenerNombre(),-22} {tag,-6} {u.ObtenerPuntaje()} pts");
                pos++;
            }
        }
    }
}
