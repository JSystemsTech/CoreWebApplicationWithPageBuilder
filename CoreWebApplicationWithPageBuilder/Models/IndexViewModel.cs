using DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWebApplicationWithPageBuilder.Models
{
    public class IndexViewModel
    {
        public IEnumerable<IdentityData> Users { get; set; }
    }
}
