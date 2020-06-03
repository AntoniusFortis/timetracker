﻿using System.ComponentModel.DataAnnotations;

namespace Timetracker.Entities.Models
{
    public class State
    {
        public byte Id { get; set; }

        [StringLength( 60 )]
        public string Title { get; set; }
    }
}