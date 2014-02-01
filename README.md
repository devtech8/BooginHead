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

There's also a custom view here:

* Presentation\Nop.Web\Views\Catalog\CYO.cshtml

The CYOModel is NOT saved to the database! 

All of the data that the CYO tool saves goes into one of the directories
under Presentation\Nop.Web\App_Data

# CYO Special Handling

The catalog must include a product with the tag "CYO". Currently, this is a product called
"CYO Pacifier", with a generic image.

The Product handler of the CatalogController has this special addition:

```c#
// Special shortcut for CYO
if (product.ProductTags.Any() && product.ProductTags.First(t => t.Name == "CYO") != null)
{
	return View("CYO", model);
}
```

And the shopping cart / order summary templates here have been modified to show 
the custom pacifier that the user ordered:

* Presentation\Nop.Web\Themes\BH\Views\ShoppingCart\OrderSummary.cshtml

Search for the string "CYO Custom Code" in these files.


# Deployment and Dependencies

* You need to deploy only Presentation\Nop.Web\
* The identity that your IIS application pool uses must have 
read/write/list/create privileges on App_Data\cyo and all of
its sub-directories.
* The identity that your IIS application pool uses must have 
read/write/list/create privileges on Booginhead\Presentation\Nop.Web\Content\images
* You need to run the SQL commands below to set up the custom scheduled jobs

Run the following commands against the SQL database. This will schedule the
daily job that cleans up the old CYO upload and proof files. The job will
run hourly (every 3600 seconds) and will delete files from App_Data/cyo/proofs
and App_Data/cyo/uploads that are more than 72 hours old.

The second job sends CYO order files to PRIDE every 20 minutes. We want that 
run as a scheduled job, so it can re-send any files that might have failed.

```sql

insert into ScheduleTask (Name, Seconds, Type, Enabled, StopOnError, LastStartUTC, LastEndUTC, LastSuccessUTC)
values ('Clean up CYO files', 3600, 'Nop.Web.Models.Custom.CYOImageCleanupTask, Nop.Web', 1, 0, null, null, null)

insert into ScheduleTask (Name, Seconds, Type, Enabled, StopOnError, LastStartUTC, LastEndUTC, LastSuccessUTC)
values ('Send CYO orders to PRIDE', 1200, 'Nop.Web.Models.Custom.CYOFileTransferTask, Nop.Web', 1, 0, null, null, null)

```

# Notes...

The CYO partial currently renders on the Wishlist page.

DB connection is stored in AppData/Settings.txt

Uploaded files are stored in App_Data/cyo/uploads

TODO: Scheduled task CYOImageCleanupTask should not delete proofs that are in a shopping cart!
