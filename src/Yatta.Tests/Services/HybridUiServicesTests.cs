namespace Yatta.Tests.Services;

using Microsoft.Extensions.DependencyInjection;
using Moq;
using Yatta.App.Services;
using Yatta.Core.Interfaces;
using Yatta.Core.Models;

/// <summary>Tests the actions shared by all hybrid windows.</summary>
[Collection("Localization")]
public class HybridUiServicesTests
{
    /// <summary>Switching an activity closes the current entry and refreshes every view.</summary>
    [Fact]
    public async Task StartOrSwitchAsync_ClosesCurrentEntryAndPublishesChange()
    {
        Guid activityId = Guid.NewGuid();
        TimeRecord current = new()
        {
            Id = Guid.NewGuid(),
            ActivityId = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = TimeOnly.FromDateTime(DateTime.Now.AddMinutes(-10))
        };
        Mock<IActivityRepository> activities = new();
        activities.Setup(repository => repository.GetByIdAsync(activityId))
            .ReturnsAsync(new Activity { Id = activityId, Active = true });
        Mock<ITimeRecordRepository> records = new();
        records.Setup(repository => repository.GetActiveAsync()).ReturnsAsync(current);
        records.Setup(repository => repository.AddAsync(It.IsAny<TimeRecord>()))
            .ReturnsAsync((TimeRecord record) => record);
        Mock<INotificationService> notifications = new();
        UiEventService events = new();
        int refreshes = 0;
        events.DataChanged += (_, _) => refreshes++;
        ServiceCollection services = new();
        services.AddScoped(_ => activities.Object);
        services.AddScoped(_ => records.Object);
        using ServiceProvider provider = services.BuildServiceProvider();
        TimeEntryService entries = new(provider.GetRequiredService<IServiceScopeFactory>(), events, notifications.Object);

        await entries.StartOrSwitchAsync(activityId, true, "Changed from tray");

        Assert.NotNull(current.EndTime);
        records.Verify(repository => repository.UpdateAsync(current), Times.Once);
        records.Verify(repository => repository.AddAsync(It.Is<TimeRecord>(record =>
            record.ActivityId == activityId && record.Telework && record.Notes == "Changed from tray")), Times.Once);
        notifications.Verify(service => service.ResetTimer(), Times.Once);
        Assert.Equal(1, refreshes);
    }

    /// <summary>A manual entry cannot overlap a running entry on the same day.</summary>
    [Fact]
    public async Task SaveAsync_RejectsOverlapWithRunningEntry()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        TimeRecord proposed = new() { ActivityId = Guid.NewGuid(), Date = today, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 0) };
        TimeRecord running = new() { Id = Guid.NewGuid(), ActivityId = Guid.NewGuid(), Date = today, StartTime = new TimeOnly(9, 0) };
        Mock<ITimeRecordRepository> records = new();
        records.Setup(repository => repository.GetByDateAsync(today)).ReturnsAsync([running]);
        Mock<IValidationService> validation = new();
        validation.Setup(service => service.ValidateNoOverlap(proposed, It.IsAny<IEnumerable<TimeRecord>>())).Returns(true);
        ServiceCollection services = new();
        services.AddScoped(_ => records.Object);
        services.AddScoped(_ => validation.Object);
        using ServiceProvider provider = services.BuildServiceProvider();
        TimeEntryService entries = new(provider.GetRequiredService<IServiceScopeFactory>(), new UiEventService(), new Mock<INotificationService>().Object);

        ArgumentException error = await Assert.ThrowsAsync<ArgumentException>(() => entries.SaveAsync(proposed));

        Assert.StartsWith("Validation_OverlappingRecord", error.Message);
        records.Verify(repository => repository.AddAsync(It.IsAny<TimeRecord>()), Times.Never);
    }

    /// <summary>All subscribers see one data event, including separate windows.</summary>
    [Fact]
    public async Task UiEvents_NotifyEveryWindowAndResolveCloseDecision()
    {
        UiEventService events = new();
        int mainRefreshes = 0;
        int trayRefreshes = 0;
        events.DataChanged += (_, _) => mainRefreshes++;
        events.DataChanged += (_, _) => trayRefreshes++;
        events.PublishDataChanged();
        events.CloseDecisionRequested += (_, _) => events.AnswerCloseDecision(CloseDecision.StopAndClose);
        events.UpdateDecisionRequested += (_, _) => events.AnswerUpdateDecision(true);

        CloseDecision answer = await events.AskCloseDecisionAsync();
        bool install = await events.AskUpdateDecisionAsync();

        Assert.Equal(1, mainRefreshes);
        Assert.Equal(1, trayRefreshes);
        Assert.Equal(CloseDecision.StopAndClose, answer);
        Assert.True(install);
    }

    /// <summary>A language change updates resource text and broadcasts once.</summary>
    [Fact]
    public void Localization_ChangesBetweenSpanishAndCatalan()
    {
        using ServiceProvider provider = new ServiceCollection().BuildServiceProvider();
        LocalizationService localization = new(provider);
        BlueprintLocalizer blueprint = new(localization);
        int changes = 0;
        localization.CultureChanged += (_, _) => changes++;
        try
        {
            localization.SetCulture("es-ES");
            Assert.Equal("Guardar", localization.GetString("Button_Save"));
            Assert.Equal("Siguiente", blueprint["Pagination.Next"]);
            localization.SetCulture("ca-ES");
            Assert.Equal("Desar", localization.GetString("Button_Save"));
            Assert.Equal("Següent", blueprint["Pagination.Next"]);
            Assert.InRange(changes, 1, 2);
        }
        finally
        {
            localization.SetCulture(null);
        }
    }
}

/// <summary>Serializes tests that change process-wide culture.</summary>
[CollectionDefinition("Localization", DisableParallelization = true)]
public sealed class LocalizationCollection
{
}
