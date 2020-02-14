using DevExpress.Mvvm;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Scheduling;
using System.Collections.Generic;
using System.Linq;

namespace DXSample {
    public class SchedulerValidationService : DialogService {
        bool ProcessAppointments(IReadOnlyList<AppointmentItem> appts, IList<AppointmentItem> itemsToCancel) {
            var toCancel = new List<AppointmentItem>();
            foreach(var item in appts) {
                var range = new DateTimeRange(item.Start, item.End);
                var startLunchRange = new DateTimeRange(item.Start.Date.AddHours(13), item.Start.Date.AddHours(14));
                var endLunchRange = new DateTimeRange(item.End.Date.AddHours(13), item.End.Date.AddHours(14));
                if(range.Intersect(startLunchRange).Duration.Ticks != 0 ||
                    range.Intersect(endLunchRange).Duration.Ticks != 0 ||
                    range.Duration.Hours > 23)
                    toCancel.Add(item);
            }

            if(toCancel.Count > 0) {
                var Cancel = new UICommand() { Caption = "Cancel", IsDefault = true };
                var CancelConflicts = new UICommand() { Caption = "Cancel Conflicts" };
                var Ignore = new UICommand() { Caption = "Ignore", IsCancel = true };
                var result = this.ShowDialog(new List<UICommand>()
                    { Cancel, CancelConflicts, Ignore },
                                             "Warning",
                                             "WarningUserControl",
                                             "The following appointment(-s) intersects the lunch time:\n\n" +
                    string.Join("\n", toCancel.Select(c => c.Subject)) +
                    "\n\nClick 'Cancel' to discard all changes.\nClick 'Cancel Conflicts' to cancel changes only in these appointment(-s).");

                if(result == Cancel)
                    return false;

                if(result == CancelConflicts)
                    foreach(var item in toCancel)
                        itemsToCancel.Add(item);
            }
            return true;
        }

        private void Scheduler_AppointmentAdding(object sender, AppointmentAddingEventArgs e)
        { e.Cancel = !ProcessAppointments(e.Appointments, e.CanceledAppointments); }

        private void Scheduler_AppointmentEditing(object sender, AppointmentEditingEventArgs e)
        { e.Cancel = !ProcessAppointments(e.EditAppointments, e.CanceledEditAppointments); }


        protected override void OnAttached() {
            base.OnAttached();
            var scheduler = this.AssociatedObject as SchedulerControl;
            scheduler.AppointmentEditing += Scheduler_AppointmentEditing;
            scheduler.AppointmentAdding += Scheduler_AppointmentAdding;
        }

        protected override void OnDetaching() {
            var scheduler = this.AssociatedObject as SchedulerControl;
            scheduler.AppointmentEditing -= Scheduler_AppointmentEditing;
            scheduler.AppointmentAdding -= Scheduler_AppointmentAdding;
            base.OnDetaching();
        }
    }
}