namespace YC.QuickGraph.Query

open Yard.Generators.GLL
open Yard.Generators.GLL.ParserCommon
open AbstractAnalysis.Common
open YaccConstructor.API
open AbstractParser
open System.Collections.Generic
open System.Linq
open Mono.Addins

module Library = 
    
    [<assembly:AddinRoot ("YaccConstructor", "1.0")>]
    do()

    AddinManager.Initialize()    
    AddinManager.Registry.Update(null)
    
    /// <summary> 
    /// Prepares grammar from .yrd file. 
    /// </summary>
    ///<param name = "grammarFile"> Path to a grammar file </param>
    let PrepareGrammarFromFile grammarFile = 
        generate grammarFile "YardFrontend" "GllGenerator" None ["ExpandMeta"] [] :?> ParserSourceGLL

    /// <summary>
    /// Prepares grammar from string.
    /// </summary>
    /// <param name="grammar">Grammar string</param>
    let PrepareGrammarFromString grammar = 
        GenerateFromStrToObj grammar "YardFrontend" "GllGenerator" None ["ExpandMeta"] :?> ParserSourceGLL

    /// <summary>
    /// Prepares a graph for parsing
    /// </summary>
    /// <param name="edgesList">A list of edges in your graph</param>
    /// <param name="edgeTagToString">Function which casts edge object to string</param>
    /// <param name="parserSource"> Prepared grammar </param>
    let InitGraph (graph : QuickGraph.AdjacencyGraph<_, _>) (edgeTagToString : _ -> string) (parserSource : ParserSourceGLL) = 
        let edgeTagToInt x = edgeTagToString x |> parserSource.StringToToken |> int
        let simpleGraph = new SimpleInputGraph<_>(graph.Edges.Count(), edgeTagToInt)
        for e in graph.Edges do
            simpleGraph.AddEdge e |> ignore
        for v in graph.Vertices do
            simpleGraph.AddVertex v |> ignore
        simpleGraph

    let InitFromEdgesList (edges : List<_>) (parserSource : ParserSourceGLL) tagToStr = 
        let tagToToken x = tagToStr x |> parserSource.StringToToken |> int
        let graph = new SimpleInputGraph<_>(edges.Count, tagToToken)
        for e in edges do
            graph.AddEdge e |> ignore
        graph


    /// <summary>
    /// Method for getting SPPF from ready GLLParserSource and prepared graph
    /// </summary>
    /// <param name="parserSource">GLLParserSource</param>
    /// <param name="inputGraph">Initialized graph</param>
    let GetSPPF (parserSource : ParserSourceGLL) (inputGraph : SimpleInputGraph<_>) = 
        let parseRes = parse parserSource inputGraph true |> snd
        match parseRes with
        | Some a -> a
        | None -> failwith("error with sppf")

    /// <summary>
    /// Executes the query and returns SPPF (probably here should be cast to smth)
    /// </summary>
    /// <param name="preparedGrammar">GLLParserSource</param>
    /// <param name="preparedGraph">Initialized graph</param>
    let executeQuery preparedGrammar preparedGraph =
        GetSPPF preparedGrammar preparedGraph

    let execQuery grammar (graph : QuickGraph.AdjacencyGraph<_, _>) tagToString = 
        let parserSource = PrepareGrammarFromString grammar
        let simpleGraph = InitGraph graph tagToString parserSource
        GetSPPF parserSource simpleGraph

    //let SPPFToShortestPath (sppf: SPPF) = 
        