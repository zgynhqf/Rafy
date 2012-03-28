using System.Collections;

namespace OEA.ORM
{
	public interface IResultSet : IEnumerable
	{
		int Rows { get; }
		int Columns { get; }
		int CurrentRow { get; set; }
		
		object this[int columnIndex] { get; }
		object this[string columnName] { get; }
		
		object Get(int row, int column);
		object Get(int row, string columnName);
		object Get(int column);
		object Get(string columnName);
		
		object[] GetRow();
		object[] GetRow(int row);
		
		bool Next();
		void Reset();
		
		string GetName(int columnIndex);
		int GetIndex(string columnName);
	}
}
