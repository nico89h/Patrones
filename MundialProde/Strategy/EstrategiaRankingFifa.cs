using System;
using MundialProde.Models;

namespace MundialProde.Strategy
{
    public class EstrategiaRankingFifa : EstrategiaSimulacionBase, IEstrategiaSimulacion
    {
        public string Nombre => "Ranking FIFA";

        public (int golesLocal, int golesVisitante) Simular(Seleccion local, Seleccion visitante)
        {
            // A menor ranking FIFA, mejor el equipo. La diferencia se traduce
            // en una ventaja de "goles esperados" (lambda de Poisson).
            double diferencia = visitante.RankingFifa - local.RankingFifa;
            double lambdaLocal = Math.Max(0.3, 1.4 + diferencia / 25.0);
            double lambdaVisitante = Math.Max(0.3, 1.1 - diferencia / 25.0);

            return (Poisson(lambdaLocal), Poisson(lambdaVisitante));
        }
    }
}
