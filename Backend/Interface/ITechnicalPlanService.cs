namespace Backend.Interface
{
    public interface ITechnicalPlanService
    {
        /// <summary>
        /// Génère un PDF avec les plans techniques d'une pièce
        /// </summary>
        Task<byte[]> GenerateTechnicalPlanPdfAsync(int pieceId);

        /// <summary>
        /// Génère un PDF avec les plans techniques d'un projet complet
        /// </summary>
        Task<byte[]> GenerateProjectTechnicalPlansPdfAsync(int projectId);
    }
}
