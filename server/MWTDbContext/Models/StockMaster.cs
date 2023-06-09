﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MWTDbContext.Models
{
    public class StockMaster
    {
        [Key]
        public int id { get; set; }
        [ForeignKey("productMasters")]
        public int ProductID { get; set; }
        [Required]
        public int Stock { get; set; }
       
        
        [Required]
        public DateTime updatedAt { get; set; }
        [Required]
        public DateTime createdAt { get; set; }
    }
}
