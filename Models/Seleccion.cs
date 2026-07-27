namespace MundialProde.Models
{
    public class Seleccion
    {
        public string Nombre { get; }
        public int RankingFifa { get; }
        public string Grupo { get; }

        // Estadísticas de la fase de grupos, usadas para armar la tabla de posiciones.
        public int PuntosGrupo { get; set; } = 0;
        public int GolesFavor { get; set; } = 0;
        public int GolesContra { get; set; } = 0;

        public int DiferenciaGoles => GolesFavor - GolesContra;

        public Seleccion(string nombre, int rankingFifa, string grupo)
        {
            Nombre = nombre;
            RankingFifa = rankingFifa;
            Grupo = grupo;
        }

        public override string ToString() => Nombre;
    }
}
