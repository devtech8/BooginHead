# Booginhead

This is the Booginhead nopCommerce project.

# Customizations

This site includes a custom component called CYO (Create Your Own), which allows customers to design their own products.

The files for the custom CYO components are in these folders:

Presentation\Nop.Web\Content\Custom\cyo
Presentation\Nop.Web\Scripts\Custom\cyo
Presentation\Nop.Web\Views\Custom\cyo

(No controller just yet...)

(No configuration just yet ...)

# Notes...

The CYO partial currently renders on the Wishlist page.

DB connection is stored in AppData/Settings.txt

Uploaded files are stored in a database table called Downloads. The full binary data for all uploads is stored there.

# ImageMagick for Proof Images

Install ImageMagick from here:

http://www.imagemagick.org/download/binaries/ImageMagick-6.8.7-10-Q16-x64-dll.exe

See http://magick.codeplex.com/ for .NET bindings for ImageMagick... but note that the project is in alpha!
