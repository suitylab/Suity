using System;

namespace Suity.Helpers;

public static class IServiceProviderExtensions
{
    /// <summary>
    /// Obtain the specified service.
    /// </summary>
    /// <typeparam name="T">The type of service to obtain.</typeparam>
    /// <param name="provider">The service provider.</param>
    /// <returns>The service, or null if the service is not available.</returns>
    public static T GetService<T>(this IServiceProvider provider) where T : class
    {
        return provider.GetService(typeof(T)) as T;
    }

    public static T GetServiceOrSelf<T>(this IServiceProvider provider) where T : class
    {
        return provider.GetService(typeof(T)) as T ?? provider as T;
    }


    /// <summary>
    /// Obtain the specified service and execute the specified action.
    /// </summary>
    /// <typeparam name="T">The type of service to obtain.</typeparam>
    /// <param name="provider">The service provider.</param>
    /// <param name="action">The action to execute. If the service is not available, the action will not be executed.</param>
    public static void DoServiceAction<T>(this IServiceProvider provider, Action<T> action) where T : class
    {
        T service = provider.GetService(typeof(T)) as T;
        if (service != null)
        {
            action(service);
        }
    }

    public static void QueueServiceAction<T>(this IServiceProvider provider, Action<T> action) where T : class
    {
        if (provider.GetService(typeof(T)) is T service)
        {
            QueuedAction.Do(() => action(service));
        }
    }
}