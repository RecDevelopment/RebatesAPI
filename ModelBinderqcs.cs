namespace RebatesAPI
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public class FormDataJsonBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!context.Metadata.IsComplexType)
            {
                return null;
            }

            if (context.Metadata.ModelType.GetCustomAttributes<FromBodyAttribute>().Any())
            {
                // Use the built-in JSON model binder for request body
                return null;
            }

            if (context.BindingInfo.BindingSource == BindingSource.Form ||
                context.BindingInfo.BindingSource == BindingSource.FormFile)
            {
                var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
                return new FormDataJsonBinder(context.Metadata.ModelType, loggerFactory);
            }

            return null;
        }

        private class FormDataJsonBinder : IModelBinder
        {
            private readonly Type _modelType;
            private readonly ILoggerFactory _loggerFactory;

            public FormDataJsonBinder(Type modelType, ILoggerFactory loggerFactory)
            {
                _modelType = modelType;
                _loggerFactory = loggerFactory;
            }

            public async Task BindModelAsync(ModelBindingContext bindingContext)
            {
                if (bindingContext == null)
                {
                    throw new ArgumentNullException(nameof(bindingContext));
                }

                var logger = _loggerFactory.CreateLogger(bindingContext.ModelType.FullName);

                try
                {
                    using var streamReader = new StreamReader(bindingContext.HttpContext.Request.Body);
                    var formValues = await streamReader.ReadToEndAsync();
                    var model = JsonConvert.DeserializeObject(formValues, _modelType);
                    bindingContext.Result = ModelBindingResult.Success(model);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to bind model from form data");
                    bindingContext.Result = ModelBindingResult.Failed();
                }
            }
        }
    }

}
