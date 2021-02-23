using DbFacade.DataLayer.Models;
using DbFacade.Exceptions;
using DomainLayer.Methods;
using DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DomainLayer.DomainFacade
{
    
    public interface IWebAppDomainFacade
    {
        IDbResponse<IdentityData> GetIdentities();

        IdentityData GetIdentity(Guid userGuid);
    }
    internal class DomainFacadeBase
    {
        
        protected virtual void HandleSQLExecutionException(SQLExecutionException sqlEx) { throw new Exception("SQL Execution Exception", sqlEx); }
        protected virtual void HandleValidationException<TDbParamsModel>(ValidationException<TDbParamsModel> validationEx) where TDbParamsModel : DbParamsModel { throw new Exception("Validation Exception", validationEx); }
        protected IDbResponse<TDbDataModel> Run<TDbParamsModel, TDbDataModel>(Func<TDbParamsModel, IDbResponse<TDbDataModel>> methodHandler, TDbParamsModel model)
            where TDbParamsModel : DbParamsModel
            where TDbDataModel : DbDataModel
        {
            try
            {
                return methodHandler(model);
            }
            catch (SQLExecutionException sqlEx)
            {
                HandleSQLExecutionException(sqlEx);
                return default;
            }
            catch (ValidationException<TDbParamsModel> validationEx)
            {
                HandleValidationException(validationEx);
                return default;
            }

        }
        protected IDbResponse Run<TDbParamsModel>(Func<TDbParamsModel, IDbResponse> methodHandler, TDbParamsModel model)
            where TDbParamsModel : DbParamsModel
        {
            try
            {
                return methodHandler(model);
            }
            catch (SQLExecutionException sqlEx)
            {
                HandleSQLExecutionException(sqlEx);
                return default;
            }
            catch (ValidationException<TDbParamsModel> validationEx)
            {
                HandleValidationException<TDbParamsModel>(validationEx);
                return default;
            }

        }
        protected IDbResponse<TDbDataModel> Run<TDbDataModel>(Func<IDbResponse<TDbDataModel>> methodHandler)
            where TDbDataModel : DbDataModel
        {
            try
            {
                return methodHandler();
            }
            catch (SQLExecutionException sqlEx)
            {
                HandleSQLExecutionException(sqlEx);
                return default;
            }

        }
        protected IDbResponse Run<TDbDataModel>(Func<IDbResponse> methodHandler)
            where TDbDataModel : DbDataModel
        {
            try
            {
                return methodHandler();
            }
            catch (SQLExecutionException sqlEx)
            {
                HandleSQLExecutionException(sqlEx);
                return default;
            }

        }


    }
    
    internal class WebAppDomainFacade : DomainFacadeBase, IWebAppDomainFacade
    {
        public WebAppDomainFacade() { }
        protected override void HandleSQLExecutionException(SQLExecutionException sqlEx)
        {
            base.HandleSQLExecutionException(sqlEx);
        }
        protected override void HandleValidationException<TDbParamsModel>(ValidationException<TDbParamsModel> validationEx)
        {
            base.HandleValidationException(validationEx);
        }
        public IDbResponse<IdentityData> GetIdentities() => Run(WebAppDbMethods.GetIdentities.Execute);

        public IdentityData GetIdentity(Guid userGuid) => Run(WebAppDbMethods.GetIdentity.Execute, new DbParamsModel<Guid>(userGuid)).FirstOrDefault();
    }
}
