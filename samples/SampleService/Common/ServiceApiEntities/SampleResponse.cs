using System;

namespace Samples.SampleService.V1.ServiceApiEntities; 

public class SampleResponse {
    public int SomeValue { get; set; }
    public SampleEntity[] SomeCollection { get; set; }
    public string RandomValue { get; set; }
    public DateTime AFutureDate { get; set; } 
}
