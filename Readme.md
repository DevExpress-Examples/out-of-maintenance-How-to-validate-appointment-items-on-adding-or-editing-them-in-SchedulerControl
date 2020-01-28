# How to validate appointment items when the user is adding or editing them

SchedulerControl provides the [AppointmentAdding](https://docs.devexpress.com/WPF/DevExpress.Xpf.Scheduling.SchedulerControl.AppointmentAdding) and [AppointmentEditing](https://docs.devexpress.com/WPF/DevExpress.Xpf.Scheduling.SchedulerControl.AppointmentEditing) events. You can use them to implement validation. This example illustrates how you can show a warning message to users when an appointment intersects the lunch time.

The lunch time is defined as a recurrent [Time Region Item](https://docs.devexpress.com/WPF/401378/Controls-and-Libraries/Scheduler/Time-Regions):

```xaml
<dxsch:SchedulerControl.TimeRegionItems>
    <dxsch:TimeRegionItem
            Type ="Pattern" 
            Start="1/1/2019 13:00:00" End="1/1/2019 14:00:00" 
            RecurrenceInfo="{dxsch:RecurrenceDaily Start='1/1/2019 13:00:00', ByDay=WorkDays}" 
            BrushName="{x:Static dxsch:DefaultBrushNames.TimeRegion3Hatch}" 
        />
</dxsch:SchedulerControl.TimeRegionItems>
```

The validation logic is implemented in the **SchedulerValidationService** class which is a [DialogService](https://docs.devexpress.com/WPF/17467/mvvm-framework/services/predefined-set/dialog-services/dialogservice) class descendant. If an appointment intersects the lunch time, the Scheduler displays a dialog window and allows the user to cancel changes either in all appointments or only in the conflicted appointments. Users can also click the **Ignore** button to override validation and save changes:


```cs
bool ProcessAppointments(IReadOnlyList<AppointmentItem> appts, IList<AppointmentItem> itemsToCancel)
{
    var toCancel = new List<AppointmentItem>();
    foreach (var item in appts){
        var range = new DateTimeRange(item.Start, item.End);
        var startLunchRange = new DateTimeRange(item.Start.Date.AddHours(13), item.Start.Date.AddHours(14));
        var endLunchRange = new DateTimeRange(item.End.Date.AddHours(13), item.End.Date.AddHours(14));
        if (range.Intersect(startLunchRange).Duration.Ticks != 0 
            || range.Intersect(endLunchRange).Duration.Ticks != 0
            || range.Duration.Hours > 23)
            toCancel.Add(item);
    }

    if (toCancel.Count > 0) {
        var Cancel = new UICommand() { Caption = "Cancel", IsDefault = true };
        var CancelConflicts = new UICommand() { Caption = "Cancel Conflicts" };
        var Ignore = new UICommand() { Caption = "Ignore", IsCancel = true };
        var result = this.ShowDialog(new List<UICommand>() { Cancel, CancelConflicts, Ignore },
            "Warning",
            "WarningUserControl",
            "The following appointment(-s) intersects the lunch time:\n\n"
            + string.Join("\n", toCancel.Select(c => c.Subject)) +
            "\n\nClick 'Cancel' to discard all changes.\nClick 'Cancel Conflicts' to cancel changes only in these appointment(-s).");
                
        if (result == Cancel)
            return false;

        if (result == CancelConflicts)
            foreach (var item in toCancel)
                itemsToCancel.Add(item);
    }
    return true;
}
```

When the **ProcessAppointments** method returns **False**, the **e.Cancel** property is set to **True** in the **AppointmentAdding** or **AppointmentEditing** event handlers. If the user chooses to cancel changes only for the conflicted appointments, these appointments are added to the [e.CanceledAppointments](https://docs.devexpress.com/WPF/DevExpress.Xpf.Scheduling.AppointmentAddingEventArgs.CanceledAppointments) or [e.CanceledEditAppointments](https://docs.devexpress.com/WPF/DevExpress.Xpf.Scheduling.AppointmentEditingEventArgs.CanceledEditAppointments) collections:

```cs
private void Scheduler_AppointmentAdding(object sender, AppointmentAddingEventArgs e)
{
    e.Cancel = !ProcessAppointments(e.Appointments, e.CanceledAppointments);
}

private void Scheduler_AppointmentEditing(object sender, AppointmentEditingEventArgs e)
{
    e.Cancel = !ProcessAppointments(e.EditAppointments, e.CanceledEditAppointments);
}
```

