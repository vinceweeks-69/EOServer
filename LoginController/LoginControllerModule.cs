using Autofac;
using InventoryServiceLayer.Interface;
using LoginServiceLayer.Implementation;
using LoginServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EO.Login_Controller
{
    public class LoginControllerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new LoginController(c.Resolve<LoginManager>(), c.Resolve<IInventoryManager>()));
        }
    }
}
