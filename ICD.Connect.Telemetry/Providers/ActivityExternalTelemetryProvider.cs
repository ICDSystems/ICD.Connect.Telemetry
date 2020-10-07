using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Logging.Activities;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Telemetry.Attributes;
using ICD.Connect.Telemetry.Providers.External;

namespace ICD.Connect.Telemetry.Providers
{
	public sealed class ActivityExternalTelemetryProvider : AbstractExternalTelemetryProvider<IActivityTelemetryProvider>
	{
		private readonly IcdHashSet<Activity> m_Activities; 
		private readonly SafeCriticalSection m_Section;

		[PublicAPI("DAV-PRO")]
		[EventTelemetry("OnActivitiesChanged")]
		public event EventHandler OnActivitiesChanged;

		[NotNull]
		[PublicAPI("DAV-PRO")]
		[PropertyTelemetry("Activities", null, "OnActivitiesChanged")]
		public IEnumerable<Activity> Activities
		{
			get { return m_Section.Execute(() => m_Activities.ToArray()); }
			private set
			{
				if (value == null)
					throw new ArgumentNullException("value");

				m_Section.Enter();

				try
				{
					IcdHashSet<Activity> set = value.ToIcdHashSet();
					if (set.SetEquals(m_Activities))
						return;

					m_Activities.Clear();
					m_Activities.AddRange(set);
				}
				finally
				{
					m_Section.Leave();
				}

				OnActivitiesChanged.Raise(this);
			}
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public ActivityExternalTelemetryProvider()
		{
			m_Activities = new IcdHashSet<Activity>();
			m_Section = new SafeCriticalSection();
		}

		/// <summary>
		/// Initializes the current telemetry state.
		/// </summary>
		public override void InitializeTelemetry()
		{
			base.InitializeTelemetry();

			UpdateActivities();
		}

		/// <summary>
		/// Updates the activities to reflect the underlying parent.
		/// </summary>
		private void UpdateActivities()
		{
			Activities =
				Parent == null
					? Enumerable.Empty<Activity>()
					: Parent.Activities;
		}

		#region Parent Callbacks

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
			UpdateActivities();
		}

		#endregion
	}
}
