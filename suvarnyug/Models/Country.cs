﻿using System.ComponentModel.DataAnnotations.Schema;

namespace suvarnyug.Models
{
    [Table("countries")]
    public class Country
    {
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public ICollection<State> States { get; set; }
    }
}
