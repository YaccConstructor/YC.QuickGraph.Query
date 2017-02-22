namespace YC.QuickGraph.Query

open QuickGraph
open Yard.Generators.GLL
open Yard.Frontends.YardFrontend
open Yard.Core.IL

module Library =

    let grammarToIL grammar = 
        let frontend = YardFrontend()
        frontend.ParseGrammar grammar

    