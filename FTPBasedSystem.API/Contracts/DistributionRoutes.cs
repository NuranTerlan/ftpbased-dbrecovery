namespace FTPBasedSystem.API.Contracts
{
    public static class DistributionRoutes
    {
        private const string BaseRoute = "api/";

        public static class Numeric
        {
            public const string Create = BaseRoute + "numbers";
        }

        public static class Text
        {
            public const string Create = BaseRoute + "texts";
        }

        public static class Date
        {
            public const string Create = BaseRoute + "dates";
        }

        public static class ActionLog
        {
            public const string Fetch = BaseRoute + "actions";
        }
    }
}