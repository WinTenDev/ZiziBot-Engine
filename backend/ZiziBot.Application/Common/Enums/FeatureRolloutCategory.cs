using System.ComponentModel;

namespace ZiziBot.Application.Common.Enums;

public enum FeatureRolloutCategory
{
    [Description("General Distribution")]
    GeneralDistribution = 0,

    [Description("Insider Preview")]
    InsiderPreview = 1,

    [Description("Premium Donation")]
    PremiumDonation = 2,
}