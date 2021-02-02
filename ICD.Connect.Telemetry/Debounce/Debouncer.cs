using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Timers;

namespace ICD.Connect.Telemetry.Debounce
{
	public sealed class Debouncer<T> : IDisposable
	{
		private readonly eDebounceMode m_Mode;
		private readonly long m_Interval;
		private readonly T m_LowValue;
		private readonly IEqualityComparer<T> m_Comparer;
		private readonly SafeTimer m_Timer;
		private readonly Action<T> m_ValueChangedCallback;
		private readonly SafeCriticalSection m_CriticalSection;

		private T m_PreviousValue;
		private T m_PreviousRaisedValue;
		private bool m_HasPreviousValue;
		private bool m_HasPreviousRaisedValue;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="interval"></param>
		/// <param name="lowValue"></param>
		/// <param name="comparer"></param>
		/// <param name="valueChangedCallback"></param>
		public Debouncer(eDebounceMode mode, long interval, [CanBeNull] T lowValue,
		                 [NotNull] IEqualityComparer<T> comparer, [NotNull] Action<T> valueChangedCallback)
		{
			if (comparer == null)
				throw new ArgumentNullException("comparer");

			if (valueChangedCallback == null)
				throw new ArgumentNullException("valueChangedCallback");

			if (interval < 0)
				throw new ArgumentOutOfRangeException();

			m_Mode = mode;
			m_Interval = interval;
			m_LowValue = lowValue;
			m_Comparer = comparer;
			m_Timer = SafeTimer.Stopped(TimerCallback);
			m_ValueChangedCallback = valueChangedCallback;
			m_CriticalSection = new SafeCriticalSection();
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		public void Dispose()
		{
			m_Timer.Dispose();
		}

		/// <summary>
		/// Enqueues the value for debouncing.
		/// </summary>
		/// <param name="value"></param>
		public void Enqueue(T value)
		{
			m_CriticalSection.Enter();

			try
			{
				if (m_HasPreviousValue && m_Comparer.Equals(value, m_PreviousValue))
					return;

				bool debounce = false;

				switch (m_Mode)
				{
					case eDebounceMode.None:
						break;

					case eDebounceMode.All:
						debounce = true;
						break;

					case eDebounceMode.RisingEdge:
						debounce = !m_Comparer.Equals(value, m_LowValue);
						break;

					case eDebounceMode.FallingEdge:
						debounce = m_Comparer.Equals(value, m_LowValue);
						break;

					default:
						throw new ArgumentOutOfRangeException();
				}

				m_PreviousValue = value;
				m_HasPreviousValue = true;

				if (debounce && m_Interval > 0)
				{
					m_Timer.Reset(m_Interval);
					return;
				}
			}
			finally
			{
				m_CriticalSection.Leave();
			}

			// Raise the value immediately
			TimerCallback();
		}

		/// <summary>
		/// Called when the debounce timer elapses.
		/// </summary>
		private void TimerCallback()
		{
			m_CriticalSection.Enter();

			try
			{
				m_Timer.Stop();

				// Don't raise the same thing twice in a row
				if (m_HasPreviousRaisedValue && m_Comparer.Equals(m_PreviousRaisedValue, m_PreviousValue))
					return;

				m_PreviousRaisedValue = m_PreviousValue;
				m_HasPreviousRaisedValue = true;
			}
			finally
			{
				m_CriticalSection.Leave();
			}

			m_ValueChangedCallback(m_PreviousRaisedValue);
		}
	}
}
