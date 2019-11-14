# dynamic mvc crud
A dynamic interface for CRUD operations for any table in the database.

# Requirements

There are multiple master tables in the application. Need a common interface to perform CRUD on them. Please note that these master tables will not contain any linked data. 

# Solution 

As per the requirements, we need everything dynamic. We just need to pass a table name and its listing/add/edit/delete operations should be ready. While doing some research, I stumbled on a component called jTable which gives an interface for CRUD dynamically. It works with JSON data.

As a first step, JSON data was needed to define the columns. Once columns are defined, the next step is to define the various methods which can provide listing, add, edit and delete operations. 

The current demo is using asp.net MVC. It has 2 layers, presentation and data layer. Data layer is dynamically constructing queries and passing data to the table for rendering and other operations. 

Here's a quick explanation of different methods in data layer 

1. GetJsonFields : It accepts a tablename and dynamically detect its primary key, so that it should not be edited or displayed. It has also a section where it skips some fields like createdby, editedby etc as these fields are just use for auditing. It returns a JSON string.

2. GetListOfRecords : It accepts parameters like tableName, startIndex, pageSize and sortField. So, this method returns the listing of records based on the startindex and pagesize. As Jtable gives a way to sort the data, so sortField is used for controlling the order in ascending or descending. During creation of JSON string, it also detects the type of the column to treat it in string as date, string, number etc.

3. AddRecord/UpdateRecord : Based on the values posted in the controller by Jtable Add or update method, all the values used and an insert or update statement is constructed dynamically, which is executed on the specified table. 

Its a quick demo, and may not be using the best practices, but it provides a base to prepare a common interface to deal with tables dynamically. 

To setup the project, create a database and run the script provided in App_Data folder. It will create a table and insert some records into it. Go to web.config and update the string. Run the code and see the results. 

Happy Programming !!!!!
