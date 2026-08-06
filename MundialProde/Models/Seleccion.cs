using System;

namespace MundialProde.Models
{
    public class Seleccion : IComparable<Seleccion>
    {
        private readonly string _nombre;
        private readonly int _rankingFifa;
        private readonly string _grupo;
        private int _puntosGrupo = 0;
        private int _golesFavor = 0;
        private int _golesContra = 0;

        public Seleccion(string nombre, int rankingFifa, string grupo)
        {
            _nombre = nombre;
            _rankingFifa = rankingFifa;
            _grupo = grupo;
        }

        public string ObtenerNombre() => _nombre;
        public int ObtenerRankingFifa() => _rankingFifa;
        public string ObtenerGrupo() => _grupo;

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
    }
}
