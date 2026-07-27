using System;
using MundialProde.Models;

namespace MundialProde.Strategy
{
    public class EstrategiaHistorica : EstrategiaSimulacionBase, IEstrategiaSimulacion
    {
        public string Nombre => "Historial de enfrentamientos directos";

        public (int golesLocal, int golesVisitante) Simular(Seleccion local, Seleccion visitante)
        {
            // Simplificación: como no tenemos una base de datos histórica real,
            // se genera un factor pseudo-determinístico a partir de los nombres
            // de los equipos, simulando que hay una "tendencia histórica" entre ellos.
            int factor = (local.Nombre.GetHashCode() - visitante.Nombre.GetHashCode()) % 5;
            double lambdaLocal = Math.Max(0.4, 1.3 + factor * 0.15);
            double lambdaVisitante = Math.Max(0.4, 1.3 - factor * 0.15);

            return (Poisson(lambdaLocal), Poisson(lambdaVisitante));
        }
    }
}
