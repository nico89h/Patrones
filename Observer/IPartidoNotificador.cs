namespace MundialProde.Observer
{
    // ===== PATRON OBSERVER (subject/notificador) =====
    public interface IPartidoNotificador
    {
        void Suscribir(IObservadorPartido observador);
        void Desuscribir(IObservadorPartido observador);
        void Notificar();
    }
}
