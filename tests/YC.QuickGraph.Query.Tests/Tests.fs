module YC.QuickGraph.Query.Tests

open NUnit.Framework
open AbstractAnalysis.Common
open YC.QuickGraph.Query.Library
open System
open Mono.Addins
open YC.QuickGraph
open YC.GLL.SPPF

[<assembly:AddinRoot ("YaccConstructor", "1.0")>]
    do()

let test (grammar : string) = 
    let vertices = new ResizeArray<int>()
    vertices.Add(0)
    vertices.Add(1)
    vertices.Add(2)
    vertices.Add(3)
    let edges = new ResizeArray<ParserEdge<string>>()
    edges.Add(new ParserEdge<string>(0, 3, "B"))
    edges.Add(new ParserEdge<string>(3, 0, "B"))
    edges.Add(new ParserEdge<string>(0, 1, "A"))
    edges.Add(new ParserEdge<string>(1, 2, "A"))
    edges.Add(new ParserEdge<string>(2, 0, "A"))
    let graph = new QuickGraph.AdjacencyGraph<int, ParserEdge<string>>()
    for v in vertices do
        graph.AddVertex v |> ignore
    for e in edges do
        graph.AddEdge e |> ignore
    ExecuteQuery grammar graph id

[<TestFixture>]
type ``Library with simple grammar tests`` () = 
    [<Test>]
    member this.``sppf view`` () = 
        let str = "AnBn.yrd" (*"s: A B | A s B"*)
        let pathset = test str
        for n in pathset do
            printfn "%s" (n.Source.ToString() + " -> " + n.Target.ToString() + " tag: " + n.Tag)
        Assert.AreEqual(42, 42)

[<EntryPoint>]
let f x =
    let g = ``Library with simple grammar tests`` ()
    g.``sppf view``()
    0