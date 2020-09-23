using Autofac;
using EO.Persistence;
using LoginServiceLayer.Implementation;
using LoginServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginServiceLayer
{
    public class LoginManagerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<LoginManager>().As<ILoginManager>();
            builder.Register(c => new LoginManager(c.Resolve<IEOPersistence>()));
        }
    }
}
