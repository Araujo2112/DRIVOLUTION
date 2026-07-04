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

        // Modelos distintos presentes na encomenda, separados por " / ".
        // Uma encomenda pode conter produtos de mais do que um modelo
        // (agrega vários ManufacturingOrders/Products) — não assumimos 1:1.
        public string ModelName { get; set; } = string.Empty;

        // Previsão de quando o ÚLTIMO carro pendente da encomenda fica pronto
        // (= quando a encomenda toda fica concluída). Null se já está 100% concluída
        // ou se não há dados suficientes para prever.
        public DateTime? EtaUtc { get; set; }
        public bool EtaIsMlPrediction { get; set; }

        // Usado apenas internamente pelo Service para calcular o EtaUtc acima;
        // nunca é serializado para o cliente.
        [System.Text.Json.Serialization.JsonIgnore]
        public List<int> PendingProductIds { get; set; } = new();
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
        public string ModelName { get; set; } = string.Empty;
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