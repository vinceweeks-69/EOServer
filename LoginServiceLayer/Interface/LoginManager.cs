using EO.Persistence;
using EO.ViewModels.DataModels;
using LoginServiceLayer.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginServiceLayer.Interface
{
    public class LoginManager : ILoginManager
    {
        private IEOPersistence persistence;

        //public LoginManager()
        //{
        //    int debug = 1;
        //}

        public LoginManager(IEOPersistence persistence)
        {
            this.persistence = persistence;
        }

        public LoginDTO GetUser(LoginDTO request)
        {
            return persistence.GetUser(request);
        }
    }
}
