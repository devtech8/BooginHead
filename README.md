# Booginhead

This is the Booginhead nopCommerce project.

# Customizations

This site includes a custom component called CYO (Create Your Own), 
which allows customers to design their own products.

The files for the custom CYO components are in these folders:

* Presentation\Nop.Web\App_Data\cyo
* Presentation\Nop.Web\Controllers\CYOController.cs
* Presentation\Nop.Web\Models\Custom\*.cs
* Presentation\Nop.Web\Themes\BH\Content\cyo
* Presentation\Nop.Web\Themes\BH\Scripts\cyo
* Presentation\Nop.Web\Themes\BH\Views\Catalog\CYO.cshtml
* Presentation\Nop.Web\Themes\BH\Views\Catalog\_cyo.cshtml

And the shopping cart / order summary templates here have been modified to show 
the custom pacifier that the user ordered:

* Presentation\Nop.Web\Themes\BH\Views\ShoppingCart\OrderSummary.cshtml

# What's in Models/Custom

The following items are in Presentation\Nop.Web\Models\Custom:

* CYOCartItemEventListener.cs - When a CYO item is added to a shopping cart, this
  module moves the proof image associated with the CYO item into App_Data/cyo/in_cart, 
  so that it does not get deleted by the CYOImageCleanupTask.
* CYOFileTransferTask.cs - This is a scheduled task that periodically transfers
  order files to PRIDE for fulfillment. There is some SQL in the README to add the
  scheduled task to the database. Once it's added, you can manage it in the admin
  panel of NopCommerce, under System > Scheduled Tasks.
* CYOImageCleanupTask.cs - A scheduled task that deletes old image files from 
  App_Data/proofs.
* CYOImageHelper.cs - This generates proof images.
* CYOModel.cs - This contains data about a customer's CYO design, including the size
  and color of the selected pacifier, URL and position of the background image, etc.
  It includes all data needed to render the image. This model is not saved to the 
  database, but it is saved to the file system as serialized JSON. The json filename
  has the same Guid as the proof image filename it describes. At some point, Booginhead
  may want to allow customers to re-edit recent designs. The JSON file contains enough
  data to allow this. It just has to be re-loaded into the UI.
* CYOOrderHelper.cs - This class generates the pipe-delimited text file that goes to
  PRIDE.
* CYOOrderListener.cs - This class builds the files that PRIDE needs to fulfill a CYO
  order, and copies them into App_Data/cyo/orders_unsent. In development, this class
  creates the order files when an order is placed. _In production, it should created
  the files when an order is paid._ There is a separate event for OrderPaid.
* CYOPDFHelper.cs - This class generates the PDF packing slip that is sent to PRIDE.
  PRIDE includes the packing slip in the order, which they ship directly from their
  factory to the customer.
* CYOSFTPHelper.cs - This class provides methods to send files to PRIDE.


# The App_Data Directory

The CYOModel is NOT saved to the database! 

All of the data that the CYO tool saves goes into one of the directories
under Presentation\Nop.Web\App_Data\cyo

These directories include the following:

* fonts - Contains custom fonts used by the CYO tool
* in_cart - Contains proof images that customers have put into their shopping carts
* orders_sent - Contains the PRIDE files (proof images, text order files, PDF packing slips) that been sent to PRIDE
* orders_unsent - Contains PRIDE files that should be sent to PRIDE the next time the scheduled SFTP job runs
* pdf_templates - Contains the PDF templates used to generate the PRIDE packing slips
* proofs - Contains proof images that users have generated with the CYO tool
* uploads - Contains images users have uploaded through the CYO tool. These become background images for CYO pacifiers

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

This is the only section of the core code that has been modified for CYO.

# Deployment and Dependencies

* You need to deploy only Presentation\Nop.Web\
* The identity that your IIS application pool uses must have 
read/write/list/create privileges on App_Data\cyo and all of
its sub-directories.
* The identity that your IIS application pool uses must have 
read/write/list/create privileges on Booginhead\Presentation\Nop.Web\Content\images
* You need to run the SQL commands below to set up the custom scheduled jobs


# Scheduled Jobs

Run the following commands against the SQL database. 


```sql

insert into ScheduleTask (Name, Seconds, Type, Enabled, StopOnError, LastStartUTC, LastEndUTC, LastSuccessUTC)
values ('Clean up CYO files', 3600, 'Nop.Web.Models.Custom.CYOImageCleanupTask, Nop.Web', 1, 0, null, null, null)

insert into ScheduleTask (Name, Seconds, Type, Enabled, StopOnError, LastStartUTC, LastEndUTC, LastSuccessUTC)
values ('Send CYO orders to PRIDE', 1200, 'Nop.Web.Models.Custom.CYOFileTransferTask, Nop.Web', 1, 0, null, null, null)

```

This will schedule the daily job that cleans up the old CYO upload and proof files. 
The job will run hourly (every 3600 seconds) and will delete files from App_Data/cyo/proofs
and App_Data/cyo/uploads that are more than 72 hours old.

The second job sends CYO order files to PRIDE every 20 minutes. We want that 
run as a scheduled job, so it can re-send any files that might have failed.

The settings for the job that sends files to PRIDE include information on how
to access PRIDE's SFTP server. This info is stored in the web.config file.

# Behaviors and Settings 

This document describes the CYO tool's basic front-end and back-end behavior, 
as well as the settings required for the CYO tool to work:

https://docs.google.com/a/7simplemachines.com/document/d/1M_nM5qZONpvYC-qM5qhA11k7Vdst7XLy2CH-nqwaP7U/edit#

# Notes...

DB connection is stored in AppData/Settings.txt

Uploaded files are stored in App_Data/cyo/uploads

TODO: Scheduled task CYOImageCleanupTask should not delete proofs that are in a shopping cart!
