FoldertoSPOLib
==============
Folder to SPO is built on .Net 4.0 framework. The tool allows you to sync a folder in local drive or network drive with a document libarary in SharePoint. 

SharePoint client object model techinque is being used to push the changes to the SharePoint. 

The output of the project is a console app and windows service. The msi project also attached with the source code. 

Before you start using the tool, you would need to configure some paramerter in setting.xml. 
<?xml version="1.0" encoding="utf-8" ?>
<root>
  <FolderToWatch>
    <folder
      networklocation="LOCATION1" // The source location 
      spsite="https://SPOnlineSiteURL" // SharePoint site URL
      spLib="DOCLIBNAME1"></folder> // SharePoint document lib name
    <folder
    
    // You can add as many locations as you want  
      networklocation="LOCATION2"
      spsite="https://SPOnlineSiteURL"
      spLib="DOCLIBNAME2"></folder>
  </FolderToWatch>
  <FileExtension>
  // filter the files .. in this case the tool will only works on the three extenions below. you can add unlimted number of extentions. 
    <filter>.xls</filter>
    <filter>.xlsx</filter>
    <filter>.txt</filter>
  </FileExtension>
  
  <LogFilePath path="C:/FileWatcherLog/"> // make sure the folder exist in this location.. The tool also write logs in the windows event log
  </LogFilePath>
  // For each SP URL in the folder section, you must provide a valid SPO user account to it. 
  <SharePointAccount>
    <spaccount spsite="https://SPOnlineSiteURL" 
               username="USERNAME" 
               password="PASSWORD" 
               ency="0" //You can provide an ecrypeted password. See the crypto project for more info. ></spaccount>
  </SharePointAccount>
</root>
