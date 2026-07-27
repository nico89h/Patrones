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

        public override bool Acerto()
        {
            return Partido.EstaFinalizado()
                && Partido.GolesLocal == GolesLocalPredichos
                && Partido.GolesVisitante == GolesVisitantePredichos;
        }

        public override int PuntosBase => 3;

        public override string Descripcion()
            => $"{Partido.Local.Nombre} {GolesLocalPredichos} - {GolesVisitantePredichos} {Partido.Visitante.Nombre}";
    }
}
