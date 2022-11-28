namespace MemoRandom.Client.Common.Enums
{
    public static class Periods
    {
        public static string[] GetPeriodValues(PeriodTypes type)
        {
            switch(type)
            {
                case PeriodTypes.Years:
                    return new string[] { "год", "года", "лет" };

                case PeriodTypes.Months:
                    return new string[] { "месяц", "месяца", "месяцев" };

                case PeriodTypes.Days:
                    return new string[] { "день", "дня", "дней" };

                case PeriodTypes.Hours:
                    return new string[] { "час", "часа", "часов" };

                case PeriodTypes.Minutes:
                    return new string[] { "минута", "минуты", "минут" };

                case PeriodTypes.Seconds:
                    return new string[] { "секунда", "секунды", "секунд" };

                default:
                    return null;
            }
        }
    }
}
