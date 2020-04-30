using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Telemetry.Crestron.Devices
{
	[KrangSettings("MockFusionRoom", typeof(MockFusionRoom.MockFusionRoom))]
	public sealed class MockFusionRoomSettings : AbstractDeviceSettings
	{
	}
}
