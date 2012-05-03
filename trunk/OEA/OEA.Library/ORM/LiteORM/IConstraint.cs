namespace OEA.ORM
{
    public interface IConstraint
    {
        /// <summary>
        /// may be null
        /// </summary>
        IQuery Query { get; }

        /// <summary>
        /// alias
        /// </summary>
        string Property { get; }

        /// <summary>
        /// =, <>, ...
        /// </summary>
        string Operator { get; }

        object Value { get; set; }
    }
}