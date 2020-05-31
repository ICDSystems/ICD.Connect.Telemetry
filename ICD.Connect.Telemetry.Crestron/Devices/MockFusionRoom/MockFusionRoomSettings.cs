using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Telemetry.Crestron.Devices.MockFusionRoom
{
	[KrangSettings("MockFusionRoom", typeof(MockFusionRoom))]
	public sealed class MockFusionRoomSettings : AbstractDeviceSettings
	{
	}
}
