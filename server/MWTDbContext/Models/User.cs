﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MWTDbContext.Models
{
    
    public class User
    {
        [Key]
        public int id { get; set; }
        [Required,MaxLength(128)]
        public string Fullname { get; set; }
        [Required]
        [MaxLength(21)]
        public string Username { get; set; }
        [Required]
        [MaxLength(65)]
        public string Password { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        public string Avatar { get; set; }
        [ForeignKey("UserRoles")]
        public virtual int Role { get; set; }

        [Required]
        public DateTime createdAt { get; set; }
        [Required]
        public DateTime updatedAt { get; set; }
        [Required]
        public bool isActive { get; set; }

    }

}
