#if SIMPLSHARP
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro;
using ICD.Common.Utils;
using ICD.Connect.Panels.SigCollections;
using ICD.Connect.Protocol.Sigs;

namespace ICD.Connect.Analytics.FusionPro
{
	public abstract class AbstractFusionSigCollectionAdapter<TAdapter, T> : ISigCollectionBase<TAdapter>
		where TAdapter : ISig
	{
		private readonly CrestronCollection<T> m_Collection;
		private readonly Func<T, TAdapter> m_Factory;

		private readonly Dictionary<uint, TAdapter> m_SigAdapterNumberCache;
		private readonly SafeCriticalSection m_CacheSection;

		#region Properties

		/// <summary>
		/// Get the sig with the specified number.
		/// </summary>
		/// <param name="sigNumber">Number of the sig to return.</param>
		/// <returns/>
		/// <exception cref="T:System.IndexOutOfRangeException">Invalid Sig Number specified.</exception>
		public TAdapter this[uint sigNumber]
		{
			get
			{
				m_CacheSection.Enter();

				try
				{
					if (!m_SigAdapterNumberCache.ContainsKey(sigNumber))
					{
						uint offset = sigNumber - FusionRoomAdapter.SIG_OFFSET;

						if (!m_Collection.Contains(offset))
						{
							string message = string.Format("Key {0} not present in {1}", sigNumber, m_Collection.GetType().Name);
							throw new IndexOutOfRangeException(message);
						}

						T sig = m_Collection[offset];
						TAdapter adapter = m_Factory(sig);

						// Number is 0 when trying to get an output sig from an input collection (or vice versa).
						if (adapter.Number == 0)
						{
							string message = string.Format("Key {0} not present in {1}", sigNumber, m_Collection.GetType().Name);
							throw new IndexOutOfRangeException(message);
						}

						m_SigAdapterNumberCache[sigNumber] = adapter;
					}

					return m_SigAdapterNumberCache[sigNumber];
				}
				finally
				{
					m_CacheSection.Leave();
				}
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractFusionSigCollectionAdapter(CrestronCollection<T> collection, Func<T, TAdapter> factory)
		{
			m_Collection = collection;
			m_Factory = factory;

			m_SigAdapterNumberCache = new Dictionary<uint, TAdapter>();
			m_CacheSection = new SafeCriticalSection();
		}

		#endregion

		public IEnumerator<TAdapter> GetEnumerator()
		{
			return GetSigs().ToList().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private IEnumerable<TAdapter> GetSigs()
		{
			List<TAdapter> temp = new List<TAdapter>();

			foreach (uint key in m_Collection.Keys)
			{
				TAdapter output;
				
				try
				{
					uint sigNumber = key + FusionRoomAdapter.SIG_OFFSET;
					output = this[sigNumber];
				}
				catch (IndexOutOfRangeException)
				{
					continue;
				}
				catch (KeyNotFoundException)
				{
					continue;
				}

				temp.Add(output);
			}

			return temp;
		}
	}
}
#endif
