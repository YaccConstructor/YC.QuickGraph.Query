module YC.QuickGraph.Query.Tests

open NUnit.Framework
open AbstractAnalysis.Common
open YC.QuickGraph.Query.Library
open System
open Mono.Addins
open YC.QuickGraph
open YC.GLL.SPPF
open QuickGraph
open Yard.Generators.GLL
open AbstractParser
open Yard.Generators.GLL.ParserCommon
open Yard.Frontends.YardFrontend
open YC.API
open System.IO

let test (grammar : string) = 
    let vertices = new ResizeArray<int>()
    vertices.Add(0)
    vertices.Add(1)
    vertices.Add(2)
    vertices.Add(3)
    let edges = new ResizeArray<ParserEdge<string>>()
    edges.Add(new ParserEdge<string>(0, 1, "A"))
    edges.Add(new ParserEdge<string>(1, 0, "A"))
    edges.Add(new ParserEdge<string>(1, 2, "B"))
    edges.Add(new ParserEdge<string>(2, 1, "B"))
    let graph = new QuickGraph.AdjacencyGraph<int, ParserEdge<string>>()
    for v in vertices do
        graph.AddVertex v |> ignore
    for e in edges do
        graph.AddEdge e |> ignore
    ExecuteQuery grammar graph id

let getInputGraph tokenizer inputFile =    
    let edges = 
        File.ReadAllLines (inputFile)
        |> Array.filter(fun x -> not (x = ""))
        |> Array.map (fun s -> let x = s.Split([|' '|])
                               (int x.[0]), (int x.[1]), x.[2])
    let edg (f : int) (t : string) (l : int) = 
        new ParserEdge<_>(f, l, tokenizer (t.ToUpper()) |> int) 
      
    let g = new SimpleInputGraph<_>([|0<positionInInput>|], id)
    
    [|for (first,last,tag) in edges -> edg first tag last |]
    |> g.AddVerticesAndEdgeRange
    |> ignore
    g 

let initGraph (graph : IVertexAndEdgeListGraph<_, _>) (edgeTagToString : _ -> string) (parserSource : ParserSourceGLL) = 
        let edgeTagToInt x = edgeTagToString x |> parserSource.StringToToken |> int
        let simpleGraph = new SimpleInputGraph<_>(graph.VertexCount, edgeTagToInt)
        simpleGraph.AddVerticesAndEdgeRange(graph.Edges) |> ignore
        simpleGraph

let getParserSource grammarFile conv = 
    let fe = new YardFrontend()
    let gen = new GLL()
    generate (grammarFile)
             fe gen 
             None
             conv
             [|""|]
             [] :?> ParserSourceGLL

[<TestFixture>]
type ``Library with simple grammar tests`` () = 
    [<Test>]
    member this.``sppf view`` () = 
        42