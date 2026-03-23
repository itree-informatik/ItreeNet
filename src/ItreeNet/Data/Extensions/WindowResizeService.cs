using Microsoft.JSInterop;
using Serilog;

namespace ItreeNet.Data.Extensions
{
    public static class WindowResizeService
    {
        public static event Func<Task>? WindowResize;
        public static int? WindowWidth { get; private set; }
        public static int? WindowHeight { get; private set; }

        [JSInvokable]
        public static async Task RaiseWindowResizeEvent(int width, int height)
        {
            WindowWidth = width;
            WindowHeight = height;
            try
            {
                if (WindowResize != null)
                    await WindowResize.Invoke()!;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "WindowResizeEvent handler failed");
            }
        }
    }
}
