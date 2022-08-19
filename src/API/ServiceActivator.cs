namespace API
{
    public static class ServiceActivator
    {
        internal static IServiceProvider _serviceProvider;

        public static void UseServiceActivator(this IApplicationBuilder app)
        {
            _serviceProvider = app.ApplicationServices;
        }

        public static IServiceScope GetScope()
        {
            return _serviceProvider?.GetRequiredService<IServiceScopeFactory>()
                                    .CreateScope();
        }
    }
}
