using Microsoft.Azure.CosmosDB.Table; 

public class VmLogEntity : TableEntity  
 {  
  public VmLogEntity (string vmName, string vmSize, string creationTime)   
 {   
  this.PartitionKey = vmSize;   this.RowKey = creationTime;  
  }   
  public VmLogEntity () { }   
  //public string Email { get; set; }   
  //public string PhoneNumber { get; set; }   
}  