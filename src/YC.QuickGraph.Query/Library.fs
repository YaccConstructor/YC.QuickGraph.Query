namespace YC.QuickGraph.Query

open Yard.Generators.GLL
open Yard.Generators.Common.ASTGLLFSA
open Yard.Generators.GLL.ParserCommon
open AbstractAnalysis.Common
open Yard.Frontends.YardFrontend
open YC.API
open AbstractParser
open Mono.Addins
open QuickGraph
open System.Collections.Generic
open YC.GLL.SPPF

module Library = 

    let frst (f, _, _) = f

    let snd (_, s, _) = s

    let trd (_, _, t) = t

    let FindNonTermByName name (ps : ParserSourceGLL) (sppf : SPPF) = 
        sppf.Nodes.Find(fun x -> x :? NonTerminalNode)

    /// <summary> 
    /// Prepares grammar from .yrd file. 
    /// </summary>
    ///<param name = "grammarFile"> Path to a grammar file </param>
    let PrepareGrammarFromFile grammarFile = 
        let fe = new YardFrontend()
        let gen = new GLL()
        generate grammarFile fe gen None Seq.empty [||] [] :?> ParserSourceGLL

    /// <summary>
    /// Prepares grammar from string.
    /// </summary>
    /// <param name="grammar">Grammar string</param>
    let PrepareGrammarFromString grammar = 
        let fe = new YardFrontend()
        let gen = new GLL()
        GenerateFromStrToObj grammar fe gen None Seq.empty [||] :?> ParserSourceGLL
    
    let PrepareGrammarWithConvFromString grammar conv = 
        let fe = new YardFrontend()
        let gen = new GLL()
        GenerateFromStrToObj grammar fe gen None conv [||] :?> ParserSourceGLL

    let PrepareGrammarWithGenParamsAndConversionsFromString grammar genParams conv = 
        let fe = new YardFrontend()
        let gen = new GLL()
        GenerateFromStrToObj grammar fe gen genParams conv [||] :?> ParserSourceGLL 
    
    /// <summary>
    /// Initializing SimpleInputGraph from IVertexAndEdgeListGraph subclasses
    /// </summary>
    /// <param name="edgesList">List of edges in your graph</param>
    /// <param name="edgeTagToString">Function which casts edge object to string</param>
    /// <param name="parserSource"> Parser source </param>
    let InitGraph (graph : IVertexAndEdgeListGraph<_, _>) (edgeTagToString : _ -> string) (parserSource : ParserSourceGLL) = 
        let edgeTagToInt x = edgeTagToString x |> parserSource.StringToToken |> int
        let simpleGraph = new SimpleInputGraph<_>(graph.VertexCount, edgeTagToInt)
        for v in graph.Vertices do
            simpleGraph.AddVertex v |> ignore
        for e in graph.Edges do
            simpleGraph.AddEdge e |> ignore
        simpleGraph

    /// <summary>
    /// Initializing SimpleInputGraph from edges and vertices lists
    /// </summary>
    /// <param name="edges">List of edges in your graph</param>
    /// <param name="vertices">List of vertices in your graph</param>
    /// <param name="parserSource"> Parser source </param>
    /// <param namr="tagToStr"> Function from edge object to string</param>
    let InitFromLists (edges : List<_>) (vertices : List<_>) (parserSource : ParserSourceGLL) tagToStr = 
        let tagToToken x = tagToStr x |> parserSource.StringToToken |> int
        let graph = new SimpleInputGraph<_>(vertices.Count, tagToToken)
        for v in vertices do
            graph.AddVertex v |> ignore
        for e in edges do
            graph.AddEdge e |> ignore
        graph

    let IntToString (ps : ParserSourceGLL) (value : int<token>) = 
        let _, s = ps.IntToString.TryGetValue (value |> int)
        s

    /// <summary>
    /// Method for getting SPPF from ready GLLParserSource and prepared graph
    /// </summary>
    /// <param name="parserSource">GLLParserSource</param>
    /// <param name="inputGraph">Initialized graph</param>
    let GetSPPF (parserSource : ParserSourceGLL) (inputGraph : SimpleInputGraph<_>) = 
        let _, parseRes, _ = parse parserSource inputGraph true
        parseRes
        
    let SPPFToShortestPath (seq) =
        42

    let SPPFToSubgraph edges (graph : IVertexAndEdgeListGraph<_, _>) =
        42

    let SPPFToPathSet (sppf: SPPF) (ps : ParserSourceGLL) = 
        let nt = sppf.Nodes.Find(fun x -> x :? NonTerminalNode) :?> NonTerminalNode
        let s = sppf.Iterate nt
        s

    let SPPFToCFRelation (seq) =
        42

    /// <summary>
    /// Executes the query and returns SPPF
    /// </summary>
    /// <param name="preparedGrammar">GLLParserSource</param>
    /// <param name="preparedGraph">Initialized graph</param>
    let ExecuteQuery grammar (graph : IVertexAndEdgeListGraph<_, _>) tagToString =
        let parserSource = PrepareGrammarFromFile grammar
        let simpleGraph = InitGraph graph tagToString parserSource
        SPPFToPathSet (GetSPPF parserSource simpleGraph) parserSource

    //let GetPathSet grammar(graph : IVertexAndEdgeListGraph<_, _>) tagToString = 
    

    let GetShortestPath grammar (graph : IVertexAndEdgeListGraph<_, _>) tagToString = 
        ExecuteQuery grammar graph tagToString |> SPPFToShortestPath

    let GetSubgraph grammar (graph : IVertexAndEdgeListGraph<_, _>) tagToString = 
        SPPFToSubgraph (ExecuteQuery grammar graph tagToString) graph

    let GetCFRelations grammar (graph : IVertexAndEdgeListGraph<_, _>) tagToString = 
        ExecuteQuery grammar graph tagToString |> SPPFToCFRelation

    (*let GetCFRelation grammar (graph : IVertexAndEdgeListGraph<_, _>) tagToString nonTermName = 
       let parserSource = PrepareGrammarFromFile grammar
       let sppf = ExecuteQuery grammar graph tagToString
       let nonTerm = FindNonTermByName nonTermName parserSource sppf
       sppf*)

    let ExecOnMultipleGraphs (ps : ParserSourceGLL) (graphs : List<IVertexAndEdgeListGraph<_, _>>) (tagToString : _ -> string) =
        let convertedGraphsList = new List<SimpleInputGraph<_>>()
        graphs |> Seq.map (fun x -> convertedGraphsList.Add(InitGraph x tagToString ps)) |> ignore
        let sppfList = new List<SPPF>()
        convertedGraphsList |> Seq.map (fun x -> sppfList.Add(GetSPPF ps x)) |> ignore
        sppfList