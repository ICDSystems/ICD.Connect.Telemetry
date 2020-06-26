using System;
using ICD.Common.Logging.Activities;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Telemetry.Attributes;
using ICD.Connect.Telemetry.Providers.External;

namespace ICD.Connect.Telemetry.Providers
{
	public sealed class ActivityExternalTelemetryProvider : AbstractExternalTelemetryProvider<IActivityTelemetryProvider>
	{
		[PublicAPI("DAV-PRO")]
		[EventTelemetry("Activity")]
		public event EventHandler<GenericEventArgs<Activity>> OnActivity;

		/// <summary>
		/// Sets the parent telemetry provider that this instance extends.
		/// </summary>
		/// <param name="parent"></param>
		protected override void SetParent(IActivityTelemetryProvider parent)
		{
			base.SetParent(parent);

			if (parent == null)
				return;

			foreach (Activity activity in parent.Activities)
				OnActivity.Raise(this, new GenericEventArgs<Activity>(activity));
		}

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Subscribe(IActivityTelemetryProvider parent)
		{
			base.Subscribe(parent);

			if (parent == null)
				return;

			parent.Activities.OnActivityChanged += ActivitiesOnActivityChanged;
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Unsubscribe(IActivityTelemetryProvider parent)
		{
			base.Unsubscribe(parent);

			if (parent == null)
				return;

			parent.Activities.OnActivityChanged -= ActivitiesOnActivityChanged;
		}

		/// <summary>
		/// Called when an activity changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void ActivitiesOnActivityChanged(object sender, GenericEventArgs<Activity> eventArgs)
		{
			OnActivity.Raise(this, new GenericEventArgs<Activity>(eventArgs.Data));
		}
	}
}
