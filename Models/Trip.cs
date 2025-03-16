using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Carvisto.Models
{
    // Модель для хранения данных о путешествии
    public class Trip
    {
        public int Id {get; set;} // Уникальный идентификатор
        public string From {get; set;} // Откуда
        public string To {get; set;} // Куда
        public DateTime DepartureDate {get; set;} // Дата отправления
        public decimal Price {get; set;} // Цена
        
        [BindNever]
        public string DriverId {get; set;} // ID водителя
    }
}