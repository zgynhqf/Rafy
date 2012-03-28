using System;
using System.Collections;
using System.Collections.Generic;

namespace OEA.ORM.sqlserver
{
	public class SqlResultSet : IResultSet
	{
		protected string[] names;
		protected List<object[]> rows;
		private int currentRow = -1;
		
		public SqlResultSet(string[] columnNames)
		{
			names = columnNames ?? new string[0];
			rows = new List<object[]>();
		}
		
		public int Rows
		{
			get { return rows.Count; }
		}
		
		public int Columns
		{
			get { return names.Length; }
		}
		
		public virtual int CurrentRow
		{
			get { return currentRow; }
			set
			{
				ValidRow(value);
				currentRow = value;
			}
		}
		
		public virtual void Reset()
		{
			currentRow = -1;
		}
		
		public virtual bool Next()
		{
			int next = currentRow + 1;
			if (next < Rows)
			{
				currentRow = next;
				return true;
			}
			else
				return false;
		}
		
		public string GetName(int columnIndex)
		{
			ValidColumn(columnIndex);
			return names[columnIndex];
		}
		
		public int GetIndex(string columnName)
		{
			for (int i = 0; i < names.Length; i++)
			{
				if (names[i].Equals(columnName))
					return i;
			}
			return -1;
		}
		
		public virtual object Get(int row, int column)
		{
			ValidRow(row);
			ValidColumn(column);
			object[] datarow = rows[row];
			return datarow[column];
		}
		
		public virtual object Get(int row, string columnName)
		{
			int i = GetIndex(columnName);
			if (i == -1)
				throw new LightException(string.Format("column {0} not found", columnName));
			return Get(row, i);
		}
		
		public virtual object Get(int column)
		{
			return Get(currentRow, column);
		}
		
		public virtual object Get(string columnName)
		{
			return Get(currentRow, columnName);
		}
		
		public virtual object this[int columnIndex]
		{
			get { return Get(columnIndex); }
		}
		
		public virtual object this[string columnName]
		{
			get { return Get(columnName); }
		}
		
		private void ValidRow(int row)
		{
			if (row < 0 || row >= Rows)
				throw new LightException(
					string.Format("row index out of bounds (rowIndex={0}, rows={1})", row, Rows));
		}
		
		private void ValidColumn(int column)
		{
			if (column < 0 || column >= Columns)
				throw new LightException(
					string.Format("column index out of bounds (columnIndex={0}, columns={1})", column, Columns));
		}
		
		public virtual void AddRow(object[] row)
		{
			for (int i = 0; i < row.Length; i++)
			{
				if (DBNull.Value.Equals(row[i]))
					row[i] = null;
			}
			rows.Add(row);
		}
		
		public virtual object[] GetRow(int row)
		{
			ValidRow(row);
			return rows[row];
		}
		
		public virtual object[] GetRow()
		{
			return GetRow(currentRow);
		}
		
		public virtual IEnumerator GetEnumerator()
		{
			return rows.GetEnumerator();
		}
	}
}
