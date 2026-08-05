using MundialProde.Usuarios;
using MundialProde.Models;

namespace MundialProde.Predicciones
{
    public class PrediccionCampeon : Prediccion
    {
        private readonly CopaMundo _copa;
        private readonly Seleccion _seleccionPredicha;

        public PrediccionCampeon(Usuario usuario, CopaMundo copa, Seleccion seleccionPredicha)
            : base(usuario)
        {
            _copa = copa;
            _seleccionPredicha = seleccionPredicha;
        }
        public override bool Acerto() => _copa.EsCampeon(_seleccionPredicha);

        public override int PuntosBase => 10;

        public override string Tipo => "Campeón";

        public override string Descripcion() => $"Campeón: {_seleccionPredicha.Nombre}";
    }
}
