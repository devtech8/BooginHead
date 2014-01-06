# Booginhead

This is the Booginhead nopCommerce project.

# Customizations

This site includes a custom component called CYO (Create Your Own), 
which allows customers to design their own products.

The files for the custom CYO components are in these folders:

Presentation\Nop.Web\App_Data\cyo
Presentation\Nop.Web\Content\Custom\cyo
Presentation\Nop.Web\Controllers\CYOController.cs
Presentation\Nop.Web\Models\Custom\CYOModel.cs
Presentation\Nop.Web\Scripts\Custom\cyo
Presentation\Nop.Web\Views\Custom\cyo

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

Uploaded files are stored in a database table called Downloads. 
The full binary data for all uploads is stored there.

# ImageMagick for Proof Images

Install ImageMagick from here:

http://www.imagemagick.org/download/binaries/ImageMagick-6.8.7-10-Q16-x64-dll.exe

See http://magick.codeplex.com/ for .NET bindings for ImageMagick... 
but note that the project is in alpha!

# ImageMagick Commands

Here is a sample ImageMagick command to build the proof image:

First, create the image by setting Desert.jpg as the
first layer, then adding shield_white.png as a layer
on top of that, then adding SkullGirl.png as a layer
on top of that. The center portion of shield_white.png
is transparent, so the desert image shows through it.
The line -layers merge +repage merges all the layers into
a single image while setting the correct X,Y coordinates
for each layer (repaging.)

Finally, add the text on top of the image we just created,
using the specified font, size, color, etc.

All of this is saved as output.png

The -crop option crops the entire image to 674 x 479 pixels.
It starts the crop from the top left corner at X/Y 
coordinates 165/145. The binky image is 674x479 pixels.
The underlying Desert image is 1024x768. We're positioning
the shield at 165/145 so that it's center matches the center
of the Desert image. We're positioning SkullGirl in the 
center of the binky, then  cropping the resulting image to
the size of the binky. We don't need that whole 1024x768
desert image bleeding past the borders of the binky in our
output image.

<pre>
convert -crop 674x479+165+145 \
    -page +0+0 Desert.jpg \
    -page +165+145 shield_white.png \
    -page +331+236 SkullGirl.png \
    -layers merge +repage \
    -pointsize 30 \
    -font "GoogleWebFonts/Allura-Regular.ttf" \
    -fill blue \
    -gravity None \
    -annotate +200+300 'Precious Baby Snookums' \
    output.png 
</pre>
