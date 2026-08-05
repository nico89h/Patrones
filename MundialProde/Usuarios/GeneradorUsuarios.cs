using System;
using System.Collections.Generic;

namespace MundialProde.Usuarios
{
    // Genera usuarios "bot" con nombres aleatorios
    public static class GeneradorUsuarios
    {
        private static readonly string[] Apodos =
        {
            "ElCraque", "FieraDelBarrio", "PronosticoFacil", "OjoClinico", "GoleadorNato",
            "TacticoDeSofa", "AnalistaDeQuiniela", "SuperFan", "ElVidente", "PalitoDeOro",
            "NoFalloUna", "CabezaCaliente", "LaGarra", "OjoDeAguila", "PicaditoFC",
            "ReyDelPrcode", "DTdeButaca", "SegundoArquero", "ElCabezazo", "Gambetin"
        };

        private static readonly Random _rnd = new Random();

        public static Usuario GenerarAleatorio()
        {
            string apodo = Apodos[_rnd.Next(Apodos.Length)];
            int numero = _rnd.Next(10, 999);
            return new Usuario($"{apodo}{numero}", esBot: true);
        }

        public static List<Usuario> GenerarVarios(int cantidad)
        {
            var lista = new List<Usuario>();
            for (int i = 0; i < cantidad; i++)
                lista.Add(GenerarAleatorio());
            return lista;
        }
    }
}
