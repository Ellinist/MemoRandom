namespace MemoRandom.Client.Common.Enums
{
    public static class Scopes
    {
        public static string[] GetPeriodValues(ScopeTypes type)
        {
            switch(type)
            {
                case ScopeTypes.Years:
                    return new string[] { "год", "года", "лет" };

                case ScopeTypes.Months:
                    return new string[] { "месяц", "месяца", "месяцев" };

                case ScopeTypes.Days:
                    return new string[] { "день", "дня", "дней" };

                case ScopeTypes.Hours:
                    return new string[] { "час", "часа", "часов" };

                case ScopeTypes.Minutes:
                    return new string[] { "минута", "минуты", "минут" };

                case ScopeTypes.Seconds:
                    return new string[] { "секунда", "секунды", "секунд" };

                case ScopeTypes.Men:
                    return new string[] { "человек", "человека", "человек" };

                default:
                    return null;
            }
        }
    }
}
