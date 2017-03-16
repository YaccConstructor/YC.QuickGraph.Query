namespace YC.QuickGraph.Query

open QuickGraph
open Yard.Generators.GLL
open Yard.Generators
open YC.FST.AbstractLexing
open YaccConstructor.API
open Yard.Generators.GLL.ParserCommon
open YC.QuickGraph.Query.GraphsImpl

module Library =
    
    let getParserSource (grammarFilePath : string) = 
        generate grammarFilePath "YardFrontend" "GllGenerator" None ["ExpandMeta"] [] :?> ParserSourceGLL

    let getInputGraph pathToFile (edgeObjectToString : 'EdgeObject -> string) =
        new ParserInputGraph<'EdgeObject>(3, edgeObjectToString)

    let parso parserSource input = 
        AbstractParser.parse parserSource input true 

    let doSmth grammarFilePath graphPath (edgeObjectToString : 'EdgeObject -> string)= 
        let parser = getParserSource grammarFilePath
        let input =  getInputGraph graphPath edgeObjectToString
        parso parser input

    //let SPPFToSubgraph (sppf: SPPF) =

    //let SPPFToCFRelation (sppf: SPPF) = 

    //let SPPFToPathSet (sppf: SPPF) = 