using Allegro.Core.Criteria;
using Allegro.Diagnostics;
using System.Data;

using Allegro.Core.Logging;
using Allegro.Core.Server.WebApi;
using Microsoft.AspNetCore.Mvc;
using Allegro.Core.Server.WebApi.Controller;

namespace Allegro.BuildTasks.UseCases.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class MainWSController
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Allegro.BuildTasks.UseCases.MainWS _apiWrapper = new Allegro.BuildTasks.UseCases.MainWS();

        [HttpPost]
        [ActionName("GetVersion")]
        //[Authorize(Policy = "AllowRead")]
        public string GetVersion()
        {
            //throw new NotImplementedException("GetVersion called with parms: ");
            return _apiWrapper.GetVersion();
        }

        [HttpPost]
        [ActionName("RetrieveData")]
        //[Authorize(Policy = "AllowRead")]
        public DataSet RetrieveData(MainWSRetrieveDataParam wrappedParameter)
        {
            //throw new NotImplementedException("RetrieveData called with parms: MainWSRetrieveDataParam wrappedParameter");
            return _apiWrapper.RetrieveData(wrappedParameter.dsRetrieve, wrappedParameter.tableNames, wrappedParameter.criteria);
        }

        [HttpPost]
        [ActionName("AnotherRetrieveData")]
        //[Authorize(Policy = "AllowRead")]
        public DataSet AnotherRetrieveData(MainWSAnotherRetrieveDataParam wrappedParameter)
        {
            //throw new NotImplementedException("AnotherRetrieveData called with parms: MainWSAnotherRetrieveDataParam wrappedParameter");
            return _apiWrapper.AnotherRetrieveData(wrappedParameter.dsRetrieve, wrappedParameter.tableNames, wrappedParameter.criteria);
        }


    }
    public class MainWSRetrieveDataParam
    {
        public DataSet dsRetrieve { get; set; }
        public string[] tableNames { get; set; }
        public SelectCriteria criteria { get; set; }
    }
    public class MainWSAnotherRetrieveDataParam
    {
        public DataSet dsRetrieve { get; set; }
        public string[] tableNames { get; set; }
        public SelectCriteria criteria { get; set; }
    }

}

