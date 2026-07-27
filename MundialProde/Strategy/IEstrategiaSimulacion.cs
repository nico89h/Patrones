using MundialProde.Models;

namespace MundialProde.Strategy
{
    // ===== PATRON STRATEGY =====
    // Permite intercambiar el criterio de simulación de un partido de forma
    // dinámica y transparente para quien lo usa (Partido / Etapa / CopaMundo).
    public interface IEstrategiaSimulacion
    {
        string Nombre { get; }
        (int golesLocal, int golesVisitante) Simular(Seleccion local, Seleccion visitante);
    }
}
