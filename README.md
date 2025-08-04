Before running the project make sure you have node.js and npm installed on your machine.
Check that they are installed properly by opening the PenChecksTest.sln file and running a build on the solution.
Please use Visual Studio 2022 if possible for best compatibility and results.

You will need to install the LocalDB database tables from the Database.dacpac file included in the root directory. This can be done by following these steps:
1. In Visual Studio 2022, open View -> SQL Server Object Explorer
2. Expand "SQL Server" and look for "MSSQLLocalDB" listed under it, then right click and select "Connect" (or just double click on it).
3. Once connected, right click on the "Databases" item under the server in item 2 above, and click on "Publish data-tier application"
4. In the pop-up window, under "File on Disk" select the Database.dacpac file from the root directory of the project, and then type "LocalDB" in the database name field. Everything else can be left as-is, then click on "Publish" and click on "Yes" for any pop-ups that might show up about overwriting tables (they should be created new unless you are overwriting an existing copy of this project to reset the DB).
5. The database is now loaded and ready!

You may also need to set the Startup Configuration on your local machine by doing the following:
1. In Visual Studio on the middle of the bar at the top there should be a green play arrow that may say "Start" next to it or have a browser or something else listed, click the drop down icon to the right of it and select "Configure Startup Projects"
2. In the pop-up on the right side it may have "Single startup project" selected, if so this is incorrect--we need both projects to start at once. Select "Multiple startup projects" and in the box below make sure the "Action" on both is set to "Start", and the debug target for the client project is set to localhost with your favorite browser (the server project can be left blank here unless we want to use Swagger to test the API endpoints directly).
3. Click on "Apply" and then close the dialog box.

The project should now be ready to start with the "Start" button on the top menu.

On first run you may be asked if you want to trust the ASP.NET Core SSL Certificate, click "Yes" to continue, and "Yes" again when asked about installing the certificate in the next pop-up. 
Then click "Yes" again when asked to trust the IIS Express SSL certificate and "Yes" on the certificate install pop-up after it.

After the SSL certificates have been installed it may be necessary to end debugging and manually close the terminal window that angular created in order to get it to bind the certificates correctly. To do so hit Ctrl+C in the terminal window, then type a capital "Y" and hit enter to end the debugging session and close the window. On next launch the certificates should bind correctly and everything should work as intended.
