using System;

namespace MundialProde.Models
{
    public class Seleccion : IComparable<Seleccion>
    {
        public string Nombre { get; }
        public int RankingFifa { get; }
        public string Grupo { get; }
        private int _puntosGrupo = 0;
        private int _golesFavor = 0;
        private int _golesContra = 0;

        public Seleccion(string nombre, int rankingFifa, string grupo)
        {
            Nombre = nombre;
            RankingFifa = rankingFifa;
            Grupo = grupo;
        }

        public void RegistrarResultadoPartido(int golesAFavor, int golesEnContra)
        {
            _golesFavor += golesAFavor;
            _golesContra += golesEnContra;

            if (golesAFavor > golesEnContra) _puntosGrupo += 3;
            else if (golesAFavor == golesEnContra) _puntosGrupo += 1;
        }
        public int CompareTo(Seleccion otra)
        {
            if (otra == null) return -1;

            int porPuntos = otra._puntosGrupo.CompareTo(_puntosGrupo);
            if (porPuntos != 0) return porPuntos;

            int porDiferenciaGoles = (otra._golesFavor - otra._golesContra)
                .CompareTo(_golesFavor - _golesContra);
            if (porDiferenciaGoles != 0) return porDiferenciaGoles;

            return otra._golesFavor.CompareTo(_golesFavor);
        }

        public override string ToString() => Nombre;
    }
}
