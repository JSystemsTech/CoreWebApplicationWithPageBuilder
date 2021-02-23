using DbFacade.DataLayer.CommandConfig;
using DbFacade.DataLayer.Models;
using DomainLayer.Connection;
using DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.Methods
{
    internal class WebAppDbMethods
    {
        public static readonly IParameterlessDbCommandMethod<IdentityData> GetIdentities
            = WebAppDbConnection.GetIdentities.CreateParameterlessMethod<IdentityData>();

        public static readonly IDbCommandMethod<DbParamsModel<Guid>, IdentityData> GetIdentity
            = WebAppDbConnection.GetIdentity.CreateMethod<DbParamsModel<Guid>, IdentityData>(p => {
                p.Add("UserGuid", p.Factory.Create(m => m.Param1));
            });
    }
}
