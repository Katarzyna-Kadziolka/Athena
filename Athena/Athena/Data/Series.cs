﻿using System;
using System.Collections.Generic;

namespace Athena.Data {
    public class Series 
    {
        public Guid Id { get; set; }
        public string SeriesName { get; set; }

        public virtual ICollection<Book> Books { get; set; }
    }
}