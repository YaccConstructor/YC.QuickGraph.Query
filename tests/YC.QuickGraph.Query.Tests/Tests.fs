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
open YC.GLL
open System.IO
open YC.QuickGraph.Query.BioData

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

let checkLength grammarFile graph tokenize pathLength ntName expectedLength = 
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
        checkSubgraphSize "KirillsGrammar1.yrd" "KirillsGraph1.txt" id 100000

    [<Test>]
    member this._01_SimpleSPPFTest() = 
        let vertices = new ResizeArray<int>()
        vertices.Add(0)
        vertices.Add(1)
        vertices.Add(2)
        let edges = new ResizeArray<ParserEdge<string>>()
        edges.Add(new ParserEdge<string>(0, 1, "A"))
        edges.Add(new ParserEdge<string>(1, 0, "A"))
        edges.Add(new ParserEdge<string>(1, 2, "B"))
        edges.Add(new ParserEdge<string>(2, 1, "B"))
        let graph = new QuickGraph.AdjacencyGraph<int, ParserEdge<string>>()
        graph.AddVerticesAndEdgeRange edges |> ignore
        checkLength "MyBrackets.yrd" graph id 100 "s" 100
        

    [<Test>]
    member this._02_SimpleEpsCycleSPPFTest() =
        let vertices = [|0; 1; 2; 3; 4; 5|]
        let edges = new ResizeArray<ParserEdge<string>>()
        edges.Add(new ParserEdge<string>(0, 1, "A"))
        edges.Add(new ParserEdge<string>(1, 2, "A"))
        edges.Add(new ParserEdge<string>(2, 3, "A"))
        edges.Add(new ParserEdge<string>(3, 4, "C"))
        edges.Add(new ParserEdge<string>(4, 5, "C"))
        let graph = new QuickGraph.AdjacencyGraph<int, ParserEdge<string>>()
        graph.AddVerticesAndEdgeRange edges |> ignore
        checkLength "EpsCycle.yrd" graph id 5 "a" 5