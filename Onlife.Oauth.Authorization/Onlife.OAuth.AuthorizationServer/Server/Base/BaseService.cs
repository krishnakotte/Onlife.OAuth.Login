using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Onlife.OAuth.AuthorizationServer.Models;

namespace Onlife.OAuth.AuthorizationServer.Server.Base
{
    public class BaseService
    {
        private OnlifeOAuthModelDataContext _context;
        private StructureMap.IContext context;

        public BaseService(StructureMap.IContext context)
        {
            // TODO: Complete member initialization
            this.context = context;
        }

        public BaseService()
        {

        }

        public OnlifeOAuthModelDataContext DBContext
        {
            get
            {
                if (_context == null)
                    _context = new OnlifeOAuthModelDataContext();
                return _context;
            }
        }
    }
}
