namespace YC.QuickGraph.Query

open Yard.Generators.GLL
open Yard.Generators.Common.ASTGLLFSA
open Yard.Generators.GLL.ParserCommon
open AbstractAnalysis.Common
open Yard.Frontends.YardFrontend
open YC.API
open AbstractParser
open QuickGraph
open System.Collections.Generic
open YC.GLL.SPPF

module Library = 

    // Just helper functions
    let private fst (f, _, _) = f

    let private snd (_, s, _) = s

    let private trd (_, _, t) = t

    /// <summary> 
    /// Prepares grammar from .yrd file. 
    /// </summary>
    ///<param name = "grammarFile"> Path to a grammar file </param>
    let private PrepareGrammarFromFile grammarFile = 
        let fe = new YardFrontend()
        let gen = new GLL()
        generate grammarFile fe gen None Seq.empty [||] [] :?> ParserSourceGLL

    /// <summary>
    /// Prepares grammar from string.
    /// </summary>
    /// <param name="grammar">Grammar string</param>
    let private PrepareGrammarFromString grammar = 
        let fe = new YardFrontend()
        let gen = new GLL()
        GenerateFromStrToObj grammar fe gen None Seq.empty [||] :?> ParserSourceGLL
    
    /// <summary>
    /// Initializing SimpleInputGraph from IVertexAndEdgeListGraph subclasses instances
    /// </summary>
    /// <param name="edgesList">List of edges in your graph</param>
    /// <param name="edgeTagToString">Function which casts edge object to string</param>
    /// <param name="parserSource"> Parser source </param>
    let private InitGraph (graph : IVertexAndEdgeListGraph<_, _>) (edgeTagToString : _ -> string) (parserSource : ParserSourceGLL) = 
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
    let private InitFromLists (edges : List<_>) (vertices : List<_>) (parserSource : ParserSourceGLL) edgeTagToString = 
        let tagToToken x = edgeTagToString x |> parserSource.StringToToken |> int
        let graph = new SimpleInputGraph<_>(vertices.Count, tagToToken)
        graph.AddVerticesAndEdgeRange edges

    /// <summary>
    /// Casts tokenized terminal name to it's original name
    /// </summary>
    /// <param name="ps">ParserSourceGLL</param>
    /// <param name="value">terminal tokenized name</param>
    let private IntToString (ps : ParserSourceGLL) (value : int<token>) = 
        let _, s = ps.IntToString.TryGetValue (value |> int)
        s

    /// <summary>
    /// Constructs SPPF from grammar, graph and tokenize function. Returns SPPF and ParserSourceGLL
    /// </summary>
    /// <param name="grammar">Path to grammar file</param>
    /// <param name="graph">Any subclass of IVertexAndEdgeListGraph</param>
    /// <param name="tagToString">Tokenization function</param>
    let private GetSPPF grammar (graph : IVertexAndEdgeListGraph<_, _>) tagToString = 
        let parserSource = PrepareGrammarFromFile grammar
        let simpleGraph = InitGraph graph tagToString parserSource
        parse parserSource simpleGraph true |> snd, parserSource
        
    /// <summary>
    /// Returns shortest path between two vertices
    /// </summary>
    /// <param name="sppf">SPPF for iterating over it</param>
    /// <param name="ps">ParserSourceGLL</param>
    /// <param name="startVert">Initial vertice</param>
    /// <param name="endVert">Finish vertice</param>
    /// <param name="nonTermName">Name of nonterminal for starting iteration from it</param>
    /// <param name="length">Should be remove on next release</param>
    let private SPPFToShortestPath (sppf : SPPF) (ps : ParserSourceGLL) startVert endVert nonTermName =
        sppf.Iterate (sppf.GetNonTermByName nonTermName ps) ps 0

    /// <summary>
    /// Returns subgraph constructed from terminal nodes of SPPF
    /// </summary>
    /// <param name="sppf">SPPF</param>
    /// <param name="ps">ParserSourceGLL</param>
    let private SPPFToSubgraph (sppf : SPPF) (ps : ParserSourceGLL) =
        let tagToLabel x = ps.IntToString.Item (x |> int)
        let edges = GetTerminals sppf |> Seq.map(fun x -> new ParserEdge<_>(snd x, trd x, (fst x |> tagToLabel)))
        let subgraph = new AdjacencyGraph<int, ParserEdge<_>>()
        subgraph.AddVerticesAndEdgeRange(edges) |> ignore
        subgraph

    /// <summary>
    /// Returns a set of paths with presetted length
    /// </summary>
    /// <param name="sppf">SPPF</param>
    /// <param name="ps">ParserSourceGLL</param>
    /// <param name="length">Size of returned set</param>
    /// <param name="nonTermName">Name of nonterminal for starting iteration from it</param>
    let private SPPFToPathSet (sppf : SPPF) (ps : ParserSourceGLL) length nonTermName =
        sppf.Iterate (sppf.GetNonTermByName nonTermName ps) ps length

    /// <summary>
    /// Returns a context-free-relations triplets, where the first element is nonterminal, second is initial vertice and third is finish vertice
    /// </summary>
    /// <param name="sppf">SPPF</param>
    /// <param name="nonTermName">Name of nonterminal for starting iteration from it</param>
    /// <param name="length">Should be remove on next release</param>
    /// <param name="ps">ParserSourceGLL</param>
    let private SPPFToCFRelation (sppf : SPPF) nonTermName ps = 
        let seq = sppf.Iterate (sppf.GetNonTermByName nonTermName ps) ps 0
        let first = seq |> Seq.item 0
        let last = seq |> Seq.last
        (nonTermName, first, last)

    /// <summary>
    /// Executes query with specified grammar on given graph. Returns a pathset with presetted length
    /// </summary>
    /// <param name="grammar">Path to grammar file</param>
    /// <param name="graph">Any subclass of IVertexAndEdgeListGraph</param>
    /// <param name="tagToString">Tokenization function</param>
    /// <param name="length">Returned path length</param>
    /// <param name="nonTermName">Name of nonterminal for starting iteration from it</param>
    let ExecuteQuery grammarFile (graph : IVertexAndEdgeListGraph<_, _>) tagToString length nonTermName=
        let sppf, ps = GetSPPF grammarFile graph tagToString
        SPPFToPathSet sppf ps length nonTermName

    /// <summary>
    /// Allows to execute prepared query on multiple graphs. Return seq of path sets
    /// </summary>
    /// <param name="ps">ParserSourceGLL</param>
    /// <param name="graphs">List of graphs</param>
    /// <param name="tagToString">Tokenization function</param>
    let ExecOnMultipleGraphs (ps : ParserSourceGLL) (graphs : List<IVertexAndEdgeListGraph<_, _>>) (tagToString : _ -> string) =
        let convertedGraphsList = new List<SimpleInputGraph<_>>()
        graphs |> Seq.map (fun x -> convertedGraphsList.Add(InitGraph x tagToString ps)) |> ignore
        let sppfList = new List<SPPF>()
        //convertedGraphsList |> Seq.map (fun x -> sppfList.Add(GetSPPF ps x |> fst)) |> ignore
        sppfList

    /// <summary>
    /// Returns subgraph of given graph.
    /// </summary>
    /// <param name="grammarFile">Path to grammar file</param>
    /// <param name="graph">Any subclass of IVertexAndEdgeListGraph</param>
    /// <param name="tagToString">Tokenization function</param>
    let GetSubgraph grammarFile (graph : IVertexAndEdgeListGraph<_, _>) tagToString = 
        let sppf, ps = GetSPPF grammarFile graph tagToString
        SPPFToSubgraph sppf ps
        
    /// <summary>
    /// Returns context-free relation for given graph and grammar.
    /// </summary>
    /// <param name="grammarFile">Path to grammar file</param>
    /// <param name="graph">Any subclass of IVertexAndEdgeListGraph</param>
    /// <param name="tagToString">Tokenization function</param>
    /// <param name="nontermName">Name of nonterminal for starting iteration from it</param>
    let GetCFRelation grammarFile (graph : IVertexAndEdgeListGraph<_, _>) tagToString nontermName = 
        let sppf, ps = GetSPPF grammarFile graph tagToString
        SPPFToCFRelation sppf nontermName ps

    let GetShortestPath grammar graph tagToString ntName startVertice endVertice = 
        let sppf, ps = GetSPPF grammar graph tagToString
        SPPFToShortestPath sppf ps startVertice endVertice ntName

