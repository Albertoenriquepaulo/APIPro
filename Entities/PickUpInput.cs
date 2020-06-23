﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vantex.Technician.Service.Entities
{
    public class PickUpInput
    {
        public PickUpInput(int baseId, DateTime tDate, string shortBase)
        {
            BaseId = baseId;
            this.Today = tDate;
            ShortBase = shortBase ?? throw new ArgumentNullException(nameof(shortBase));
        }

        public int BaseId { get; set; }
        public DateTime Today { get; set; }

        public string ShortBase { get; set; }

    }
}
