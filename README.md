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
* The machine must have ImageMagick installed. See below.

# Notes...

The CYO partial currently renders on the Wishlist page.

DB connection is stored in AppData/Settings.txt

Uploaded files are stored in App_Data/cyo/uploads

