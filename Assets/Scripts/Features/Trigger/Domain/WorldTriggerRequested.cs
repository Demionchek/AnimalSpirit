namespace Features.Trigger.Domain
{
    public readonly struct WorldTriggerRequested
    {
        public readonly TriggerConfigSO Config;

        public WorldTriggerRequested(TriggerConfigSO config)
        {
            Config = config;
        }
    }
}