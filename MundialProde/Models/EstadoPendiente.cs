using MundialProde.Strategy;

namespace MundialProde.Models
{
    public class EstadoPendiente : EstadoPartido
    {
        public override string Nombre => "Pendiente";

        public override void Simular(Partido partido, IEstrategiaSimulacion estrategia)
        {
            var resultado = estrategia.Simular(partido.Local, partido.Visitante);
            partido.RegistrarResultado(resultado.golesLocal, resultado.golesVisitante);
            partido.CambiarEstado(new EstadoFinalizado());

            partido.ActualizarEstadisticasGrupo();

            // En fase eliminatoria no puede quedar un ganador ausente: se define por penales.
            if (!partido.Etapa.StartsWith("Grupo") && partido.GolesLocal == partido.GolesVisitante)
                partido.DefinirGanadorPorPenales();

            // Dispara la notificación a los observadores (ej: Ranking) -> PATRON OBSERVER
            partido.Notificar();

            // Llamada directa (sin patrón, sin clase aparte): revisa si con
            // este partido se completó la etapa actual y arma la siguiente.
            Partido.AlFinalizarPartido?.Invoke();

            // Se muestra el resultado apenas se termina de simular el partido.
            partido.Mostrar();
        }

        public override bool EstaFinalizado() => false;

        public override Seleccion ObtenerGanador(Partido partido) => null;

        public override string ObtenerResultado(Partido partido) => "vs";
    }
}
