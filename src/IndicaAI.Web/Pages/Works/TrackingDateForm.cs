using IndicaAI.Web.Features.Tracking;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace IndicaAI.Web.Pages.Works;

public sealed record TrackingDateForm(DetailsModel.TrackingInput Input, DateOnly IncludedOn, DateWarning Warnings)
{
    public static bool ShowsStart(string state) => state is "in_progress" or "paused" or "completed" or "dropped";
    public static bool ShowsCompletion(string state) => state == "completed";

    public static bool Posted(IFormCollection form, ModelStateDictionary modelState, string property, bool applicable)
    {
        var key = "Input." + property;
        if (!applicable) modelState.Remove(key);
        return applicable && form.ContainsKey(key);
    }
}

public sealed record DateConfirmation(DateWarning Warnings, string Key, string Prefix = "")
{
    public static DateWarning WarningsFrom(Domain.DomainException error) => error.Arguments.OfType<DateWarning>().FirstOrDefault();
}
