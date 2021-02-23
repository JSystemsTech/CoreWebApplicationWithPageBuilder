using DbFacade.DataLayer.Models;
using DbFacade.DataLayer.Models.Attributes;
using System;
using System.Collections.Generic;

namespace DomainLayer.Models
{
    public class IdentityData:DbDataModel
    {
        [DbColumn("Guid")]
        public Guid Guid { get; private set; }
        [DbColumn("IsActive")]
        public bool IsActive { get; private set; }
        [DbColumn("Name")]
        public string Name { get; private set; }
        [DbColumn("Roles")]
        public IEnumerable<string> Roles { get; private set; }
    }
}
