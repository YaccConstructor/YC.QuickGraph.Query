namespace YC.QuickGraph.Query

open Yard.Generators.GLL
open YaccConstructor.API
open Yard.Generators.GLL.ParserCommon
open AbstractAnalysis.Common

module Library =
    
    let PrepareGrammarFromFile grammarFilePath = 
        generate grammarFilePath "YardFrontend" "GllGenerator" None ["ExpandMeta"] [] :?> ParserSourceGLL

    //let PrepareGrammarFromString (grammarStr : string) = 
    //Add after release

    let InitGraph (*adjList*) tagToToken =
        //Add graph creation after release
        new SimpleGraphInput<_>(tagToToken)

    let Parse parserSource input = 
        AbstractParser.parse parserSource input true 

    let executeQuery grammarFilePath tagToToken = 
        let parser = PrepareGrammarFromFile grammarFilePath
        let input = InitGraph tagToToken
        Parse parser input |> snd
        
    
    //let SPPFToSubgraph (sppf: YC.GLL.SPPF.SPPF) =
    
    //let SPPFToCFRelation (sppf: SPPF) = 

    //let SPPFToPathSet (sppf: SPPF) = 