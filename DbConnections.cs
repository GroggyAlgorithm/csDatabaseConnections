using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;


/// <summary>
/// Database connector class for making connections to database. To be inherited, not static because not everything needs to connect, no way.
/// </summary>
public static class DatabaseConnector
{

	/// <summary>
	/// Gets a connection to a database
	/// </summary>
	/// <param name="SQLConn">The sql connection variable to use</param>
	/// <param name="configurationString">The connection string</param>
	/// <returns>true if worked</returns>
	/// <exception cref="Exception">Literally any exception</exception>
	public static bool GetDBConnection(ref SqlConnection SQLConn, string configurationString)
	{

		try
		{
			if (SQLConn == null) SQLConn = new SqlConnection();
			if (SQLConn.State != ConnectionState.Open)
			{
				SQLConn.ConnectionString = configurationString;
				SQLConn.Open();
			}
			return true;
		}
		catch (Exception ex) { throw new Exception(ex.Message); }
	}
    



	/// <summary>
	/// Closes a connection to a database
	/// </summary>
	/// <param name="SQLConn">The open connection</param>
	/// <returns>True if closed</returns>
	/// <exception cref="Exception">Literally any exception</exception>
	public static bool CloseDBConnection(ref SqlConnection SQLConn)
	{
		try
		{
			if (SQLConn.State != ConnectionState.Closed)
			{
				SQLConn.Close();
				SQLConn.Dispose();
				SQLConn = null;
			}
			return true;
		}
		catch (Exception ex)
		{
			throw new Exception(ex.Message);
		}
	}



	/// <summary>
	/// Sets the parameters for sql command
	/// </summary>
	/// <param name="sqlCommand">The sql command to use</param>
	/// <param name="ParameterName">The name of the parameter</param>
	/// <param name="Value">Value for the parameter</param>
	/// <param name="ParameterType">The type of parameter</param>
	/// <param name="FieldSize">The size of the field</param>
	/// <param name="Direction">Direction for the parameters</param>
	/// <param name="Precision">The precision value for the parameters</param>
	/// <param name="Scale">The scale value for the parameters</param>
	/// <returns>0 on success</returns>
	/// <exception cref="Exception">Literally any exception</exception>
	public static int SetParameter(ref SqlCommand sqlCommand, string ParameterName, Object Value
		, SqlDbType ParameterType, int FieldSize = -1
		, ParameterDirection Direction = ParameterDirection.Input
		, Byte Precision = 0, Byte Scale = 0)
	{
		try
		{
			sqlCommand.CommandType = CommandType.StoredProcedure;
			if (FieldSize == -1)
				sqlCommand.Parameters.Add(ParameterName, ParameterType);
			else
				sqlCommand.Parameters.Add(ParameterName, ParameterType, FieldSize);

			if (Precision > 0) sqlCommand.Parameters[sqlCommand.Parameters.Count - 1].Precision = Precision;
			if (Scale > 0) sqlCommand.Parameters[sqlCommand.Parameters.Count - 1].Scale = Scale;

			sqlCommand.Parameters[sqlCommand.Parameters.Count - 1].Value = Value;
			sqlCommand.Parameters[sqlCommand.Parameters.Count - 1].Direction = Direction;

			return 0;
		}
		catch (Exception ex) { throw new Exception(ex.Message); }
	}



	/// <summary>
	/// Sets the parameters for sql command
	/// </summary>
	/// <param name="sqlDataAdapter">The sql Data adapter to use</param>
	/// <param name="ParameterName">The name of the parameter</param>
	/// <param name="Value">Value for the parameter</param>
	/// <param name="ParameterType">The type of parameter</param>
	/// <param name="FieldSize">The size of the field</param>
	/// <param name="Direction">Direction for the parameters</param>
	/// <param name="Precision">The precision value for the parameters</param>
	/// <param name="Scale">The scale value for the parameters</param>
	/// <returns>0 on success</returns>
	/// <exception cref="Exception">Literally any exception</exception>
	public static int SetParameter(ref SqlDataAdapter sqlDataAdapter, string ParameterName, Object Value
		, SqlDbType ParameterType, int FieldSize = -1
		, ParameterDirection Direction = ParameterDirection.Input
		, Byte Precision = 0, Byte Scale = 0)
	{
		try
		{
			sqlDataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
			if (FieldSize == -1)
				sqlDataAdapter.SelectCommand.Parameters.Add(ParameterName, ParameterType);
			else
				sqlDataAdapter.SelectCommand.Parameters.Add(ParameterName, ParameterType, FieldSize);

			if (Precision > 0) sqlDataAdapter.SelectCommand.Parameters[sqlDataAdapter.SelectCommand.Parameters.Count - 1].Precision = Precision;
			if (Scale > 0) sqlDataAdapter.SelectCommand.Parameters[sqlDataAdapter.SelectCommand.Parameters.Count - 1].Scale = Scale;

			sqlDataAdapter.SelectCommand.Parameters[sqlDataAdapter.SelectCommand.Parameters.Count - 1].Value = Value;
			sqlDataAdapter.SelectCommand.Parameters[sqlDataAdapter.SelectCommand.Parameters.Count - 1].Direction = Direction;

			return 0;
		}
		catch (Exception ex) { throw new Exception(ex.Message); }
	}



	/// <summary>
	/// Runs the cmd passed on the database. 
	/// </summary>
	/// <param name="cmd">The command to run</param>
	/// <param name="cmdType">The type of command it is. Defaults to CommandType.Text</param>
	/// <returns>A HashSet of the data rows returned.</returns>
	public static HashSet<DataRow> ExecSql(string cmd, string connectionString, CommandType cmdType = CommandType.Text)
	{
		//The return hash set
		HashSet<DataRow> sqlReturn = new HashSet<DataRow>();

		try
		{

			DataSet ds = new DataSet(); //Data set to use
			SqlConnection conn = new SqlConnection(); //SQL connection to use

			//If a connection could not be made to the database...
			if (!GetDBConnection(ref conn, connectionString))
			{
				//Throw a new exception
				throw new Exception("Could not connect to database");
			}

			//Create a data adapter
			SqlDataAdapter adapter = new SqlDataAdapter(cmd, conn);

			//Set the command type
			adapter.SelectCommand.CommandType = cmdType;

			//Try to fill the data set from the data adapter...
			try
			{
				adapter.Fill(ds);


				//Check for errors
				if (ds.Tables[0].Rows.Count > 0)
				{
					//Get the data rows as a hash set. Union for fast yes fast fast fast, no extra duplicates, by mem address
					sqlReturn.UnionWith(ds.Tables[0].Rows.Cast<DataRow>());
				}
			}
			catch (Exception ex2)
			{

				//SysLog.UpdateLogFile(this.ToString(), MethodBase.GetCurrentMethod().Name.ToString(), ex2.Message);
			}

			CloseDBConnection(ref conn);


		}
		catch (Exception ex)
		{
			throw new Exception(ex.Message);
		}

		return sqlReturn;
	}





	/// <summary>
	/// Example for running a stored procedure cmd passed on the database. 
	/// </summary>
	/// <param name="stored_proc_name">The stored procedure to run</param>
	/// <param name="id">The id to perform on</param>
	/// <param name="connectionString">The string for connecting to the database</param>
	/// <param name="idDbName">The id title in the DB</param>
	/// <returns>A list of the data rows returned. List great for ordering</returns>
	public static List<DataRow> ExampleExecSqlStoredProc(string stored_proc_name, int id, string connectionString, string idDbName)
	{
		List<DataRow> sqlReturn = new List<DataRow>();

		try
		{

			DataSet ds = new DataSet(); //Data set to use
			SqlConnection conn = new SqlConnection(); //SQL connection to use

			//If a connection could not be made to the database...
			if (!GetDBConnection(ref conn, connectionString)) 
			{
				//Throw a new exception
				throw new Exception("Could not connect to database");
			}

			//Create a data adapter
			SqlDataAdapter adapter = new SqlDataAdapter(stored_proc_name, conn);

			//Set the command type
			adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
			
			//Set the parameter
			SetParameter(ref adapter, idDbName, id, SqlDbType.Int);

			//Try to fill the data set from the data adapter...
			try
			{
				adapter.Fill(ds);


				//Check for errors
				if (ds.Tables[0].Rows.Count > 0)
				{
					//Get the data rows
					sqlReturn.AddRange(ds.Tables[0].Rows.Cast<DataRow>());
				}
			}
			catch (Exception ex2)
			{

				//SysLog.UpdateLogFile(this.ToString(), MethodBase.GetCurrentMethod().Name.ToString(), ex2.Message);
			}

			CloseDBConnection(ref conn);


		}
		catch (Exception ex)
		{
			throw new Exception(ex.Message);
		}

		return sqlReturn;
	}



	

	/// <summary>
	/// Gets a connection to a database
	/// </summary>
	/// <param name="SQLConn">The sql connection variable to use</param>
	/// <param name="configurationString">The connection string</param>
	/// <returns>true if worked</returns>
	/// <exception cref="Exception">Literally any exception</exception>
	public static bool GetDBConnection(ref SQLiteConnection SQLConn, string configurationString)
	{

		try
		{
			if (SQLConn == null) SQLConn = new SQLiteConnection();
			if (SQLConn.State != ConnectionState.Open)
			{
				SQLConn.ConnectionString = configurationString;
				SQLConn.Open();
			}
			return true;
		}
		catch (Exception ex) { throw new Exception(ex.Message); }
	}
    



	/// <summary>
	/// Closes a connection to a database
	/// </summary>
	/// <param name="SQLConn">The open connection</param>
	/// <returns>True if closed</returns>
	/// <exception cref="Exception">Literally any exception</exception>
	public static bool CloseDBConnection(ref SQLiteConnection SQLConn)
	{
		try
		{
			if (SQLConn.State != ConnectionState.Closed)
			{
				SQLConn.Close();
				SQLConn.Dispose();
				SQLConn = null;
			}
			return true;
		}
		catch (Exception ex)
		{
			throw new Exception(ex.Message);
		}
	}



	/// <summary>
	/// Sets the parameters for sql command
	/// </summary>
	/// <param name="sqlCommand">The sql command to use</param>
	/// <param name="ParameterName">The name of the parameter</param>
	/// <param name="Value">Value for the parameter</param>
	/// <param name="ParameterType">The type of parameter</param>
	/// <param name="FieldSize">The size of the field</param>
	/// <param name="Direction">Direction for the parameters</param>
	/// <param name="Precision">The precision value for the parameters</param>
	/// <param name="Scale">The scale value for the parameters</param>
	/// <returns>0 on success</returns>
	/// <exception cref="Exception">Literally any exception</exception>
	public static int SetParameter(ref SQLiteCommand sqlCommand, string ParameterName, Object Value
		, DbType ParameterType, int FieldSize = -1
		, ParameterDirection Direction = ParameterDirection.Input
		, Byte Precision = 0, Byte Scale = 0)
	{
		try
		{
			sqlCommand.CommandType = CommandType.StoredProcedure;
			if (FieldSize == -1)
				sqlCommand.Parameters.Add(ParameterName, ParameterType);
			else
				sqlCommand.Parameters.Add(ParameterName, ParameterType, FieldSize);

			if (Precision > 0) sqlCommand.Parameters[sqlCommand.Parameters.Count - 1].Precision = Precision;
			if (Scale > 0) sqlCommand.Parameters[sqlCommand.Parameters.Count - 1].Scale = Scale;

			sqlCommand.Parameters[sqlCommand.Parameters.Count - 1].Value = Value;
			sqlCommand.Parameters[sqlCommand.Parameters.Count - 1].Direction = Direction;

			return 0;
		}
		catch (Exception ex) { throw new Exception(ex.Message); }
	}



	/// <summary>
	/// Sets the parameters for sql command
	/// </summary>
	/// <param name="sqlDataAdapter">The sql Data adapter to use</param>
	/// <param name="ParameterName">The name of the parameter</param>
	/// <param name="Value">Value for the parameter</param>
	/// <param name="ParameterType">The type of parameter</param>
	/// <param name="FieldSize">The size of the field</param>
	/// <param name="Direction">Direction for the parameters</param>
	/// <param name="Precision">The precision value for the parameters</param>
	/// <param name="Scale">The scale value for the parameters</param>
	/// <returns>0 on success</returns>
	/// <exception cref="Exception">Literally any exception</exception>
	public static int SetParameter(ref SQLiteDataAdapter sqlDataAdapter, string ParameterName, Object Value
		, DbType ParameterType, int FieldSize = -1
		, ParameterDirection Direction = ParameterDirection.Input
		, Byte Precision = 0, Byte Scale = 0)
	{
		try
		{
			sqlDataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
			if (FieldSize == -1)
				sqlDataAdapter.SelectCommand.Parameters.Add(ParameterName, ParameterType);
			else
				sqlDataAdapter.SelectCommand.Parameters.Add(ParameterName, ParameterType, FieldSize);

			if (Precision > 0) sqlDataAdapter.SelectCommand.Parameters[sqlDataAdapter.SelectCommand.Parameters.Count - 1].Precision = Precision;
			if (Scale > 0) sqlDataAdapter.SelectCommand.Parameters[sqlDataAdapter.SelectCommand.Parameters.Count - 1].Scale = Scale;

			sqlDataAdapter.SelectCommand.Parameters[sqlDataAdapter.SelectCommand.Parameters.Count - 1].Value = Value;
			sqlDataAdapter.SelectCommand.Parameters[sqlDataAdapter.SelectCommand.Parameters.Count - 1].Direction = Direction;

			return 0;
		}
		catch (Exception ex) { throw new Exception(ex.Message); }
	}



	/// <summary>
	/// Runs the cmd passed on the database. 
	/// </summary>
	/// <param name="cmd">The command to run</param>
	/// <param name="cmdType">The type of command it is. Defaults to CommandType.Text</param>
	/// <returns>A HashSet of the data rows returned.</returns>
	public static HashSet<DataRow> ExecSqlite(string cmd, string connectionString, CommandType cmdType = CommandType.Text)
	{
		//The return hash set
		HashSet<DataRow> sqlReturn = new HashSet<DataRow>();

		try
		{

			DataSet ds = new DataSet(); //Data set to use
			SQLiteConnection conn = new SQLiteConnection(); //SQL connection to use

			//If a connection could not be made to the database...
			if (!GetDBConnection(ref conn, connectionString))
			{
				//Throw a new exception
				throw new Exception("Could not connect to database");
			}

			//Create a data adapter
			SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd, conn);

			//Set the command type
			adapter.SelectCommand.CommandType = cmdType;

			//Try to fill the data set from the data adapter...
			try
			{
				adapter.Fill(ds);


				//Check for errors
				if (ds.Tables[0].Rows.Count > 0)
				{
					//Get the data rows as a hash set. Union for fast yes fast fast fast, no extra duplicates, by mem address
					sqlReturn.UnionWith(ds.Tables[0].Rows.Cast<DataRow>());
				}
			}
			catch (Exception ex2)
			{

				//SysLog.UpdateLogFile(this.ToString(), MethodBase.GetCurrentMethod().Name.ToString(), ex2.Message);
			}

			CloseDBConnection(ref conn);


		}
		catch (Exception ex)
		{
			throw new Exception(ex.Message);
		}

		return sqlReturn;
	}




	



}