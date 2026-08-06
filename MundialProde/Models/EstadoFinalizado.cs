using MundialProde.Strategy;

namespace MundialProde.Models
{
    public class EstadoFinalizado : EstadoPartido
    {
        public override string Nombre => "Finalizado";

        public override void Simular(Partido partido, IEstrategiaSimulacion estrategia) { }

        public override bool EstaFinalizado() => true;

        public override Seleccion ObtenerGanador(Partido partido)
        {
            if (partido.ObtenerGolesLocal() > partido.ObtenerGolesVisitante()) return partido.ObtenerLocal();
            if (partido.ObtenerGolesVisitante() > partido.ObtenerGolesLocal()) return partido.ObtenerVisitante();
            return null;
        }

        public override string ObtenerResultado(Partido partido)
        {
            string resultado = $"{partido.ObtenerGolesLocal()} - {partido.ObtenerGolesVisitante()}";
            if (partido.ObtenerGanadorPenales() != null)
                resultado += $" (definido por penales: {partido.ObtenerGanadorPenales().ObtenerNombre()})";
            return resultado;
        }
    }
}
