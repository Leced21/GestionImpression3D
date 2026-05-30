namespace Backend.Enums
{
    public enum PrintJobStatus
    {
        Pending = 1,        // En attente
        Queued = 2,         // En file d'attente
        Printing = 3,       // En cours d'impression
        Paused = 4,         // En pause
        Completed = 5,      // Terminé
        Failed = 6,         // Échoué
        Cancelled = 7       // Annulé
    }
}
