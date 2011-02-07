OpenSlx QuickForm Controls

The .vm files in the Templates folder must be copied to your project folder
under the Model\QuickForms\Web folder.  Also for each of these templates you 
must add a <WebControlRenderingProvider> node to Web.ControlConfiguration.xml
(paste the content of the ControlConfiguration.xml file).

For development, you must copy the OpenSlx.Lib.dll file to the directory of 
SageAppArchitect.exe, for example C:\PF\SalesLogix.  You must also add the OpenSlx.Lib.dll
name to the QuickFormsConfiguration.xml file:
      &lt;string&gt;OpenSlx.Lib.dll&lt;/string&gt;
Application Architect creates the XML file the first time it opens after installation. 
It places it in C:\ProgramData\Sage\Platform\Configuration\Application\SalesLogix (Vista) 
or C:\Documents and Settings\All Users\Application Data\Sage\Platform\Configuration\Application\SalesLogix (Windows 2008).
 (this is not necessary at runtime though).  

For more info refer to the "Creating Custom Controls for Quick Forms" topic in the 
Application Architect help.
