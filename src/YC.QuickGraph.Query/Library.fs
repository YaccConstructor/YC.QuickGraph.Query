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

    let fst (f, _, _) = f

    let snd (_, s, _) = s

    let trd (_, _, t) = t

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
    
    /// <summary>
    /// Initializing SimpleInputGraph from IVertexAndEdgeListGraph subclasses
    /// </summary>
    /// <param name="edgesList">List of edges in your graph</param>
    /// <param name="edgeTagToString">Function which casts edge object to string</param>
    /// <param name="parserSource"> Parser source </param>
    let InitGraph (graph : IVertexAndEdgeListGraph<_, _>) (edgeTagToString : _ -> string) (parserSource : ParserSourceGLL) = 
        let edgeTagToInt x = edgeTagToString x |> parserSource.StringToToken |> int
        let simpleGraph = new SimpleInputGraph<_>(graph.VertexCount, edgeTagToInt)
        simpleGraph.AddVerticesAndEdgeRange graph.Edges |> ignore
        simpleGraph

    /// <summary>
    /// Initializing SimpleInputGraph from edges and vertices lists
    /// </summary>
    /// <param name="edges">List of edges in your graph</param>
    /// <param name="vertices">List of vertices in your graph</param>
    /// <param name="parserSource"> Parser source </param>
    /// <param namr="tagToStr"> Function from edge object to string</param>
    let InitFromLists (edges : List<_>) (vertices : List<_>) (parserSource : ParserSourceGLL) edgeTagToString = 
        let tagToToken x = edgeTagToString x |> parserSource.StringToToken |> int
        let graph = new SimpleInputGraph<_>(vertices.Count, tagToToken)
        graph.AddVerticesAndEdgeRange edges

    let IntToString (ps : ParserSourceGLL) (value : int<token>) = 
        let _, s = ps.IntToString.TryGetValue (value |> int)
        s

    /// <summary>
    /// Method for getting SPPF from ready GLLParserSource and prepared graph
    /// </summary>
    /// <param name="parserSource">GLLParserSource</param>
    /// <param name="inputGraph">Initialized graph</param>
    let GetSPPF grammar (graph : IVertexAndEdgeListGraph<_, _>) tagToString = 
        let parserSource = PrepareGrammarFromFile grammar
        let simpleGraph = InitGraph graph tagToString parserSource
        parse parserSource simpleGraph true |> snd, parserSource
        
    let SPPFToShortestPath (sppf : SPPF) (ps : ParserSourceGLL) startVert endVert nonTermName length =
        sppf.Iterate (sppf.GetNonTermByName nonTermName ps) ps length

    let SPPFToSubgraph (sppf : SPPF) (ps : ParserSourceGLL) =
        let tagToLabel x = ps.IntToString.Item (x |> int)
        let edges = GetTerminals sppf |> Seq.map(fun x -> new ParserEdge<_>(snd x, trd x, (fst x |> tagToLabel)))
        let subgraph = new AdjacencyGraph<int, ParserEdge<_>>()
        subgraph.AddVerticesAndEdgeRange(edges) |> ignore
        subgraph

    let SPPFToPathSet (sppf : SPPF) (ps : ParserSourceGLL) length nonTermName =
        sppf.Iterate (sppf.GetNonTermByName nonTermName ps) ps length

    let SPPFToCFRelation (sppf : SPPF) nonTermName length ps = 
        let seq = sppf.Iterate (sppf.GetNonTermByName nonTermName ps) ps length
        let first = seq |> Seq.item 0
        let last = seq |> Seq.last
        (nonTermName, first, last)

    let ExecuteQuery grammar (graph : IVertexAndEdgeListGraph<_, _>) tagToString length nonTermName=
        let sppf, ps = GetSPPF grammar graph tagToString
        SPPFToPathSet sppf ps length nonTermName

    (*let GetCFRelation grammar (graph : IVertexAndEdgeListGraph<_, _>) tagToString nonTermName = 
       let parserSource = PrepareGrammarFromFile grammar
       let sppf = ExecuteQuery grammar graph tagToString
       let nonTerm = FindNonTermByName nonTermName parserSource sppf
       sppf*)

    let ExecOnMultipleGraphs (ps : ParserSourceGLL) (graphs : List<IVertexAndEdgeListGraph<_, _>>) (tagToString : _ -> string) =
        let convertedGraphsList = new List<SimpleInputGraph<_>>()
        graphs |> Seq.map (fun x -> convertedGraphsList.Add(InitGraph x tagToString ps)) |> ignore
        let sppfList = new List<SPPF>()
        //convertedGraphsList |> Seq.map (fun x -> sppfList.Add(GetSPPF ps x |> fst)) |> ignore
        sppfList

    let GetSubgraph grammar (graph : IVertexAndEdgeListGraph<_, _>) tagToString = 
        let sppf, ps = GetSPPF grammar graph tagToString
        SPPFToSubgraph sppf ps
        
    let GetCFRelation grammar (graph : IVertexAndEdgeListGraph<_, _>) tagToString nontermName = 
        let sppf, ps = GetSPPF grammar graph tagToString
        SPPFToCFRelation sppf nontermName 100 ps

