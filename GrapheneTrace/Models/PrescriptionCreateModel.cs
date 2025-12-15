using GrapheneTrace.Controllers;

namespace GrapheneTrace.Models
{
    public class PrescriptionCreateModel
    {
        public int PatientId { get; set; }
        public required string DrugName { get; set; } = string.Empty;
        public required string Dosage { get; set; } = string.Empty;
        public required string Frequency { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Instructions { get; set; }
        internal List<PrescriptionDetail> Prescriptions { get; set; } = new();
    }
}
