# Booginhead

This is the Booginhead nopCommerce project.

# Customizations

This site includes a custom component called CYO (Create Your Own), 
which allows customers to design their own products.

The files for the custom CYO components are in these folders:

* Presentation\Nop.Web\App_Data\cyo
* Presentation\Nop.Web\Content\Custom\cyo
* Presentation\Nop.Web\Controllers\CYOController.cs
* Presentation\Nop.Web\Models\Custom\CYOModel.cs
* Presentation\Nop.Web\Scripts\Custom\cyo
* Presentation\Nop.Web\Views\Custom\cyo

(No configuration just yet ...)

The CYOModel is NOT saved to the database! 

All of the data that the CYO tool saves goes into one of the directories
under Presentation\Nop.Web\App_Data

# Deployment and Dependencies

* You need to deploy only Presentation\Nop.Web\
* The identity that your IIS application pool uses must have 
read/write/list/create privileges on App_Data\cyo and all of
its sub-directories.

Run the following command against the SQL database. This will schedule the
daily job that cleans up the old CYO upload and proof files. The job will
run hourly (every 3600 seconds) and will delete files from App_Data/cyo/proofs
and App_Data/cyo/uploads that are more than 24 hours old.

<code language="sql">

insert into ScheduleTask (Name, Seconds, Type, Enabled, StopOnError, LastStartUTC, LastEndUTC, LastSuccessUTC)
values ('Clean up CYO files', 3600, 'Nop.Web.Models.Custom.CYOScheduledTask, Nop.Web', 1, 0, null, null, null)

</code>

# Notes...

The CYO partial currently renders on the Wishlist page.

DB connection is stored in AppData/Settings.txt

Uploaded files are stored in App_Data/cyo/uploads

