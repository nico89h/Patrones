using MundialProde.Strategy;

namespace MundialProde.Models
{
    public class EstadoPendiente : EstadoPartido
    {
        public override string Nombre => "Pendiente";

        public override void Simular(Partido partido, IEstrategiaSimulacion estrategia)
        {
            var resultado = estrategia.Simular(partido.ObtenerLocal(), partido.ObtenerVisitante());
            partido.RegistrarResultado(resultado.golesLocal, resultado.golesVisitante);
            partido.CambiarEstado(new EstadoFinalizado());

            partido.ActualizarEstadisticasGrupo();

            // En fase eliminatoria no puede quedar sin ganador, se define por penales.
            if (!partido.ObtenerEtapa().StartsWith("Grupo") && partido.ObtenerGolesLocal() == partido.ObtenerGolesVisitante())
                partido.DefinirGanadorPorPenales();

            // Notifica a los observer
            partido.Notificar();

            // este partido se completó la etapa actual y arma la siguiente.
            Partido.AlFinalizarPartido?.Invoke();

            partido.Mostrar();
        }

        public override bool EstaFinalizado() => false;

        public override Seleccion ObtenerGanador(Partido partido) => null;

        public override string ObtenerResultado(Partido partido) => "vs";
    }
}
