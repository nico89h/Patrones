using System;
using MundialProde.Models;

namespace MundialProde.Strategy
{
    public class EstrategiaEstadistica : EstrategiaSimulacionBase, IEstrategiaSimulacion
    {
        private readonly Random _rnd = new Random();

        public string Nombre => "Estadísticas de rendimiento reciente";

        public (int golesLocal, int golesVisitante) Simular(Seleccion local, Seleccion visitante)
        {
            // Combina el ranking FIFA con un factor aleatorio de "forma reciente".
            double formaLocal = _rnd.NextDouble() * 0.6;
            double formaVisitante = _rnd.NextDouble() * 0.6;
            double diferenciaRanking = (visitante.ObtenerRankingFifa() - local.ObtenerRankingFifa()) / 30.0;

            double lambdaLocal = Math.Max(0.3, 1.2 + diferenciaRanking + formaLocal);
            double lambdaVisitante = Math.Max(0.3, 1.2 - diferenciaRanking + formaVisitante);

            return (Poisson(lambdaLocal), Poisson(lambdaVisitante));
        }
    }
}
