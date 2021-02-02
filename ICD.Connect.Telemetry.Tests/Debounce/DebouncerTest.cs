using System.Collections.Generic;
using ICD.Common.Utils;
using ICD.Connect.Telemetry.Debounce;
using NUnit.Framework;

namespace ICD.Connect.Telemetry.Tests.Debounce
{
	[TestFixture]
	public sealed class DebouncerTest
	{
		[Test]
		public void DebounceNoneTest()
		{
			List<int> output = new List<int>();

			using (Debouncer<int> debouncer = new Debouncer<int>(eDebounceMode.None, 1000, 0,
			                                                     EqualityComparer<int>.Default,
			                                                     v => output.Add(v)))
			{
				debouncer.Enqueue(0);
				debouncer.Enqueue(1);
				debouncer.Enqueue(2);
				debouncer.Enqueue(3);
				debouncer.Enqueue(4);
			}

			Assert.AreEqual(5, output.Count);
			Assert.AreEqual(0, output[0]);
			Assert.AreEqual(1, output[1]);
			Assert.AreEqual(2, output[2]);
			Assert.AreEqual(3, output[3]);
			Assert.AreEqual(4, output[4]);
		}

		[Test]
		public void DebounceAllTest()
		{
			List<int> output = new List<int>();

			using (Debouncer<int> debouncer = new Debouncer<int>(eDebounceMode.All, 1000, 0,
			                                                     EqualityComparer<int>.Default,
			                                                     v => output.Add(v)))
			{
				debouncer.Enqueue(0);
				debouncer.Enqueue(1);
				ThreadingUtils.Sleep(1500);
				debouncer.Enqueue(2);
				debouncer.Enqueue(3);
				ThreadingUtils.Sleep(1500);
				debouncer.Enqueue(4);
			}

			Assert.AreEqual(2, output.Count);
			Assert.AreEqual(1, output[0]);
			Assert.AreEqual(3, output[1]);
		}

		[Test]
		public void DebounceRisingEdgeTest()
		{
			List<bool> output = new List<bool>();

			using (Debouncer<bool> debouncer = new Debouncer<bool>(eDebounceMode.RisingEdge, 1000, false,
			                                                       EqualityComparer<bool>.Default,
			                                                       v => output.Add(v)))
			{
				debouncer.Enqueue(false); // Start offline
				debouncer.Enqueue(true); // Go online
				ThreadingUtils.Sleep(500);
				debouncer.Enqueue(false); // Oscillate quickly back to offline
				ThreadingUtils.Sleep(1500);
				debouncer.Enqueue(true); // Go online
				ThreadingUtils.Sleep(1500);
				debouncer.Enqueue(false); // Stays online for more than threshold, then offline again
			}

			Assert.AreEqual(3, output.Count);
			Assert.AreEqual(false, output[0]); // We get the initial value immediately
			Assert.AreEqual(true, output[1]); // We get the next value once the timer elapses
			Assert.AreEqual(false, output[2]); // We get the next value immediately
		}

		[Test]
		public void DebounceFallingEdgeTest()
		{
			List<bool> output = new List<bool>();

			using (Debouncer<bool> debouncer = new Debouncer<bool>(eDebounceMode.FallingEdge, 1000, false,
			                                                       EqualityComparer<bool>.Default,
			                                                       v => output.Add(v)))
			{
				debouncer.Enqueue(true); // Start online
				debouncer.Enqueue(false); // Go offline
				ThreadingUtils.Sleep(500);
				debouncer.Enqueue(true); // Oscillate quickly back to online
				ThreadingUtils.Sleep(1500);
				debouncer.Enqueue(false); // Go offline
				ThreadingUtils.Sleep(1500);
				debouncer.Enqueue(true); // Stays offline for more than threshold, then online again
			}

			Assert.AreEqual(3, output.Count);
			Assert.AreEqual(true, output[0]); // We get the initial value immediately
			Assert.AreEqual(false, output[1]); // We get the next value once the timer elapses
			Assert.AreEqual(true, output[2]); // We get the next value immediately
		}
	}
}
