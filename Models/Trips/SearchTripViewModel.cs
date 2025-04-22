using System.ComponentModel.DataAnnotations;

namespace Carvisto.Models
{
    public class SearchTripViewModel
    {
        public string StartLocation { get; set; }
        public string EndLocation { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? DepartureDate { get; set; }
    }
}