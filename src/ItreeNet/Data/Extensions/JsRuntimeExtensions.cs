using Microsoft.JSInterop;
using Serilog;

namespace ItreeNet.Data.Extensions;

public static class JsRuntimeExtensions
{
    /// <summary>
    /// Ruft eine JS-Funktion auf, ohne dass ein Fehler im Browser den Circuit abbricht.
    /// Ist die Verbindung bereits getrennt, wird der Aufruf ignoriert; JS-Fehler werden geloggt.
    /// </summary>
    public static async Task TryInvokeVoidAsync(this IJSRuntime jsRuntime, string identifier, params object?[]? args)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync(identifier, args);
        }
        catch (JSDisconnectedException)
        {
            // Circuit getrennt (Tab geschlossen, Verbindung weg) – nichts mehr zu tun.
        }
        catch (OperationCanceledException)
        {
            // Aufruf abgebrochen, z. B. beim Beenden des Circuits.
        }
        catch (JSException ex)
        {
            Log.Warning(ex, "JS-Aufruf {Identifier} fehlgeschlagen", identifier);
        }
    }
}
