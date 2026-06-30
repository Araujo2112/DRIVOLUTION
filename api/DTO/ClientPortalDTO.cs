namespace Drivolution.DTO
{
    // Resposta de GET /api/client/orders
    public class ClientOrderSummaryDTO
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public int TotalCars { get; set; }
        public int CompletedCars { get; set; }
        public string Status { get; set; } = string.Empty; // ex: "3 de 5 concluídos"
    }

    // Resposta de GET /api/client/orders/{id}/products
    public class ClientOrderDetailDTO
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public List<ClientProductDTO> Products { get; set; } = new();
    }

    public class ClientProductDTO
    {
        public int ProductId { get; set; }
        public string SerialNumber { get; set; } = string.Empty; // VIN
        public string CurrentPhase { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime? EtaUtc { get; set; }          // null se já concluído ou sem dados ML
        public bool EtaIsMlPrediction { get; set; }
    }

    // DTO para criação de conta de cliente (POST /api/client-accounts)
    public class CreateClientAccountDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // DTO para edição de conta de cliente (PUT /api/client-accounts/{id})
    public class UpdateClientAccountDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    // DTO para repor password (PUT /api/client-accounts/{id}/reset-password)
    public class ResetClientPasswordDTO
    {
        public string NewPassword { get; set; } = string.Empty;
    }
}