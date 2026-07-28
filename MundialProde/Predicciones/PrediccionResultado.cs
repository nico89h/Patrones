using MundialProde.Usuarios;
using MundialProde.Models;

namespace MundialProde.Predicciones
{
    public class PrediccionResultado : Prediccion
    {
        public Partido Partido { get; }
        public int GolesLocalPredichos { get; }
        public int GolesVisitantePredichos { get; }

        public PrediccionResultado(Usuario usuario, Partido partido, int golesLocal, int golesVisitante)
            : base(usuario)
        {
            Partido = partido;
            GolesLocalPredichos = golesLocal;
            GolesVisitantePredichos = golesVisitante;
        }

        private bool EsResultadoExacto()
            => Partido.EstaFinalizado()
               && Partido.GolesLocal == GolesLocalPredichos
               && Partido.GolesVisitante == GolesVisitantePredichos;

        private bool AciertaGanador()
        {
            if (!Partido.EstaFinalizado()) return false;
            return Signo(Partido.GolesLocal, Partido.GolesVisitante)
                == Signo(GolesLocalPredichos, GolesVisitantePredichos);
        }

        private static string Signo(int golesLocal, int golesVisitante)
        {
            if (golesLocal > golesVisitante) return "Local";
            if (golesVisitante > golesLocal) return "Visitante";
            return "Empate";
        }

        // "Acertar" (para pasar a estado Acertado) incluye tanto clavar el resultado
        // exacto como solo acertar quién gana (o el empate).
        public override bool Acerto() => EsResultadoExacto() || AciertaGanador();

        // 3 puntos si acertó el marcador exacto, 1 punto si solo acertó el ganador/empate.
        public override int PuntosBase => EsResultadoExacto() ? 3 : 1;

        public override string Descripcion()
            => $"{Partido.Local.Nombre} {GolesLocalPredichos} - {GolesVisitantePredichos} {Partido.Visitante.Nombre}";
    }
}
