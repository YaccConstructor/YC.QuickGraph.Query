module YC.QuickGraph.Query.Tests

open NUnit.Framework
open AbstractAnalysis.Common
open YC.QuickGraph.Query.Library
open System
open Mono.Addins
open YC.QuickGraph

[<assembly:AddinRoot ("YaccConstructor", "1.0")>]
    do()

let test (grammar : string) = 
    let ps = PrepareGrammarFromFile grammar
    let vertices = new ResizeArray<int>()
    vertices.Add(0)
    vertices.Add(1)
    vertices.Add(2)
    vertices.Add(3)
    vertices.Add(4)
    vertices.Add(5)
    let edges = new ResizeArray<ParserEdge<string>>()
    edges.Add(new ParserEdge<string>(0, 1, "A"))
    edges.Add(new ParserEdge<string>(1, 2, "A"))
    edges.Add(new ParserEdge<string>(2, 3, "B"))
    edges.Add(new ParserEdge<string>(3, 4, "B"))
    edges.Add(new ParserEdge<string>(0, 5, "B"))
    edges.Add(new ParserEdge<string>(5, 4, "A"))
    GetSPPF ps (InitFromLists edges vertices ps id)

[<TestFixture>]
type ``Library with simple grammar tests`` () = 
    [<Test>]
    member this.``sppf view`` () = 
        let str = "AnBn.yrd" (*"s: A B | A s B"*)
        let sppf = test str
        for n in sppf.TerminalNodes do
            printfn "%s" (n.Key.ToString() + " " + n.Value.ToString())
        Assert.AreEqual(42, 42)

[<EntryPoint>]
let f x =
    let g = ``Library with simple grammar tests`` ()
    g.``sppf view``()
    0