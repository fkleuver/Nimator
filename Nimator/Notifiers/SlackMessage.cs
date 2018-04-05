using System.Linq;
using Newtonsoft.Json;
using Nimator.Formatters;
using Nimator.Logging;

namespace Nimator.Notifiers
{
    public sealed class SlackMessage
    {
        public SlackMessage(
            [NotNull]HealthCheckResult result,
            [NotNull]IHealthCheckResultFormatter formatter)
        {
            // Because "text" will be shown (a) full-width and (b) full height without
            // a 'Show More...' link, we prefer that to use for the full description.
            // The "attachments" will then be a simpler things, drawing attention with
            // specific coloring and icons.

            Text = result.Reason
                + ":\n```" 
                + formatter.Format(result)
                + "```";

            SlackMessageAttachments = new[] 
            {
                new SlackMessageAttachment
                {
                    Text = CallToActionForLevel(result.Level),
                    Color = GetHexForLevel(result.Level)
                }
            };
        }

        public void AddAttachment(string addendum)
        {
            SlackMessageAttachments = SlackMessageAttachments.Union(new[] 
            {
                new SlackMessageAttachment
                {
                    Text = addendum,
                    Color = "#00A2E8",
                }
            }).ToArray();
        }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("attachments")]
        public SlackMessageAttachment[] SlackMessageAttachments { get; set; }

        private static string CallToActionForLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Info:
                    return ":white_check_mark: Everything is just fine!";
                case LogLevel.Warn:
                    return ":warning: Careful: warnings are errors of the future!";
                case LogLevel.Error:
                    return ":x: You really should take some action!";
                case LogLevel.Fatal:
                    return ":fire: Stuff is on fire! (Or monitoring's broken...)";
                default:
                    return ":grey_question: It is quite unclear what the status is...";
            }
        }

        private static string GetHexForLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Info:
                    return "#22B14C";
                case LogLevel.Warn:
                    return "#FFD427";
                case LogLevel.Error:
                    return "#FF5527";
                case LogLevel.Fatal:
                default:
                    return "#000000";
            }
        }
    }

    public sealed class SlackMessageAttachment
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("mrkdwn_in")]
        public string[] UseMarkdownTrigger => new[] { "text" };

        [JsonProperty("color")]
        public string Color { get; set; }
    }
}
