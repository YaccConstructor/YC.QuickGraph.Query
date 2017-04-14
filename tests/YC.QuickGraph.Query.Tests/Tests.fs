module YC.QuickGraph.Query.Tests

open NUnit.Framework
open AbstractAnalysis.Common
open YC.QuickGraph.Query.Library
open System
open Mono.Addins

[<assembly:AddinRoot ("YaccConstructor", "1.0")>]
do()

[<OneTimeSetUp>]
let f () = 
    AddinManager.Initialize()    
    AddinManager.Registry.Update(null)

let test (grammar : string) = 
    let ps = PrepareGrammarFromString grammar
    let graph = 
        let edges = new ResizeArray<ParserEdge<string>>()
        edges.Add(new ParserEdge<string>(0, 1, "a"))
        edges.Add(new ParserEdge<string>(1, 2, "a"))
        edges.Add(new ParserEdge<string>(2, 3, "b"))
        edges.Add(new ParserEdge<string>(3, 4, "b"))
        edges.Add(new ParserEdge<string>(0, 5, "b"))
        edges.Add(new ParserEdge<string>(5, 4, "a"))
        InitFromEdgesList edges ps id
    executeQuery ps graph

[<TestFixture>]
type ``Library with simple grammar tests`` () = 
    [<Test>]
    member this.``sppf view`` () = 
        let str = "S : a b | a S b"
        let sppf = test str
        printfn "%s" (sppf.ToString())
        Assert.AreEqual(42, 42)