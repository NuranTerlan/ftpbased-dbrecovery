using System;
using System.Collections.Generic;
using FTPBasedSystem.API.Configs;
using static System.Int32;

namespace FTPBasedSystem.API.Helpers
{
    public static class Generators
    {
        public static string CronGenerator(CronOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            TryParse(options.EverySeconds, out var seconds);
            TryParse(options.EveryMinutes, out var minutes);
            TryParse(options.EveryHours, out var hours);
            TryParse(options.EveryDays, out var days);
            TryParse(options.EveryMonths, out var months);

            var configNums = new List<int>
            {
                seconds, minutes, hours, days, months
            };

            var resultCron = "";
            foreach (var configNum in configNums)
            {
                if (configNum != 0)
                {
                    resultCron += $"*/{configNum} ";
                    continue;
                }

                resultCron += "* ";
            }

            // last asterisk is important because we want to generate second based cron
            // last asterisk represent the day of the week (0-6 or exact name)
            resultCron += "*";

            return resultCron;
        }

        public static string FirstCaseUpperStringGenerator(string word)
        {
            return word.Substring(0, 1).ToUpper() + word.Substring(1, word.Length - 1).ToLower();
        }
    }
}