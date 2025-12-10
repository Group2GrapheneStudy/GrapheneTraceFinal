
namespace GrapheneTrace.Controllers
{
    internal class PrescriptionDetail
    {
        public int Id { get; set; }
        public string DrugName { get; set; }
        public string Dosage { get; set; }
        public string Frequency { get; set; }
        public DateTime DatePrescribed { get; set; }
        public int Quantity { get; set; }
        public string Instructions { get; set; }
    }
}