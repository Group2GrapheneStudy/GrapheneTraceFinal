
namespace GrapheneTrace.Controllers
{
    internal class PrescriptionDetail
    {
        public int Id { get; set; }
        public required string DrugName { get; set; }
        public required string Dosage { get; set; }
        public required string Frequency { get; set; }
        public DateTime DatePrescribed { get; set; }
        public int Quantity { get; set; }
        public string? Instructions { get; set; }
    }
}