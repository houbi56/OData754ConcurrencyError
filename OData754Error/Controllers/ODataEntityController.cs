using Microsoft.AspNet.OData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNet.OData.Query;
using Microsoft.Extensions.Logging;
using System.Net;
using Microsoft.AspNet.OData.Routing;

namespace OData_754_Error.Controllers
{
    
    public class ODataEntityController : ODataController
    {

        EFContext Ctx;
        
        public ODataEntityController(EFContext ctx)
        {
            Ctx = ctx;
        }

        [EnableQuery(MaxNodeCount = int.MaxValue)]
        public virtual IQueryable<ODataEntity> Get(ODataQueryOptions<ODataEntity> queryOptions)
        {
            IQueryable<ODataEntity> query = Ctx.Set<ODataEntity>();
            return query;
        }
    }
}
