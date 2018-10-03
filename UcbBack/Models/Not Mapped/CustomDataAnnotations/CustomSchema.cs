﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace UcbBack.Models.Not_Mapped.CustomDataAnnotations
{
    public class CustomSchema : System.ComponentModel.DataAnnotations.Schema.TableAttribute
    {
        private new static String Schema = "ADMNALRRHH";
        public CustomSchema(string name) : base(addSchema(name))
        {        
        }

        private static string addSchema(string table)
        {
            return Schema + "." + table;
        }
    }
}