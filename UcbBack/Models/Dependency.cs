﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace UcbBack.Models
{
    [Table("ADMNALRRHH.Dependency")]
    public class Dependency
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { set; get; }

        [Required(ErrorMessage = "Te olvidaste de {0}")]
        public string Cod { get; set; }

        [Required(ErrorMessage = "Te olvidaste de {0}")]
        public string Name { get; set; }

        public Dependency Parent { get; set; }

        [Required(ErrorMessage = "Te olvidaste de {0}")]
        [ForeignKey("Parent")]
        [Column("Parent")]
        public int? ParentId { get; set; }

        [Required(ErrorMessage = "Te olvidaste de {0}")]
        public int OrganizationalUnitId { get; set; }

        public OrganizationalUnit OrganizationalUnit { get; set; }
    }
}