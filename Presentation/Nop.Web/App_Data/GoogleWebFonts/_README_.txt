This directory contains TrueType fonts that match the fonts used on the 
front-end CYO tool. When the back end creates a proof of the user's 
customized binky, it needs these fonts to reproduce what the user built 
on the front end.

Here's how to get new Google Web Fonts in True Type format (.ttf):

Step 1. Request the web font from Google with a browser or a tool like wget
or curl. The web font URLs are listed in 
Booginhead/Presentation/Nop.Web/Views/Custom/CYO/_cyo.cshtml.

Example:

curl "http://fonts.googleapis.com/css?family=ABeeZee:400,700,400italic,700italic"

This returns the following:

@font-face {
  font-family: 'ABeeZee';
  font-style: normal;
  font-weight: 400;
  src: local('ABeeZee'), local('ABeeZee-Regular'), url(http://themes.googleusercontent.com/static/fonts/abeezee/v1/JYPhMn-3Xw-JGuyB-fEdNA.ttf) format('truetype'
);
}
@font-face {
  font-family: 'ABeeZee';
  font-style: italic;
  font-weight: 400;
  src: local('ABeeZee Italic'), local('ABeeZee-Italic'), url(http://themes.googleusercontent.com/static/fonts/abeezee/v1/gdFHbjHcLYROorto_Tq2E6CWcynf_cDxXwCLxiixG1c.ttf) format('truetype');
}

Step 2. Request the url shown in the src attribute of each font and save it
using the "local" name. If the local name does not end with -Regular, -Italic,
-Bold, or -BoldItalic, then append -Regular to the ttf file name.

Example:

curl "http://themes.googleusercontent.com/static/fonts/abeezee/v1/JYPhMn-3Xw-JGuyB-fEdNA.ttf" > GoogleWebFonts/ABeeZee-Regular.ttf
curl "http://themes.googleusercontent.com/static/fonts/abeezee/v1/gdFHbjHcLYROorto_Tq2E6CWcynf_cDxXwCLxiixG1c.ttf" > GoogleWebFonts/ABeeZee-Italic.ttf

As of 12/17/2013, we are using only the -Regular fonts, and CodaCaption was 
renamed from CodaCaption-Heavy to CodaCaption-Regular so the proof tool would
have an easier time finding it.