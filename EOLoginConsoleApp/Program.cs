using Autofac;
using Autofac.Integration.WebApi;
using EO.Login_Controller;
using EO.Persistence;
using EO.ViewModels.ControllerModels;
using EO.ViewModels.DataModels;
using InventoryServiceLayer.Implementation;
using LoginServiceLayer;
using LoginServiceLayer.Interface;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Topshelf;

namespace EOLoginConsoleApp
{
    //http://localhost:9000/swagger/docs/v1
    //http://localhost:9000/swagger/ui/index
    public class LoginConsoleApp
    {

        static void Main(string[] args)
        {
            //string baseAddress = "http://localhost:9000/";
            //string baseAddress = "http://192.168.1.3:9000/";

            StartOptions options = new StartOptions();
            options.Urls.Add("http://localhost:9000");
            options.Urls.Add("http://127.0.0.1:9000");
            //options.Urls.Add(GetNetworkConfig());

            //options.Urls.Add("http://192.168.0.1");
            //options.Urls.Add("http://192.168.1.134");

            //options.Urls.Add("http://10.0.0.4:9000"); //Me royalwood

            //options.Urls.Add("http://10.1.10.148:9000");  //Me EO

            //options.Urls.Add("http://99.125.200.187:9000");    //Me FL

            //options.Urls.Add("http://192.168.0.129:9000");    //Me CA

            //options.Urls.Add("http://192.168.1.134:9000/");   //Thom

            options.Urls.Add("http://10.1.10.36:9000");   //Thom

            options.Urls.Add("http://*:9000");

            //options.Urls.Add("http://eo.hopto.org:9000");  //No-Ip Vince's account

            //options.Urls.Add("http://192.168.1.1:9000");
            //options.Urls.Add("http://192.168.1.2:9000");
            //options.Urls.Add("http://192.168.1.134:9000");


            // Start OWIN host 
            using (WebApp.Start<Startup>(options))
            //using (WebApp.Start<Startup>(url: baseAddress))
            {
                // Create HttpCient and make a request to api/values 
                HttpClient client = new HttpClient();

                //var response = client.GetAsync(baseAddress + "api/login").Result;

                Console.WriteLine("The Elegant Orchids Login Service is now running");

                LoginController controller = new LoginController();

                while (Console.ReadLine() == String.Empty)
                {

                }
            }
        }
    }


    public class HttpMessageHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        {
            bool authorized = false;

            if (request.RequestUri.AbsoluteUri.Contains("swagger"))
            {
                authorized = true;
            }
            else
            {
                if (request.Headers.Authorization != null)
                {
                    if (request.Headers.Authorization.Scheme == "Basic")
                    {
                        //"QWRtaW46YUExIQ==" is EO
                        //"IEFkbWluOmFBMSE=" is Royalwood

                        if (request.Headers.Authorization.Parameter == "IEFkbWluOmFBMSE=")
                        {
                            request.Headers.Add("EO-Header", "Admin:aA1!");
                            authorized = true;
                        }
                    }
                }
                else
                {
                    authorized = ValidateKey(request);
                }
            }

            if (authorized)
            {
                Task<HttpResponseMessage> response = base.SendAsync(request, cancellationToken);

                if (response.Result.IsSuccessStatusCode)
                {
                    IEnumerable<string> values;
                    request.Headers.TryGetValues("EO-Header", out values);
                    if (values != null && values.ToList().Count == 1)
                    {
                        response.Result.Headers.Add("EO-Header", values.First());
                    }
                }

                return response;
            }
            else
            {
                CancellationTokenSource _tokenSource = new CancellationTokenSource();
                cancellationToken = _tokenSource.Token;
                _tokenSource.Cancel();
                HttpResponseMessage response = new HttpResponseMessage();
                response = request.CreateResponse(HttpStatusCode.Unauthorized);
                response.Content = new StringContent("Not authorized");
                return base.SendAsync(request, cancellationToken).ContinueWith(task =>
                {
                    return response;
                });
            }
        }

        private bool ValidateKey(HttpRequestMessage message)
        {
            bool success = false;

            var query = message.RequestUri.ParseQueryString();
            IEnumerable<string> values;
            message.Headers.TryGetValues("EO-Header", out values);
            if (values != null && values.ToList().Count == 1)
            {
                LoginManager manager = new LoginManager(new EOPersistence());

                LoginDTO login = new LoginDTO();
                string[] userNamePwd = values.First().Split(':');
                if (userNamePwd.Length == 2)
                {
                    login.UserName = userNamePwd[0].Trim();
                    login.Password = userNamePwd[1].Trim();
                    login = manager.GetUser(login);
                    if (login.UserId > 0)
                    {
                        success = true;
                    }
                }
            }

            return success;
        }
    }

    public class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.MessageHandlers.Add(new HttpMessageHandler());

            config.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings();

            SwaggerConfig.Register(config);

            ContainerBuilder containerBuilder = new ContainerBuilder();
                       
            containerBuilder.RegisterModule<PersistenceModule>();

            containerBuilder.RegisterModule<InventoryServiceLayerModule>();

            containerBuilder.RegisterModule<LoginManagerModule>();

            containerBuilder.RegisterModule<LoginControllerModule>();

            IContainer container = containerBuilder.Build();

            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            appBuilder.UseAutofacMiddleware(container);

            appBuilder.UseAutofacWebApi(config);

            appBuilder.UseWebApi(config);
        }
    }
}
