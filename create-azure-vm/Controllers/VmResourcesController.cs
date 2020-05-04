using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Samples.Common;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

//using Microsoft.Azure.Storage;
using Microsoft.Azure.CosmosDB.Table;
using System.Web.Http.Cors;

namespace create_azure_vm.Models
{
[Route("api/[controller]")]
//[Route("api/vmresources")]
[ApiController]
public class VmResourcesController : ControllerBase
{
 

// POST: api/VmResources
[HttpPost("sdk")]
public ActionResult<VmResources> PostSDKVmResource(VmResources vmResource)
{
    String vmName = "";

    Console.WriteLine("Inside PostSDKVmResource!!");
    
    Console.WriteLine("Inside PostSDKVmResource - vmResource.Name: "+ vmResource.Name);
    Console.WriteLine("Inside PostSDKVmResource - vmResource.LunchType: "+ vmResource.LunchType);

    if(vmResource.Name.Equals("winVM")){
       Console.WriteLine("Windows VM selected");
       vmName = createWindowsVMviaSDK();
    }else{
       Console.WriteLine("Linux VM selected");
       vmName = createLinuxVMviaSDK();

    }

    return CreatedAtAction(nameof(GetVmResources), new { id = vmResource.Id, name = vmName }, vmResource);
    
}



public String createWindowsVMviaSDK(){
    String vmName= null;

    var region = Region.USEast;
            var windowsVmName = Utilities.CreateRandomName("wVM");
            var linuxVmName = Utilities.CreateRandomName("lVM");
            //var rgName = Utilities.CreateRandomName("rgCOMV");
            var rgName = "myResourceGroup";
            var userName = "skcntr"; //REMOVE **************
            var password = "Sk1@#cntr"; //REMOVE ************

            // Authenticate
                var credentials = SdkContext.AzureCredentialsFactory.FromFile("/Users/SK/Desktop/practice/azureauth.properties");
            
            var azure = Azure
                    .Configure()
                    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                    .Authenticate(credentials)
                    .WithDefaultSubscription();

            // Print selected subscription
                Utilities.Log("Selected subscription: " + azure.SubscriptionId);

            // Prepare a creatable data disk for VM
            var dataDiskCreatable = azure.Disks.Define(Utilities.CreateRandomName("dsk-"))
                        .WithRegion(region)
                        .WithExistingResourceGroup(rgName)
                        .WithData()
                        .WithSizeInGB(2);
                        //WithSizeInGB(100);

            // Create a data disk to attach to VM
                //
                var dataDisk = azure.Disks.Define(Utilities.CreateRandomName("dsk-"))
                        .WithRegion(region)
                        .WithNewResourceGroup(rgName)
                        .WithData()
                        .WithSizeInGB(2)
                        .Create();
            
            
            Utilities.Log("Creating a Windows VM");

            //var t1 = new DateTime();

            var windowsVM = azure.VirtualMachines.Define(windowsVmName)
                        .WithRegion(region)
                        .WithNewResourceGroup(rgName)
                        .WithNewPrimaryNetwork("10.0.0.0/28")
                        .WithPrimaryPrivateIPAddressDynamic()
                        .WithoutPrimaryPublicIPAddress()
                        .WithPopularWindowsImage(KnownWindowsVirtualMachineImage.WindowsServer2012R2Datacenter)
                        .WithAdminUsername(userName)
                        .WithAdminPassword(password)
                        .WithNewDataDisk(2) 
                        .WithNewDataDisk(dataDiskCreatable)
                        .WithExistingDataDisk(dataDisk)
                        .WithSize(VirtualMachineSizeTypes.StandardD3V2)
                        .Create();

            vmName = windowsVM.Name;

            //Microsoft.Azure.Storage.CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=aztabledemostorageacct;AccountKey=riQb1KJXk3c8IEZVGpKWvh1/dObB5oWS4uNrZ8AvZl5FamcfQvSverE6y1TE3L5PoN7TuoBLrFsjP2Ku9lbgFA==;EndpointSuffix=core.windows.net"); 
            var account = Microsoft.Azure.Storage.CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=aztabledemostorageacct;AccountKey=6YPEdzhaXf0wfOBSPdG8U1LOw/euckNcWVNI8nVc9mPsdjgfjSOsElp098Ss7cIjjXwc28OZmI69HQ3O0ADdCNRq91kQ==;EndpointSuffix=core.windows.net"); 
            var client = account.CreateCloudTableClient(); 
            var table = client.GetTableReference("VmLog");
            string vmSize = windowsVM.Size.ToString();
            string time = System.DateTime.Now.ToString();
            
            table.CreateIfNotExistsAsync();
            VmLogEntity vmLogEntity = new VmLogEntity(vmName, vmSize, time);
            TableOperation insertOperation = TableOperation.Insert(vmLogEntity);  
            table.ExecuteAsync(insertOperation);

            var query = new TableQuery<VmLogEntity>();
            var lst= table.ExecuteQuery(query);
         
    return vmName;
}

public String createLinuxVMviaSDK(){
    String vmName= null;

    //var region = Region.USWestCentral;
    var region = Region.USEast;
            var windowsVmName = Utilities.CreateRandomName("wVM");
            var linuxVmName = Utilities.CreateRandomName("lVM");
            //var rgName = Utilities.CreateRandomName("rgCOMV");
            var rgName = "myResourceGroup";
            var userName = "skcntr"; //REMOVE **************
            var password = "Sk1@#cntr"; //REMOVE ************

            // Authenticate
                var credentials = SdkContext.AzureCredentialsFactory.FromFile("/Users/SK/Desktop/practice/azureauth.properties");
            
            var azure = Azure
                    .Configure()
                    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                    .Authenticate(credentials)
                    .WithDefaultSubscription();

            // Print selected subscription
                Utilities.Log("Selected subscription: " + azure.SubscriptionId);

            // Prepare a creatable data disk for VM
            var dataDiskCreatable = azure.Disks.Define(Utilities.CreateRandomName("dsk-"))
                        .WithRegion(region)
                        .WithExistingResourceGroup(rgName)
                        .WithData()
                        .WithSizeInGB(2);

            // Create a data disk to attach to VM
                //
                var dataDisk = azure.Disks.Define(Utilities.CreateRandomName("dsk-"))
                        .WithRegion(region)
                        .WithNewResourceGroup(rgName)
                        .WithData()
                        .WithSizeInGB(2)
                        .Create();
            
            
            
             // Get the network where Windows VM is hosted
                //var network = windowsVM.GetPrimaryNetworkInterface().PrimaryIPConfiguration.GetNetwork();
            
                //=============================================================
                // Create a Linux VM in the same virtual network

                Utilities.Log("Creating a Linux VM in the network");

                var linuxVM = azure.VirtualMachines.Define(linuxVmName)
                        .WithRegion(region)
                        .WithNewResourceGroup(rgName)
                        .WithNewPrimaryNetwork("10.0.0.0/28")
                        .WithPrimaryPrivateIPAddressDynamic()
                        .WithoutPrimaryPublicIPAddress()

                        .WithPopularLinuxImage(KnownLinuxVirtualMachineImage.UbuntuServer16_04_Lts)
                        .WithRootUsername(userName)
                        .WithRootPassword(password)
                        .WithSize(VirtualMachineSizeTypes.StandardD3V2)
                        .Create();

                Utilities.Log("Created a Linux VM (in the same virtual network): " + linuxVM.Id);
                Utilities.PrintVirtualMachine(linuxVM);
                vmName = linuxVM.Name;

            var account = Microsoft.Azure.Storage.CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=aztabledemostorageacct;AccountKey=6YPEdzhaXf0wfOBSPdG8U1LOw/euckNcWVNI8nVc9mPsdjgfjSOsElp098Ss7cIjjXwc28OZmI69HQ3O0ADdCNRq91kQ==;EndpointSuffix=core.windows.net"); 
            var client = account.CreateCloudTableClient(); 
            var table = client.GetTableReference("VmLog");
            string vmSize = linuxVM.Size.ToString();
            string time = System.DateTime.Now.ToString();
            
            table.CreateIfNotExistsAsync();
            VmLogEntity vmLogEntity = new VmLogEntity(vmName, vmSize, time);
            TableOperation insertOperation = TableOperation.Insert(vmLogEntity);  
            table.ExecuteAsync(insertOperation);

            var query = new TableQuery<VmLogEntity>();
            var lst= table.ExecuteQuery(query);

    
    return vmName;
}



// POST: api/VmResources
[HttpPost("template")]
public ActionResult<VmResources> PostTemplateVmResource(VmResources vmResource)
{
    Console.WriteLine("Inside PostTemplateVmResource!!");
    String vmName = "";
    
    Console.WriteLine("Inside PostSDKVmResource - vmResource.Name: "+ vmResource.Name);
    Console.WriteLine("Inside PostSDKVmResource - vmResource.LunchType: "+ vmResource.LunchType);

    if(vmResource.Name.Equals("winVM")){
       Console.WriteLine("Windows VM selected");
       vmName = createWindowsVMviaTemplate();
    }else{
       Console.WriteLine("Linux VM selected");
       vmName = createLinuxVMviaTemplate();

    }

    return CreatedAtAction(nameof(GetVmResources), new { id = vmResource.Id, name = vmName }, vmResource);
}

public String createWindowsVMviaTemplate(){

    Console.WriteLine("Inside createWindowsVMviaTemplate........");

    String vmName= null;

    

            // Authenticate
            var credentials = SdkContext.AzureCredentialsFactory.FromFile("/Users/SK/Desktop/practice/azureauth.properties");
            
            var azure = Azure
                    .Configure()
                    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                    .Authenticate(credentials)
                    .WithDefaultSubscription();

            // Print selected subscription
                Utilities.Log("Selected subscription: " + azure.SubscriptionId);

                var groupName = "myResourceGroup";
                var location = Region.USEast;

                var resourceGroup = azure.ResourceGroups.Define(groupName)
                        .WithRegion(location)
                        .Create();

                string storageAccountName = SdkContext.RandomResourceName("st", 10);

Console.WriteLine("Creating storage account...");
var storage = azure.StorageAccounts.Define(storageAccountName)
    .WithRegion(Region.USEast)
    .WithExistingResourceGroup(resourceGroup)
    .Create();

var storageKeys = storage.GetKeys();
string storageConnectionString = "DefaultEndpointsProtocol=https;"
    + "AccountName=" + storage.Name
    + ";AccountKey=" + storageKeys[0].Value
    + ";EndpointSuffix=core.windows.net";

var account = CloudStorageAccount.Parse(storageConnectionString);
var serviceClient = account.CreateCloudBlobClient();

Console.WriteLine("Creating container...");
var container = serviceClient.GetContainerReference("templates");
container.CreateIfNotExistsAsync().Wait();
var containerPermissions = new BlobContainerPermissions()
    { PublicAccess = BlobContainerPublicAccessType.Container };
container.SetPermissionsAsync(containerPermissions).Wait();

Console.WriteLine("Uploading template file...");
var templateblob = container.GetBlockBlobReference("CreateVMTemplate_Win.json");
templateblob.UploadFromFileAsync("..\\..\\CreateVMTemplate_Win.json");

Console.WriteLine("Uploading parameters file...");
var paramblob = container.GetBlockBlobReference("Parameters.json");
paramblob.UploadFromFileAsync("..\\..\\Parameters.json");


var templatePath = "https://github.com/sukant0203/azure-demo/blob/master/azure-demo/CreateVMTemplate_Win.json";
var paramPath = "https://github.com/sukant0203/azure-demo/blob/master/azure-demo/Parameters.json";
var deployment = azure.Deployments.Define("myDeployment")
    .WithExistingResourceGroup(groupName)
    .WithTemplateLink(templatePath, "1.0.0.0")
    .WithParametersLink(paramPath, "1.0.0.0")
    .WithMode(Microsoft.Azure.Management.ResourceManager.Fluent.Models.DeploymentMode.Incremental)
    .Create();

    var tableaccount = Microsoft.Azure.Storage.CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=aztabledemostorageacct;AccountKey=6YPEdzhaXf0wfOBSPdG8U1LOw/euckNcWVNI8nVc9mPsdjgfjSOsElp098Ss7cIjjXwc28OZmI69HQ3O0ADdCNRq91kQ==;EndpointSuffix=core.windows.net"); 
            var client = tableaccount.CreateCloudTableClient(); 
            var table = client.GetTableReference("VmLog");
            string vmSize = "";
            string time = System.DateTime.Now.ToString();
            
            table.CreateIfNotExistsAsync();
            VmLogEntity vmLogEntity = new VmLogEntity(vmName, vmSize, time);
            TableOperation insertOperation = TableOperation.Insert(vmLogEntity);  
            table.ExecuteAsync(insertOperation);

            var query = new TableQuery<VmLogEntity>();
            var lst= table.ExecuteQuery(query);

    return vmName;
}

public String createLinuxVMviaTemplate(){

    Console.WriteLine("Inside createLinuxVMviaTemplate....");

    String vmName= null;

    

            // Authenticate
            var credentials = SdkContext.AzureCredentialsFactory.FromFile("/Users/SK/Desktop/practice/azureauth.properties");
            
            var azure = Azure
                    .Configure()
                    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                    .Authenticate(credentials)
                    .WithDefaultSubscription();

            // Print selected subscription
                Utilities.Log("Selected subscription: " + azure.SubscriptionId);

                var groupName = "myResourceGroup";
                var location = Region.USEast;

                var resourceGroup = azure.ResourceGroups.Define(groupName)
                        .WithRegion(location)
                        .Create();

                string storageAccountName = SdkContext.RandomResourceName("st", 10);

Console.WriteLine("Creating storage account...");
var storage = azure.StorageAccounts.Define(storageAccountName)
    .WithRegion(Region.USEast)
    .WithExistingResourceGroup(resourceGroup)
    .Create();

var storageKeys = storage.GetKeys();
string storageConnectionString = "DefaultEndpointsProtocol=https;"
    + "AccountName=" + storage.Name
    + ";AccountKey=" + storageKeys[0].Value
    + ";EndpointSuffix=core.windows.net";

var account = CloudStorageAccount.Parse(storageConnectionString);
var serviceClient = account.CreateCloudBlobClient();

Console.WriteLine("Creating container...");
var container = serviceClient.GetContainerReference("templates");
container.CreateIfNotExistsAsync().Wait();
var containerPermissions = new BlobContainerPermissions()
    { PublicAccess = BlobContainerPublicAccessType.Container };
container.SetPermissionsAsync(containerPermissions).Wait();

Console.WriteLine("Uploading template file...");
var templateblob = container.GetBlockBlobReference("CreateVMTemplate_Lin.json");
templateblob.UploadFromFileAsync("..\\..\\CreateVMTemplate_Lin.json");

Console.WriteLine("Uploading parameters file...");
var paramblob = container.GetBlockBlobReference("Parameters.json");
paramblob.UploadFromFileAsync("..\\..\\Parameters.json");


var templatePath = "https://github.com/sukant0203/azure-demo/blob/master/azure-demo/CreateVMTemplate_Lin.json";
var paramPath = "https://github.com/sukant0203/azure-demo/blob/master/azure-demo/Parameters.json";
var deployment = azure.Deployments.Define("myDeployment")
    .WithExistingResourceGroup(groupName)
    .WithTemplateLink(templatePath, "1.0.0.0")
    .WithParametersLink(paramPath, "1.0.0.0")
    .WithMode(Microsoft.Azure.Management.ResourceManager.Fluent.Models.DeploymentMode.Incremental)
    .Create();

    var tableaccount = Microsoft.Azure.Storage.CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=aztabledemostorageacct;AccountKey=6YPEdzhaXf0wfOBSPdG8U1LOw/euckNcWVNI8nVc9mPsdjgfjSOsElp098Ss7cIjjXwc28OZmI69HQ3O0ADdCNRq91kQ==;EndpointSuffix=core.windows.net"); 
            var client = tableaccount.CreateCloudTableClient(); 
            var table = client.GetTableReference("VmLog");
            string vmSize = "";
            string time = System.DateTime.Now.ToString();
            
            table.CreateIfNotExistsAsync();
            VmLogEntity vmLogEntity = new VmLogEntity(vmName, vmSize, time);
            TableOperation insertOperation = TableOperation.Insert(vmLogEntity);  
            table.ExecuteAsync(insertOperation);

            var query = new TableQuery<VmLogEntity>();
            var lst= table.ExecuteQuery(query);

    
    return vmName;
}





// GET: api/VmResources
[HttpGet]
public ActionResult GetVmResources()
{
    Console.WriteLine("Inside GetVmResources!!");

    return Ok(new {
        value = "VM Resource List",
        link = "some resource link"
    });
}



}
}

