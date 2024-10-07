using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Primitives;
using System.Text.RegularExpressions;
using System.Text;

namespace CrudWithMongoDB.Middleware
{
    public class EncryptionMiddleWare
    {
        private readonly RequestDelegate _next;
        private static string _secretKey;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EncryptionMiddleWare> _logger;

        public EncryptionMiddleWare(RequestDelegate next, IConfiguration configuration, ILogger<EncryptionMiddleWare> logger)
        {
            _next = next;
            _secretKey = configuration["Key:SecretKey"] ?? throw new ArgumentNullException("secretKey", "Secret key cannot be null");
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;  // Save the original response stream

            using (var newBodyStream = new MemoryStream())
            {
                var controllerAction = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
                var methodName = controllerAction?.ActionName ?? "Unknown";
                var controllerName = controllerAction?.ControllerName ?? "Unknown";


                string parameters = string.Empty;

                if (context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    context.Request.EnableBuffering();
                    using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
                    {
                        parameters = await reader.ReadToEndAsync();
                        context.Request.Body.Position = 0;  // Reset stream position after reading
                    }
                }
                else if (context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase) || context.Request.Method.Equals("DELETE", StringComparison.OrdinalIgnoreCase))
                {
                    parameters = context.Request.QueryString.ToString();
                }

                string logParameters = MaskPasswordInLog(parameters);

                try
                {
                    // Decrypt Query Parameters
                    var decryptedParams = new Dictionary<string, StringValues>();

                    foreach (var param in context.Request.Query)
                    {
                        var decryptedValue = EncryptionHelper.EncryptionHelper.Decrypt(param.Value, _secretKey);

                        if (string.IsNullOrEmpty(decryptedValue) || decryptedValue.ToLower() == "null")
                        {
                            decryptedParams.Add(param.Key, StringValues.Empty);
                        }
                        else
                        {
                            decryptedParams.Add(param.Key, decryptedValue);
                        }
                    }
                    //var queryString = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(string.Empty, decryptedParams);
                    //context.Request.QueryString = new QueryString(queryString);
                    // Update query parameters in context.Request.Query directly
                    var decryptedQueryString = new QueryCollection(decryptedParams);
                    context.Request.Query = decryptedQueryString;

                    // Replace request body if it's a POST with decrypted data
                    if (context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) && context.Request.ContentLength > 0)
                    {
                        var bodyAsText = await new StreamReader(context.Request.Body).ReadToEndAsync();
                        context.Request.Body.Position = 0;  // Reset body position after reading

                        if (!string.IsNullOrEmpty(bodyAsText))
                        {
                            try
                            {
                                var enytpt = EncryptionHelper.EncryptionHelper.Encrypt(bodyAsText, _secretKey);

                                var decryptedBody = EncryptionHelper.EncryptionHelper.Decrypt(bodyAsText, _secretKey);
                                var buffer = Encoding.UTF8.GetBytes(decryptedBody);
                                context.Request.Body = new MemoryStream(buffer);
                                context.Request.Body.Position = 0;  // Reset position for further processing
                            }
                            catch
                            {
                                context.Request.Body.Position = 0;  // In case of decryption failure, use original body
                            }
                        }
                    }

                    context.Response.Body = newBodyStream;  // Replace the response body stream
                    await _next(context);  // Continue with the next middleware

                    newBodyStream.Seek(0, SeekOrigin.Begin);
                    var plainTextResponse = await new StreamReader(newBodyStream).ReadToEndAsync();
                    var encryptedResponse = EncryptionHelper.EncryptionHelper.Decrypt(plainTextResponse, _secretKey);
                    var encryptedData = Encoding.UTF8.GetBytes(encryptedResponse);

                    context.Response.Body = originalBodyStream;  // Restore original response stream
                    await context.Response.Body.WriteAsync(encryptedData, 0, encryptedData.Length);  // Write encrypted response
                }
                catch (Exception ex)
                {
                    await HandleExceptionAsync(context, ex);  // Handle any exceptions
                }
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An error occurred while processing the request");
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Internal Server Error from Middleware"
            }.ToString());
        }

        private string MaskPasswordInLog(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Mask 'password' field in JSON logs
            return Regex.Replace(input, @"(?i)(""password""\s*:\s*"")(.*?)(?="")", "$1****", RegexOptions.IgnoreCase);
        }
    }
}
