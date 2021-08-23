namespace Caregivr
{
	public class ProcedureEvent
	{
		public Procedure procedure { get; protected set; }
		public string eventName { get; protected set; }
		public VariantDictionary data { get; protected set; }

		public ProcedureEvent(Procedure procedure, string eventName, VariantDictionary data)
		{
			this.procedure = procedure;
			this.eventName = eventName;
			this.data = data;
			if (this.data == null)
			{
				this.data = new VariantDictionary();
			}
		}
		
		public ProcedureEvent(Procedure procedure, string eventName) : this(procedure, eventName, null) {}

		public override string ToString()
		{
			return string.Format("[ProcedureEvent] {0} {1}", eventName, procedure!=null ? procedure.ID : "null");
		}
	}
}
