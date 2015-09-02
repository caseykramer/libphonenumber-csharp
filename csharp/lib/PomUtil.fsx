#r @"..\..\packages\FSharp.Data\lib\net40\FSharp.Data.dll"
#r @"System.Xml.Linq"

module internal PomTool = 
    open FSharp.Data
    
    let getProjectVersion () =
        let pom = XmlProvider<"https://raw.githubusercontent.com/caseykramer/libphonenumber-csharp/master/java/pom.xml" ,InferTypesFromValues=true>.GetSample()
        pom.Version.Replace("-SNAPSHOT","")

let getPomProjectVersion() = PomTool.getProjectVersion()
        

    