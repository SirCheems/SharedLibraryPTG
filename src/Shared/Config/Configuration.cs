namespace Shared.Config;

using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;

public static class Configuration
{
    // Singleton en memoria para evitar recargar archivos repetidamente
    private static StringDictionary? appConfiguration;

    // Devuelve la configuración cargada, si no está cargada, la carga
    private static StringDictionary GetAppConfiguration()
    {
        return appConfiguration == null ?
          appConfiguration = LoadAppConfiguration() : appConfiguration;
    }

    // Carga la configuración desde los archivos de configuración según el modo de despliegue
    private static StringDictionary LoadAppConfiguration()
    {
        var cfg = new StringDictionary();
        var basePath = Directory.GetCurrentDirectory();

        // Detecta modo de despliegue (development/prod/etc) desde environment variables
        var deploymentMode = Environment.GetEnvironmentVariable("DEPLOYMENT_MODE") ?? "development";

        // Archivos de configuración posibles
        var paths = new string[] { "appsettings.cfg",
          $"appsettings.{deploymentMode}.cfg" };

        foreach (var path in paths)
        {
            var file = Path.Combine(basePath, path);

            if (File.Exists(file))
            {
                var tmp = LoadConfigurationFile(file);

                // Mezcla las claves encontradas en el diccionario final
                foreach (string k in tmp.Keys) { cfg[k] = tmp[k]; }
            }
        }

        return cfg;
    }

    // Lee un archivo de configuración línea por línea y devuelve un diccionario key=value
    public static StringDictionary LoadConfigurationFile(string file)
    {
        string[] lines = File.ReadAllLines(file);

        var cfg = new StringDictionary();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].TrimStart();

            // Ignora líneas vacías o comentarios (#)
            if (string.IsNullOrEmpty(line) || line.StartsWith('#')) { continue; }

            var kv = line.Split('=', 2, StringSplitOptions.TrimEntries);

            cfg[kv[0]] = kv[1];
        }

        return cfg;
    }

    // Devuelve un valor de configuración como string, null si no existe
    public static string? Get(string key)
    {
        return Get(key, null);
    }

    // Devuelve un valor de configuración, con fallback a 'val' si no existe
    public static string? Get(string key, string? val)
    {
        // Primero busca en environment variables, luego en archivos de configuración
        return Environment.GetEnvironmentVariable(key)
         ?? GetAppConfiguration()[key] ?? val;
    }

    // Versión genérica que convierte a tipo T
    public static T Get<T>(string key)
    {
        return Get<T>(key, default!);
    }

    // Devuelve valor convertido a tipo T, con fallback si falla
    public static T Get<T>(string key, T val)
    {
        string? value = Environment.GetEnvironmentVariable(key)
          ?? GetAppConfiguration()[key];

        if (string.IsNullOrWhiteSpace(value)) { return val; }

        try
        {
            Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            // Maneja enums
            if (targetType.IsEnum)
            {
                return (T)Enum.Parse(targetType, value, ignoreCase: true);
            }

            // Conversión genérica usando TypeDescriptor
            var converter = TypeDescriptor.GetConverter(targetType);

            if (converter != null && converter.CanConvertFrom(typeof(string)))
            {
                return (T)converter.ConvertFromString(null,
                  CultureInfo.InvariantCulture, value)!;
            }

            // Conversión fallback
            return (T)Convert.ChangeType(value, targetType,
              CultureInfo.InvariantCulture);
        }
        catch
        {
            // Si falla la conversión, devuelve el valor por defecto
            return val;
        }
    }
}