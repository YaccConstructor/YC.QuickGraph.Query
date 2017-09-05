module YC.QuickGraph.Query.Tests

open NUnit.Framework
open AbstractAnalysis.Common
open YC.QuickGraph.Query.Library
open System
open YC.QuickGraph
open YC.GLL.SPPF
open QuickGraph
open Yard.Generators.GLL
open AbstractParser
open Yard.Generators.GLL.ParserCommon
open Yard.Frontends.YardFrontend
open YC.API
open System.IO

let getInputGraph inputFile =    
    let edges = 
        File.ReadAllLines (inputFile)
        |> Array.filter(fun x -> not (x = ""))
        |> Array.map (fun s -> let x = s.Split([|' '|])
                               (int x.[0]), (int x.[1]), x.[2])
    
    let edg (f : int) (t : string) (l : int) = 
        new ParserEdge<_>(f, l, t) 

    let g = new AdjacencyGraph<_, _>()
    [|for (first,last,tag) in edges -> edg first tag last |]
    |> g.AddVerticesAndEdgeRange |> ignore
    g

let getParserSource grammarFile conv = 
    let fe = new YardFrontend()
    let gen = new GLL()
    generate (grammarFile)
             fe gen 
             None
             conv
             [|""|]
             [] :?> ParserSourceGLL 

let checkLength grammarFile graphFile tokenize pathLength ntName expectedLength = 
    let graph = getInputGraph graphFile
    let pathSet = Library.ExecuteQuery grammarFile graph tokenize pathLength ntName
    Assert.AreEqual(Seq.length pathSet, expectedLength)

let checkPaths grammarFile graphFile tokenize pathLength ntName expectedPath = 
    let graph = getInputGraph graphFile
    let pathSet = Library.ExecuteQuery grammarFile graph tokenize pathLength ntName
    Assert.AreEqual(pathSet, pathLength)

let checkSubgraphSize grammarFile graphFile tokenize expectedSize = 
    let graph = getInputGraph graphFile
    let subgraph = Library.GetSubgraph grammarFile graph tokenize
    Assert.AreEqual(subgraph.VertexCount, expectedSize)

let checkCFRelation grammarFile graphFile tokenize ntName expectedCFR = 
    let graph = getInputGraph graphFile
    let cfr = Library.GetCFRelation grammarFile graph tokenize ntName
    Assert.AreEqual(cfr, expectedCFR) 

let checkShortestPathLength grammarFile graphFile tokenize ntName startVertice endVertice expectedLength = 
    let graph = getInputGraph graphFile
    let shortest = Library.GetShortestPath grammarFile graph tokenize ntName startVertice endVertice
    Assert.AreEqual(Seq.length shortest, expectedLength)

[<TestFixture>]
type ``Library with simple grammar tests`` () = 
    [<Test>]
    member this.``pathset test`` () = 
        "..."