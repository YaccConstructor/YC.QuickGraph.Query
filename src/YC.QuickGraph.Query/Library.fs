namespace YC.QuickGraph.Query

open QuickGraph
open Yard.Generators.GLL
open Yard.Generators
open YC.GLL.SPPF
open YC.FST.AbstractLexing
open YaccConstructor.API
open Yard.Generators.GLL.ParserCommon

module Library =
    
    let getParserSource (grammarFilePath : string) = 
        generate grammarFilePath "YardFrontend" "GllGenerator" None ["ExpandMeta"] [] :?> ParserSourceGLL

    let getInputGraph pathToFile (edgeObjectToString : 'EdgeObject -> string) =
        new ParserInputGraph<'EdgeObject>(42, edgeObjectToString)

    (*let doSmth grammarFilePath = 
        let parser = getParserSource grammarFilePath
        let input =  *)
    //let SPPFToSubgraph (sppf: SPPF) =

    //let SPPFToCFRelation (sppf: SPPF) = 

    //let SPPFToPathSet (sppf: SPPF) = 