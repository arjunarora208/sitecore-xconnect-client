# Introduction
The purpose of this application is to show how you can connect Sitecore XConnect using its XConnect Client side API. This application focuses on showing what configuration is requred to connect to XConnect and then how we can extract data, spefically interactions and contact data from xDB.

Target: Sitecore version is 10.1.0 with Asp.net MVC 5 application 4.8 Framework.

# Pre-requisites
To test this application make sure your XConnect is working and ssl certificate is valid.

# Getting Started
Once you have cloned the repository, the first thing is to configure web.config

### Step 1 : Configure XConnect Endpoints
 ```
<connectionStrings>
   <add name="sitecore.reporting.client" connectionString="{YourXConnectSiteUrl}" />
   <add name="sitecore.reporting.client.certificate" connectionString="StoreName=My;StoreLocation=LocalMachine;FindType=FindByThumbprint;FindValue=ac6947554c4a0409dddee7d251f75ec154a8a386" />
   <add name="XConnectClient" connectionString="{YourXConnectSiteUrl}/odata" />
   <add name="XConnectclientConfig" connectionString="{YourXConnectSiteUrl}/configuration" />
 </connectionStrings>
```

Example, in my case, i have configured my xconnect at https://localsc10xconnect.dev.local

### Step 2 : Project Build and Restore Packages
Once above step is done, build the project and resolves the dependencies if raised. Currently my local sitecore instance was configured with Sitecore version 10.1.1, so same version of packages were installed. You can upgrade the version as per your need.

Below are packages which are required:
 ```
<package id="Sitecore.XConnect" version="10.1.1" targetFramework="net48" />
<package id="Sitecore.XConnect.Client" version="10.1.1" targetFramework="net48" />
<package id="Sitecore.XConnect.Collection.Model" version="10.1.1" targetFramework="net48" />
<package id="Sitecore.XConnect.Core" version="10.1.1" targetFramework="net48" />
<package id="Sitecore.XConnect.Diagnostics" version="10.1.1" targetFramework="net48" />
<package id="Sitecore.XConnect.Search" version="10.1.1" targetFramework="net48" />
<package id="Sitecore.Xdb.Common.Web" version="10.1.1" targetFramework="net48" />
 ```
### Step 3 : Run Application and Debug Options
You can simply run the application by pressing f5 and go to https://{hosturl}/ManageContacts/FetchCount in browser to get the interaction count during specified period of time.
You can debug the application, XConnectClientReader class will create the trust connection with XConnect.

> options.AllowInvalidClientCertificates = "true"// This option is set to true but can be configured to false if you want to ignore the ssl verification
> ManageContactsController is the main controller where interactions and contacts are fetching with all facets
> Batching has been implemented with batch size of 5, but can be configured as per need

### Step 4 : Basic info on controller endpoints

Below are the 3 endpoints created:

> https://{hosturl}/ManageContacts/FetchCount\
> https://{hosturl}/ManageContacts/Process\
> https://{hosturl}/ManageContacts/ResumeProcess

# Summary
This is a very basic app created to show how we can connect to xconnect and traverse interactions and contacts. You can further customize this application according to your needs. This app was created when Sitecore 10.1.0 was released but can be used with its later versions as well.
