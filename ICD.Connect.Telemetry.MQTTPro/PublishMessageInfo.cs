namespace ICD.Connect.Telemetry.MQTTPro
{
	public struct PublishMessageInfo
	{
		private readonly string m_Topic;
		private readonly PublishMessage m_PublishMessage;

		/// <summary>
		/// Gets the topic.
		/// </summary>
		public string Topic { get { return m_Topic; } }

		/// <summary>
		/// Gets the publish message.
		/// </summary>
		public PublishMessage PublishMessage { get { return m_PublishMessage; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="publishMessage"></param>
		public PublishMessageInfo(string message, PublishMessage publishMessage)
		{
			m_Topic = message;
			m_PublishMessage = publishMessage;
		}
	}
}
