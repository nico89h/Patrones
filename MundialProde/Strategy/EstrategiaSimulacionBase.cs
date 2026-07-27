using System;

namespace MundialProde.Strategy
{
    // Clase de soporte (no forma parte de la interfaz Strategy) para no repetir
    // la generación de goles según una distribución Poisson en cada estrategia.
    public abstract class EstrategiaSimulacionBase
    {
        private static readonly Random Rnd = new Random();

        protected int Poisson(double lambda)
        {
            double l = Math.Exp(-lambda);
            int k = 0;
            double p = 1.0;
            do
            {
                k++;
                p *= Rnd.NextDouble();
            } while (p > l);
            return k - 1;
        }
    }
}
