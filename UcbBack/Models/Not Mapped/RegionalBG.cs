﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using UcbBack.Models.Not_Mapped.CustomDataAnnotations;

namespace UcbBack.Models
{
    public class RegionalBG
    {
        public int Id { set; get; }
        public string Abr { get; set; }
        public string Name { get; set; }
        public int BranchesId { set; get; }
    }
}