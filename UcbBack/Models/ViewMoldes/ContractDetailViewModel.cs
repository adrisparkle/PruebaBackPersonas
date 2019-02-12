﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace UcbBack.Models.ViewMoldes
{
    [NotMapped]
    public class ContractDetailViewModel
    {
        public int Id { get; set; }
        public string CUNI { get; set; }
        public string Document { get; set; }
        public string FullName { get; set; }
        public string Dependency { get; set; }
        public string DependencyCod { get; set; }
        public string Branches { get; set; }
        public int BranchesId { get; set; }
        public string Positions { get; set; }
        public string Dedication { get; set; }
        public string Linkage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}