using AgriFleet.Core.Dtos;
using AgriFleet.Core.Enums;
using AgriFleet.Core.Validators;
using FluentAssertions;

namespace AgriFleet.Tests;

public class ValidatorTests
{
    private static readonly CreateServiceTicketRequest ValidTicket =
        new(Guid.NewGuid(), "Pompa arızası", "Basınç düşük", TicketPriority.High);

    private static readonly CreateMaintenanceRecordRequest ValidRecord =
        new(MaintenanceType.Repair, "Pompa değişti", "Servis Ekibi 1");

    private static IEnumerable<string> ErrorsOf(CreateServiceTicketRequest request) =>
        new CreateServiceTicketRequestValidator().Validate(request).Errors.Select(e => e.PropertyName);

    private static IEnumerable<string> ErrorsOf(CreateMaintenanceRecordRequest request) =>
        new CreateMaintenanceRecordRequestValidator().Validate(request).Errors.Select(e => e.PropertyName);

    [Fact]
    public void Valid_ticket_passes()
    {
        ErrorsOf(ValidTicket).Should().BeEmpty();
    }

    [Fact]
    public void Ticket_requires_a_drone()
    {
        ErrorsOf(ValidTicket with { DroneId = Guid.Empty }).Should().Contain("DroneId");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Ticket_requires_a_title(string? title)
    {
        ErrorsOf(ValidTicket with { Title = title! }).Should().Contain("Title");
    }

    [Fact]
    public void Ticket_title_length_limit_is_inclusive()
    {
        ErrorsOf(ValidTicket with { Title = new string('a', 200) }).Should().BeEmpty();
        ErrorsOf(ValidTicket with { Title = new string('a', 201) }).Should().Contain("Title");
    }

    [Fact]
    public void Ticket_rejects_an_undefined_priority()
    {
        ErrorsOf(ValidTicket with { Priority = (TicketPriority)99 }).Should().Contain("Priority");
    }

    [Fact]
    public void Valid_maintenance_record_passes()
    {
        ErrorsOf(ValidRecord).Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Maintenance_requires_a_description(string? description)
    {
        ErrorsOf(ValidRecord with { Description = description! }).Should().Contain("Description");
    }

    [Fact]
    public void Maintenance_requires_who_performed_it()
    {
        ErrorsOf(ValidRecord with { PerformedBy = "" }).Should().Contain("PerformedBy");
    }

    [Fact]
    public void Maintenance_description_length_limit_is_inclusive()
    {
        ErrorsOf(ValidRecord with { Description = new string('a', 1000) }).Should().BeEmpty();
        ErrorsOf(ValidRecord with { Description = new string('a', 1001) }).Should().Contain("Description");
    }

    [Fact]
    public void Maintenance_rejects_an_undefined_type()
    {
        ErrorsOf(ValidRecord with { Type = (MaintenanceType)99 }).Should().Contain("Type");
    }
}
