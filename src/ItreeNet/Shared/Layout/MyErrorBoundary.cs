using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Serilog.Events;

namespace ItreeNet.Shared.Layout
{
    public class MyErrorBoundary : ErrorBoundary
    {
        [Inject] private IErrorBoundaryLogger? ErrorBoundaryLogger { get; set; }
        [Inject] private Serilog.ILogger? Logger { get; set; }
        const string MessageTemplate = "SeriLog: {t:HH:mm:ss} {text} ";

        public new Exception? CurrentException => base.CurrentException;

        /// <summary>
        /// Invoked by the base class when an error is being handled. The default implementation
        /// logs the error.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> being handled.</param>
        protected override async Task OnErrorAsync(Exception exception)
        {
            await ErrorBoundaryLogger!.LogErrorAsync(exception);

            if (Logger != null)
            {
                var text = exception.Message;
                //Logger.Fatal(exception, MessageTemplate, DateTime.Now, text);
                Logger.Write(LogEventLevel.Error, exception, MessageTemplate, DateTime.Now, text);
            }
        }
    }
}
