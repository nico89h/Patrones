using System;
using System.Collections.Generic;
using MundialProde.Strategy;
using MundialProde.Observer;

namespace MundialProde.Models
{
    // ===== PATRON COMPOSITE (hoja) + PATRON OBSERVER (subject/notificador) =====
    // Un Partido es la hoja del árbol del torneo (Composite) y, a la vez, el
    // "publisher" que avisa a sus observadores (ej: el Ranking) cuando finaliza.
    // El avance de fases (armar Cuartos/Semifinal/Final) NO es un observer ni
    // una clase aparte: es una función privada que se engancha acá
    // (ver AlFinalizarPartido) y se llama directo, sin ningún patrón de por medio.
    public class Partido : ComponenteTorneo, IPartidoNotificador
    {
        private static readonly Random _rndPenales = new Random();
        internal static Action AlFinalizarPartido;
        public Seleccion Local { get; }
        public Seleccion Visitante { get; }
        public int GolesLocal { get; private set; }
        public int GolesVisitante { get; private set; }
        public string Etapa { get; }
        public Seleccion GanadorPenales { get; private set; }

        private EstadoPartido _estado = new EstadoPendiente();

        private readonly List<IObservadorPartido> _observadores = new List<IObservadorPartido>();

        public Partido(Seleccion local, Seleccion visitante, string etapa)
            : base($"{local.Nombre} vs {visitante.Nombre}")
        {
            Local = local;
            Visitante = visitante;
            Etapa = etapa;
        }

        public void Suscribir(IObservadorPartido observador) => _observadores.Add(observador);
        public void Desuscribir(IObservadorPartido observador) => _observadores.Remove(observador);

        public void Notificar()
        {
            foreach (var obs in _observadores)
                obs.ActualizarPuntaje(this);
        }

        private Seleccion Ganador => _estado.ObtenerGanador(this);

        public Seleccion GanadorDefinitivo => Ganador ?? GanadorPenales;

        public override void Simular(IEstrategiaSimulacion estrategia)
            => _estado.Simular(this, estrategia);

        internal void RegistrarResultado(int golesLocal, int golesVisitante)
        {
            GolesLocal = golesLocal;
            GolesVisitante = golesVisitante;
        }
        internal void CambiarEstado(EstadoPartido estado) => _estado = estado;
        internal void ActualizarEstadisticasGrupo()
        {
            Local.RegistrarResultadoPartido(GolesLocal, GolesVisitante);
            Visitante.RegistrarResultadoPartido(GolesVisitante, GolesLocal);
        }

        internal void DefinirGanadorPorPenales()
            => GanadorPenales = _rndPenales.Next(2) == 0 ? Local : Visitante;

        public override List<Partido> ObtenerPartidos() => new List<Partido> { this };

        public override bool EstaFinalizado() => _estado.EstaFinalizado();

        public override ComponenteTorneo Buscar(string nombre) => Nombre == nombre ? this : null;

        public override void Mostrar(string indent = "")
        {
            string resultado = _estado.ObtenerResultado(this);
            Console.WriteLine($"{indent}[{Etapa}] {Local.Nombre} {resultado} {Visitante.Nombre}  ({_estado.Nombre})");
        }
        public string DescripcionCorta() => $"[{Etapa}] {Local.Nombre} vs {Visitante.Nombre}";
        public bool EsLaFinal() => Etapa == "Final";
    }
}
