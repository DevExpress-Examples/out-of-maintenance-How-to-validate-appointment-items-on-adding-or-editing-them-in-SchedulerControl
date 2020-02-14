Imports DevExpress.Mvvm
Imports DevExpress.Xpf.Core
Imports DevExpress.Xpf.Scheduling
Imports System.Collections.Generic
Imports System.Linq

Namespace DXSample
	Public Class SchedulerValidationService
		Inherits DialogService

		Private Function ProcessAppointments(ByVal appts As IReadOnlyList(Of AppointmentItem), ByVal itemsToCancel As IList(Of AppointmentItem)) As Boolean
			Dim toCancel = New List(Of AppointmentItem)()
			For Each item In appts
				Dim range = New DateTimeRange(item.Start, item.End)
				Dim startLunchRange = New DateTimeRange(item.Start.Date.AddHours(13), item.Start.Date.AddHours(14))
				Dim endLunchRange = New DateTimeRange(item.End.Date.AddHours(13), item.End.Date.AddHours(14))
				If range.Intersect(startLunchRange).Duration.Ticks <> 0 OrElse range.Intersect(endLunchRange).Duration.Ticks <> 0 OrElse range.Duration.Hours > 23 Then
					toCancel.Add(item)
				End If
			Next item

			If toCancel.Count > 0 Then
				Dim Cancel = New UICommand() With {
					.Caption = "Cancel",
					.IsDefault = True
				}
				Dim CancelConflicts = New UICommand() With {.Caption = "Cancel Conflicts"}
				Dim Ignore = New UICommand() With {
					.Caption = "Ignore",
					.IsCancel = True
				}
				Dim result = Me.ShowDialog(New List(Of UICommand)() From {Cancel, CancelConflicts, Ignore}, "Warning", "WarningUserControl", "The following appointment(-s) intersects the lunch time:" & vbLf & vbLf & String.Join(vbLf, toCancel.Select(Function(c) c.Subject)) & vbLf & vbLf & "Click 'Cancel' to discard all changes." & vbLf & "Click 'Cancel Conflicts' to cancel changes only in these appointment(-s).")

				If result Is Cancel Then
					Return False
				End If

				If result Is CancelConflicts Then
					For Each item In toCancel
						itemsToCancel.Add(item)
					Next item
				End If
			End If
			Return True
		End Function

		Private Sub Scheduler_AppointmentAdding(ByVal sender As Object, ByVal e As AppointmentAddingEventArgs)
			e.Cancel = Not ProcessAppointments(e.Appointments, e.CanceledAppointments)
		End Sub

		Private Sub Scheduler_AppointmentEditing(ByVal sender As Object, ByVal e As AppointmentEditingEventArgs)
			e.Cancel = Not ProcessAppointments(e.EditAppointments, e.CanceledEditAppointments)
		End Sub


		Protected Overrides Sub OnAttached()
			MyBase.OnAttached()
			Dim scheduler = TryCast(Me.AssociatedObject, SchedulerControl)
			AddHandler scheduler.AppointmentEditing, AddressOf Scheduler_AppointmentEditing
			AddHandler scheduler.AppointmentAdding, AddressOf Scheduler_AppointmentAdding
		End Sub

		Protected Overrides Sub OnDetaching()
			Dim scheduler = TryCast(Me.AssociatedObject, SchedulerControl)
			RemoveHandler scheduler.AppointmentEditing, AddressOf Scheduler_AppointmentEditing
			RemoveHandler scheduler.AppointmentAdding, AddressOf Scheduler_AppointmentAdding
			MyBase.OnDetaching()
		End Sub
	End Class
End Namespace