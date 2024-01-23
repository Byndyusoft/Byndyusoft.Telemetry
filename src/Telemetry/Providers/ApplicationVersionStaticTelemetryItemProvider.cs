namespace Byndyusoft.Telemetry.Providers
{
    using Consts;
    using Interface;

    public class ApplicationVersionStaticTelemetryItemProvider : IStaticTelemetryItemProvider
    {
        private readonly string _version;

        public ApplicationVersionStaticTelemetryItemProvider(string version)
        {
            _version = version;
        }

        public TelemetryItem[] GetTelemetryItems()
        {
            return new[]
                       {
                           new TelemetryItem(TelemetryItemNames.Version, _version)
                       };
        }
    }
}