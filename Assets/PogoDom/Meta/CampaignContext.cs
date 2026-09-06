namespace PogoDom.Meta
{
    public enum CampaignMode
    {
        None = 0,
        Attack = 1,
        Defense = 2
    }

    public sealed class CampaignContext
    {
        public CampaignMode Mode { get; }
        public CityId PlayerCity { get; }
        public CityId? TargetCity { get; }

        private CampaignContext(CampaignMode mode, CityId playerCity, CityId? targetCity)
        {
            Mode = mode;
            PlayerCity = playerCity;
            TargetCity = targetCity;
        }

        public static CampaignContext None(CityId playerCity) => new CampaignContext(CampaignMode.None, playerCity, null);
        public static CampaignContext Attack(CityId playerCity, CityId targetCity) => new CampaignContext(CampaignMode.Attack, playerCity, targetCity);
        public static CampaignContext Defense(CityId playerCity) => new CampaignContext(CampaignMode.Defense, playerCity, playerCity);
    }
}
