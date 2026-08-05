using MundialProde.Usuarios;
using MundialProde.Models;

namespace MundialProde.Predicciones
{
    public class PrediccionResultado : Prediccion
    {
        private readonly Partido _partido;
        private readonly int _golesLocalPredichos;
        private readonly int _golesVisitantePredichos;

        public PrediccionResultado(Usuario usuario, Partido partido, int golesLocal, int golesVisitante)
            : base(usuario)
        {
            _partido = partido;
            _golesLocalPredichos = golesLocal;
            _golesVisitantePredichos = golesVisitante;
        }
        public bool CorrespondeAPartido(Partido partido) => _partido == partido;

        private bool EsResultadoExacto()
            => _partido.EstaFinalizado()
               && _partido.GolesLocal == _golesLocalPredichos
               && _partido.GolesVisitante == _golesVisitantePredichos;

        private bool AciertaGanador()
        {
            if (!_partido.EstaFinalizado()) return false;
            return Signo(_partido.GolesLocal, _partido.GolesVisitante)
                == Signo(_golesLocalPredichos, _golesVisitantePredichos);
        }

        private static string Signo(int golesLocal, int golesVisitante)
        {
            if (golesLocal > golesVisitante) return "Local";
            if (golesVisitante > golesLocal) return "Visitante";
            return "Empate";
        }

        public override bool Acerto() => EsResultadoExacto() || AciertaGanador();

        public override int PuntosBase => EsResultadoExacto() ? 3 : 1;

        public override string Tipo => "Resultado";

        public override string Descripcion()
            => $"{_partido.Local.Nombre} {_golesLocalPredichos} - {_golesVisitantePredichos} {_partido.Visitante.Nombre}";
    }
}
