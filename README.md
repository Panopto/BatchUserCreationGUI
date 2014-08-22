Panopto-BatchUserCreationGUI
=====================

Panopto Batch User Creation GUI

The program is directed towards a test server by default. User can change this in App.config or BatchUserCreationGUI.exe.config. Simply change the server address in address property in all endpoint parameter to the user's desired server address.

Program will create users listed in the given CSV file using the given admin account and outputing any error messages on the error log.

	User Name: admin account user name
	Password: admin account password
	File: CSV file containing new user information
	Error Log: display error messages

CSV File Format: UserName,FirstName,LastName,EmailAddress
