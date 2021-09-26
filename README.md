# ImageMatchNet
Image signature and search  
c# implementation of the [image-match](https://github.com/ProvenanceLabs/image-match) library


## Usage

### Signature

Install `ImageMatchNet` Nuget Package
```
PM> Install-Package ImageMatchNet
```
Example
```csharp
var gis = new ImageSignature();

var sign1 = gis.GenerateSignature("path1");
var sign2 = gis.GenerateSignature("path2");

var dist = gis.NormalizedDistance(sign1, sign2);
```

### Elasticsearch Storage   
Install `ImageMatchNet.Elasticsearch` Nuget Package
```
PM> Install-Package ImageMatchNet.Elasticsearch
```
Example
```csharp
ISignatureStorage storage = new ElasticsearchSignatureStorage("http://localhost:9200");

storage.AddOrUpdateImage("iamge1", "filePath");
var matchs = storage.SearchImage("filePath");
```