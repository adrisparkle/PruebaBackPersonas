﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace UcbBack.Models
{
    [Table("ADMNALRRHH.Dist_Pregrado")]
    public class Dist_Pregrado
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { set; get; }

        public string Document { get; set; }
        public string FullName { get; set; }
        public Decimal TotalNeto { get; set; }
        public string Carrera { get; set; }
        public string CUNI { get; set; }
        public string Dependency { get; set; }

        public decimal Porcentaje { get; set; }
        public string segmentoOrigen { get; set; }
        public string mes { get; set; }
        public string gestion { get; set; }

    }
}