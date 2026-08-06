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
        private readonly Seleccion _local;
        private readonly Seleccion _visitante;
        private int _golesLocal;
        private int _golesVisitante;
        private readonly string _etapaNombre;
        private Seleccion _ganadorPenales;

        private EstadoPartido _estado = new EstadoPendiente();

        private readonly List<IObservadorPartido> _observadores = new List<IObservadorPartido>();

        public Partido(Seleccion local, Seleccion visitante, string etapa)
            : base($"{local.ObtenerNombre()} vs {visitante.ObtenerNombre()}")
        {
            _local = local;
            _visitante = visitante;
            _etapaNombre = etapa;
        }

        public Seleccion ObtenerLocal() => _local;
        public Seleccion ObtenerVisitante() => _visitante;
        public int ObtenerGolesLocal() => _golesLocal;
        public int ObtenerGolesVisitante() => _golesVisitante;
        public string ObtenerEtapa() => _etapaNombre;
        public Seleccion ObtenerGanadorPenales() => _ganadorPenales;

        public void Suscribir(IObservadorPartido observador) => _observadores.Add(observador);
        public void Desuscribir(IObservadorPartido observador) => _observadores.Remove(observador);

        public void Notificar()
        {
            foreach (var obs in _observadores)
                obs.ActualizarPuntaje(this);
        }

        private Seleccion Ganador => _estado.ObtenerGanador(this);

        public Seleccion ObtenerGanadorDefinitivo() => Ganador ?? _ganadorPenales;

        public override void Simular(IEstrategiaSimulacion estrategia)
            => _estado.Simular(this, estrategia);

        internal void RegistrarResultado(int golesLocal, int golesVisitante)
        {
            _golesLocal = golesLocal;
            _golesVisitante = golesVisitante;
        }
        internal void CambiarEstado(EstadoPartido estado) => _estado = estado;
        internal void ActualizarEstadisticasGrupo()
        {
            _local.RegistrarResultadoPartido(_golesLocal, _golesVisitante);
            _visitante.RegistrarResultadoPartido(_golesVisitante, _golesLocal);
        }

        internal void DefinirGanadorPorPenales()
            => _ganadorPenales = _rndPenales.Next(2) == 0 ? _local : _visitante;

        public override List<Partido> ObtenerPartidos() => new List<Partido> { this };

        public override bool EstaFinalizado() => _estado.EstaFinalizado();

        public override ComponenteTorneo Buscar(string nombre) => Nombre == nombre ? this : null;

        public override void Mostrar(string indent = "")
        {
            string resultado = _estado.ObtenerResultado(this);
            Console.WriteLine($"{indent}[{_etapaNombre}] {_local.ObtenerNombre()} {resultado} {_visitante.ObtenerNombre()}  ({_estado.Nombre})");
        }
        public string DescripcionCorta() => $"[{_etapaNombre}] {_local.ObtenerNombre()} vs {_visitante.ObtenerNombre()}";
        public bool EsLaFinal() => _etapaNombre == "Final";
    }
}
