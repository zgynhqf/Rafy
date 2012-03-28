namespace OEA.ORM
{
	public interface IConstraint
	{
		string Column { get; } //alias
		string Operator { get; } //=, <>, ...
		object Value { get; set; }
		
		bool HasQuery { get; }
		IQuery Query { get; }
	}
}
