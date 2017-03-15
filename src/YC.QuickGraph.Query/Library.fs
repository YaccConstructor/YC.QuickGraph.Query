namespace YC.QuickGraph.Query

open QuickGraph
open Yard.Generators.GLL
open Yard.Core.IL
open YC.GLL.SPPF
open YC.FST.AbstractLexing
open AbstractAnalysis.Common

module Library =

    let generateSmth (graph : ParserInputGraph<_>) (grammar : string) = 
        let generator = Yard.Generators.GLL.GLL()

    //let SPPFToSubgraph (sppf: SPPF) =

    //let SPPFToCFRelation (sppf: SPPF) = 

    //let SPPFToPathSet (sppf: SPPF) = 