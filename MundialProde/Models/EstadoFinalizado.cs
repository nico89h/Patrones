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
            if (partido.GolesLocal > partido.GolesVisitante) return partido.Local;
            if (partido.GolesVisitante > partido.GolesLocal) return partido.Visitante;
            return null;
        }

        public override string ObtenerResultado(Partido partido)
        {
            string resultado = $"{partido.GolesLocal} - {partido.GolesVisitante}";
            if (partido.GanadorPenales != null)
                resultado += $" (definido por penales: {partido.GanadorPenales.Nombre})";
            return resultado;
        }
    }
}
