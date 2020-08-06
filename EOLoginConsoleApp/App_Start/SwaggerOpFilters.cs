using Swashbuckle.Swagger;

namespace EOLoginConsoleApp
{

    ///// <summary>
    ///// Operation filter to add the requirement of the custom header
    ///// </summary>
    //public class MyHeaderFilter : IOperationFilter
    //{
    //    public void Apply(Operation operation, OperationFilterContext context)
    //    {
    //        if (operation.Parameters == null)
    //            operation.Parameters = new List<IParameter>();

    //        operation.Parameters.Add(new NonBodyParameter
    //        {
    //            Name = "MY-HEADER",
    //            In = "header",
    //            Type = "string",
    //            Required = true // set to false if this is optional
    //        });
    //    }
    //}
}
